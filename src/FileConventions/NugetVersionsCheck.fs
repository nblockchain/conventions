module NugetVersionsCheck

open System
open System.IO
open System.Linq
open System.Text.RegularExpressions
open System.Xml
open System.Xml.Linq
open System.Xml.XPath

open Microsoft.Build.Construction

type ScriptTarget =
    | Solution of FileInfo
    | Folder of DirectoryInfo

module MapHelper =
    let GetKeysOfMap(map: Map<'Key, 'Value>) : seq<'Key> =
        map |> Map.toSeq |> Seq.map fst

    let MergeIntoMap<'Key, 'Value when 'Key: comparison>
        (from: seq<'Key * 'Value>)
        : Map<'Key, seq<'Value>> =
        let keys = from.Select(fun (key, _value) -> key)

        let keyValuePairs =
            seq {
                for theKey in keys do
                    let valsForKey =
                        (from.Where(fun (aKey, _) -> theKey = aKey))
                            .Select(fun (_, aValue) -> aValue)
                        |> seq

                    yield theKey, valsForKey
            }

        keyValuePairs |> Map.ofSeq

    let MergeMaps<'Key, 'Value when 'Key: comparison and 'Value: comparison>
        (map1: Map<'Key, Set<'Value>>)
        (map2: Map<'Key, Set<'Value>>)
        : Map<'Key, Set<'Value>> =
        map2
        |> Map.fold
            (fun acc key value ->
                let combinedVersions =
                    Set.union
                        (acc |> Map.tryFind key |> Option.defaultValue Set.empty)
                        value

                acc |> Map.add key combinedVersions
            )
            map1

[<StructuralEquality; StructuralComparison>]
type private PackageInfo =
    {
        PackageId: string
        PackageVersion: string
        ReqReinstall: Option<bool>
    }

type private DependencyHolder =
    {
        Name: string
    }

[<CustomComparison; CustomEquality>]
type private ComparableFileInfo =
    {
        File: FileInfo
    }

    member self.DependencyHolderName: DependencyHolder =
        if self.File.FullName.ToLower().EndsWith ".nuspec" then
            {
                Name = self.File.Name
            }
        else
            {
                Name =
                    self.File.Directory.Name
                    + (Path.DirectorySeparatorChar.ToString())
            }

    interface IComparable with
        member self.CompareTo obj =
            match obj with
            | null -> self.File.FullName.CompareTo null
            | :? ComparableFileInfo as other ->
                self.File.FullName.CompareTo other.File.FullName
            | _ -> invalidArg "obj" "not a ComparableFileInfo"

    override self.Equals obj =
        match obj with
        | :? ComparableFileInfo as other ->
            self.File.FullName.Equals other.File.FullName
        | _ -> false

    override self.GetHashCode() =
        self.File.FullName.GetHashCode()

// this seems to be a bug in Microsoft.Build nuget library, FIXME: report
let normalizeDirSeparatorsPaths(path: string) : string =
    path
        .Replace('\\', Path.DirectorySeparatorChar)
        .Replace('/', Path.DirectorySeparatorChar)

let private GetAllPackageInfos
    (packages: Map<ComparableFileInfo, seq<PackageInfo>>)
    =
    let pkgInfos =
        seq {
            for KeyValue(_, pkgs) in packages do
                for pkg in pkgs do
                    yield pkg
        }

    Set pkgInfos

let rec private findProjectFiles(sol: FileInfo) : seq<FileInfo> =
    let parsedSolution = SolutionFile.Parse sol.FullName

    seq {
        for projPath in
            (parsedSolution
                .ProjectsInOrder
                .Select(fun proj ->
                    normalizeDirSeparatorsPaths proj.AbsolutePath
                )
                .ToList()) do
            if projPath.ToLower().EndsWith ".fsproj"
               || projPath.ToLower().EndsWith ".csproj" then
                yield (FileInfo projPath)
    }

let private notPackagesFolder
    (nugetSolutionPackagesDir: DirectoryInfo)
    (dir: DirectoryInfo)
    : bool =
    dir.FullName <> nugetSolutionPackagesDir.FullName

let private notSubmodule
    (currentDirectory: string)
    (dir: DirectoryInfo)
    : bool =
    let getSubmoduleDirsForThisRepo() : seq<DirectoryInfo> =
        if File.Exists ".gitmodules" then
            let regex = Regex("path\s*=\s*([^\s]+)")

            seq {
                for regexMatch in regex.Matches(File.ReadAllText ".gitmodules") do
                    let submoduleFolderRelativePath =
                        regexMatch.Groups.[1].ToString()

                    let submoduleFolder =
                        DirectoryInfo(
                            Path.Combine(
                                currentDirectory,
                                submoduleFolderRelativePath
                            )
                        )

                    yield submoduleFolder
            }
        else
            Seq.empty

    not(
        getSubmoduleDirsForThisRepo()
            .Any(fun d -> dir.FullName = d.FullName)
    )

let rec private findNuspecFiles
    (currentDirectory: string)
    (nugetSolutionPackagesDir: DirectoryInfo)
    (sol: FileInfo)
    (dir: DirectoryInfo)
    : seq<FileInfo> =
    dir.Refresh()

    seq {
        for file in dir.EnumerateFiles() do
            if (file.Name.ToLower()).EndsWith ".nuspec" then
                yield file

        for subdir in
            dir
                .EnumerateDirectories()
                .Where(notSubmodule currentDirectory)
                .Where(notPackagesFolder nugetSolutionPackagesDir) do
            for file in
                findNuspecFiles
                    currentDirectory
                    nugetSolutionPackagesDir
                    sol
                    subdir do
                yield file
    }

let private GetPackagesInfoFromProjectFile(projectFile: FileInfo) =
    let xmlDoc = XDocument.Load projectFile.FullName
    let query = "//PackageReference"
    let pkgReferences = xmlDoc.XPathSelectElements query

    seq {
        for pkgReference in pkgReferences do
            let id =
                pkgReference
                    .Attributes()
                    .Single(fun attr ->
                        attr.Name.LocalName = "Include"
                        || attr.Name.LocalName = "Update"
                    )
                    .Value

            let version =
                pkgReference
                    .Attributes()
                    .Single(fun attr -> attr.Name.LocalName = "Version")
                    .Value

            yield
                {
                    File = projectFile
                },
                {
                    PackageId = id
                    PackageVersion = version
                    ReqReinstall = None
                }
    }

let private getPackageTree
    (currentDirectory: string)
    (nugetSolutionPackagesDir: DirectoryInfo)
    (sol: FileInfo)
    : Map<ComparableFileInfo, seq<PackageInfo>> =
    let projectFiles = findProjectFiles sol

    let projectElements =
        seq {
            for projectFile in projectFiles do
                yield! GetPackagesInfoFromProjectFile projectFile
        }
        |> List.ofSeq

    let solDir = sol.Directory
    solDir.Refresh()

    let nuspecFiles =
        findNuspecFiles currentDirectory nugetSolutionPackagesDir sol solDir

    let nuspecFileElements =
        seq {
            for nuspecFile in nuspecFiles do
                let xmlDoc = XDocument.Load nuspecFile.FullName

                let nsOpt =
                    let nsString = xmlDoc.Root.Name.Namespace.ToString()

                    if String.IsNullOrEmpty nsString then
                        None
                    else
                        let nsManager = XmlNamespaceManager(NameTable())
                        let nsPrefix = "x"
                        nsManager.AddNamespace(nsPrefix, nsString)

                        if nsString
                           <> "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd" then
                            Console.Error.WriteLine
                                "Warning: the namespace URL doesn't match expectations, nuspec's XPath query may result in no elements"

                        Some(nsManager, sprintf "%s:" nsPrefix)

                let query = "//{0}dependency"

                let dependencies =
                    match nsOpt with
                    | None ->
                        let fixedQuery = String.Format(query, String.Empty)

                        xmlDoc.XPathSelectElements fixedQuery
                    | Some(nsManager, nsPrefix) ->
                        let fixedQuery = String.Format(query, nsPrefix)

                        xmlDoc.XPathSelectElements(fixedQuery, nsManager)

                for dependency in dependencies do
                    let id =
                        dependency
                            .Attributes()
                            .Single(fun attr -> attr.Name.LocalName = "id")
                            .Value

                    let version =
                        dependency
                            .Attributes()
                            .Single(fun attr -> attr.Name.LocalName = "version")
                            .Value

                    yield
                        {
                            File = nuspecFile
                        },
                        {
                            PackageId = id
                            PackageVersion = version
                            ReqReinstall = None
                        }
        }
        |> List.ofSeq

    let allElements = Seq.append projectElements nuspecFileElements

    allElements |> MapHelper.MergeIntoMap

let GetVersionsMap(projectFiles: #seq<FileInfo>) : Map<string, Set<string>> =
    let packageInfos =
        projectFiles |> Seq.collect GetPackagesInfoFromProjectFile

    let packageVersionPairs =
        packageInfos
        |> Seq.map(fun (_, packageInfo) ->
            packageInfo.PackageId, packageInfo.PackageVersion
        )

    MapHelper.MergeIntoMap packageVersionPairs
    |> Map.map(fun _ versions -> Set.ofSeq versions)

let rec FindSolutions
    (currentDirectory: string)
    (dir: DirectoryInfo)
    : seq<FileInfo> =
    dir.Refresh()

    seq {
        // FIXME: avoid returning duplicates? (in case there are 2 .sln files in the same dir...)
        for file in dir.EnumerateFiles() do
            if file.Name.ToLower().EndsWith ".sln" then
                yield file

        for subdir in
            dir
                .EnumerateDirectories()
                .Where(notSubmodule currentDirectory) do
            for solution in FindSolutions currentDirectory subdir do
                yield solution
    }

let SanityCheckNugetPackages
    (target: ScriptTarget)
    (currentDirectory: string)
    (nugetSolutionPackagesDir: DirectoryInfo)
    =
    let sanityCheckNugetPackagesFromSolution(sol: FileInfo) =
        let getAllPackageIdsAndVersions
            (packageTree: Map<ComparableFileInfo, seq<PackageInfo>>)
            : Map<PackageInfo, seq<DependencyHolder>> =
            seq {
                for KeyValue(dependencyHolderFile, pkgs) in packageTree do
                    for pkg in pkgs do
                        yield pkg, dependencyHolderFile.DependencyHolderName
            }
            |> MapHelper.MergeIntoMap

        let getDirectoryNamesForPackagesSet
            (packages: Map<PackageInfo, seq<DependencyHolder>>)
            : Map<string, seq<DependencyHolder>> =
            seq {
                for KeyValue(package, prjs) in packages do
                    let dirForPackage =
                        sprintf
                            "%s%s%s"
                            (package.PackageId.ToLower())
                            (Path.DirectorySeparatorChar.ToString())
                            package.PackageVersion

                    yield dirForPackage, prjs
            }
            |> Map.ofSeq

        let findMissingPackageDirs
            (solDir: DirectoryInfo)
            (idealPackageDirs: Map<string, seq<DependencyHolder>>)
            : Map<string, seq<DependencyHolder>> =
            solDir.Refresh()

            if not nugetSolutionPackagesDir.Exists then
                failwithf
                    "'%s' subdir under solution dir %s doesn't exist, run `make` first"
                    nugetSolutionPackagesDir.Name
                    nugetSolutionPackagesDir.FullName

            let packageDirsAbsolutePaths =
                nugetSolutionPackagesDir
                    .EnumerateDirectories()
                    .Select(fun dir -> dir.FullName)

            if not(packageDirsAbsolutePaths.Any()) then
                Console.Error.WriteLine(
                    sprintf
                        "'%s' subdir under solution dir %s doesn't contain any packages"
                        nugetSolutionPackagesDir.Name
                        nugetSolutionPackagesDir.FullName
                )

                Console.Error.WriteLine
                    "Maybe you forgot to issue the commands `git submodule sync --recursive && git submodule update --init --recursive`?"

                Environment.Exit 1

            seq {
                for KeyValue(packageDirNameThatShouldExist, prjs) in
                    idealPackageDirs do
                    let pkgDirToLookFor =
                        Path.Combine(
                            nugetSolutionPackagesDir.FullName,
                            packageDirNameThatShouldExist
                        )
                        |> DirectoryInfo

                    if not pkgDirToLookFor.Exists then
                        yield packageDirNameThatShouldExist, prjs
            }
            |> Map.ofSeq

        let findPackagesWithMoreThanOneVersion
            (packageTree: Map<ComparableFileInfo, seq<PackageInfo>>)
            : Map<string, seq<ComparableFileInfo * PackageInfo>> =

            let getAllPackageVersionsForPackageId
                (packages: seq<PackageInfo>)
                (packageId: string)
                =
                seq {
                    for package in packages do
                        if package.PackageId = packageId then
                            yield package.PackageVersion
                }
                |> Set

            let packageInfos = GetAllPackageInfos packageTree

            let packageIdsWithMoreThan1Version =
                seq {
                    for packageId in
                        packageInfos.Select(fun pkg -> pkg.PackageId) do
                        let versions =
                            getAllPackageVersionsForPackageId
                                packageInfos
                                packageId

                        if versions.Count > 1 then
                            yield packageId
                }

            if not(packageIdsWithMoreThan1Version.Any()) then
                Map.empty
            else
                seq {
                    for pkgId in packageIdsWithMoreThan1Version do
                        let pkgs =
                            seq {
                                for KeyValue(file, packageInfos) in packageTree do
                                    for pkg in packageInfos do
                                        if pkg.PackageId = pkgId then
                                            yield file, pkg
                            }

                        yield pkgId, pkgs
                }
                |> Map.ofSeq

        let packageTree =
            getPackageTree currentDirectory nugetSolutionPackagesDir sol

        let packages = getAllPackageIdsAndVersions packageTree

        Console.WriteLine(
            sprintf
                "%i nuget packages found for solution %s"
                packages.Count
                sol.Name
        )

        let idealDirList = getDirectoryNamesForPackagesSet packages

        let solDir = sol.Directory
        solDir.Refresh()
        let missingPackageDirs = findMissingPackageDirs solDir idealDirList

        if missingPackageDirs.Any() then
            for KeyValue(missingPkg, depHolders) in missingPackageDirs do
                let depHolderNames =
                    String.Join(",", depHolders.Select(fun dh -> dh.Name))

                Console.Error.WriteLine(
                    sprintf
                        "Missing folder for nuget package in submodule: %s (referenced from %s)"
                        missingPkg
                        depHolderNames
                )

            Environment.Exit 1

        let pkgWithMoreThan1VersionPrint
            (key: string)
            (packageInfos: seq<ComparableFileInfo * PackageInfo>)
            =
            Console.Error.WriteLine(
                sprintf
                    "Package found with more than one version: %s. All occurrences:"
                    key
            )

            for file, pkgInfo in packageInfos do
                Console.Error.WriteLine(
                    sprintf
                        "* Version: %s. Dependency holder: %s"
                        pkgInfo.PackageVersion
                        file.DependencyHolderName.Name
                )

        let packagesWithMoreThanOneVersion =
            findPackagesWithMoreThanOneVersion packageTree

        if packagesWithMoreThanOneVersion.Any() then
            Map.iter pkgWithMoreThan1VersionPrint packagesWithMoreThanOneVersion
            Environment.Exit 1

        let findPackagesWithSomeReqReinstallAttrib
            (packageTree: Map<ComparableFileInfo, seq<PackageInfo>>)
            : seq<ComparableFileInfo * PackageInfo> =
            seq {
                for KeyValue(file, packageInfos) in packageTree do
                    for pkg in packageInfos do
                        match pkg.ReqReinstall with
                        | Some true -> yield file, pkg
                        | _ -> ()
            }

        let packagesWithWithSomeReqReinstallAttrib =
            findPackagesWithSomeReqReinstallAttrib packageTree

        if packagesWithWithSomeReqReinstallAttrib.Any() then
            Console.Error.WriteLine(
                sprintf
                    "Packages found with some RequireReinstall attribute (please reinstall it before pushing):"
            )

            for file, pkg in packagesWithWithSomeReqReinstallAttrib do
                Console.Error.WriteLine(
                    sprintf
                        "* Name: %s. Project: %s"
                        pkg.PackageId
                        file.DependencyHolderName.Name
                )

            Environment.Exit 1

        Console.WriteLine(
            sprintf
                "Nuget sanity check succeeded for solution dir %s"
                solDir.FullName
        )

    match target with
    | Solution solution -> sanityCheckNugetPackagesFromSolution solution
    | Folder folder ->

        let solutions = FindSolutions currentDirectory folder

        if Seq.isEmpty solutions then
            failwithf "There is no *.sln file located in: '%s'" folder.FullName

        for sol in solutions do
            sanityCheckNugetPackagesFromSolution sol
