#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Linq
open System.Diagnostics

open System.Text
open System.Text.RegularExpressions

#r "System.Core.dll"
open System.Xml
#r "System.Xml.Linq.dll"

open System.Xml.Linq
open System.Xml.XPath

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk
open Fsdk.Process


#r "nuget: Microsoft.Build, Version=16.11.0"
open Microsoft.Build.Construction
let ScriptsDir = __SOURCE_DIRECTORY__ |> DirectoryInfo
let RootDir = Path.Combine(ScriptsDir.FullName, "..") |> DirectoryInfo

let nugetSolutionPackagesDir =
    Path.Combine(RootDir.FullName, "packages") |> DirectoryInfo

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
                        (from.Where(fun (aKey, aValue) -> theKey = aKey))
                            .Select(fun (aKey, aValue) -> aValue)
                        |> seq

                    yield theKey, valsForKey
            }

        keyValuePairs |> Map.ofSeq

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


let SanityCheckNugetPackages() =

    let notPackagesFolder(dir: DirectoryInfo) : bool =
        dir.FullName <> nugetSolutionPackagesDir.FullName

    let notSubmodule(dir: DirectoryInfo) : bool =
        let getSubmoduleDirsForThisRepo() : seq<DirectoryInfo> =
            if File.Exists ".gitmodules" then
                let regex = Regex("path\s*=\s*([^\s]+)")

                seq {
                    for regexMatch in
                        regex.Matches(File.ReadAllText ".gitmodules") do
                        let submoduleFolderRelativePath =
                            regexMatch.Groups.[1].ToString()

                        let submoduleFolder =
                            DirectoryInfo(
                                Path.Combine(
                                    Directory.GetCurrentDirectory(),
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

    // this seems to be a bug in Microsoft.Build nuget library, FIXME: report
    let normalizeDirSeparatorsPaths(path: string) : string =
        path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)

    let sanityCheckNugetPackagesFromSolution(sol: FileInfo) =
        let rec findProjectFiles() : seq<FileInfo> =
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

        let rec findNuspecFiles(dir: DirectoryInfo) : seq<FileInfo> =
            dir.Refresh()

            seq {
                for file in dir.EnumerateFiles() do
                    if (file.Name.ToLower()).EndsWith ".nuspec" then
                        yield file

                for subdir in
                    dir
                        .EnumerateDirectories()
                        .Where(notSubmodule)
                        .Where(notPackagesFolder) do
                    for file in findNuspecFiles subdir do
                        yield file
            }

        let getPackageTree
            (sol: FileInfo)
            : Map<ComparableFileInfo, seq<PackageInfo>> =
            let projectFiles = findProjectFiles()

            let projectElements =
                seq {
                    for projectFile in projectFiles do
                        let xmlDoc = XDocument.Load projectFile.FullName
                        let query = "//PackageReference"
                        let pkgReferences = xmlDoc.XPathSelectElements query

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
                                    .Single(fun attr ->
                                        attr.Name.LocalName = "Version"
                                    )
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
                |> List.ofSeq

            let solDir = sol.Directory
            solDir.Refresh()
            let nuspecFiles = findNuspecFiles solDir

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
                                let fixedQuery =
                                    String.Format(query, String.Empty)

                                xmlDoc.XPathSelectElements fixedQuery
                            | Some(nsManager, nsPrefix) ->
                                let fixedQuery = String.Format(query, nsPrefix)

                                xmlDoc.XPathSelectElements(
                                    fixedQuery,
                                    nsManager
                                )

                        for dependency in dependencies do
                            let id =
                                dependency
                                    .Attributes()
                                    .Single(fun attr ->
                                        attr.Name.LocalName = "id"
                                    )
                                    .Value

                            let version =
                                dependency
                                    .Attributes()
                                    .Single(fun attr ->
                                        attr.Name.LocalName = "version"
                                    )
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

        let findExcessPackageDirs
            (solDir: DirectoryInfo)
            (idealPackageDirs: Map<string, seq<DependencyHolder>>)
            : seq<string> =
            solDir.Refresh()

            if not(nugetSolutionPackagesDir.Exists) then
                failwithf
                    "'%s' subdir under solution dir %s doesn't exist, run `make` first"
                    nugetSolutionPackagesDir.Name
                    nugetSolutionPackagesDir.FullName
            // "src" is a directory for source codes and build scripts,
            // not for packages, so we need to exclude it from here
            let packageDirNames =
                nugetSolutionPackagesDir
                    .EnumerateDirectories()
                    .Select(fun dir -> dir.Name)
                    .Except([ "src" ])

            if not(packageDirNames.Any()) then
                failwithf
                    "'%s' subdir under solution dir %s doesn't contain any packages"
                    nugetSolutionPackagesDir.Name
                    nugetSolutionPackagesDir.FullName

            let packageDirsThatShouldExist =
                MapHelper.GetKeysOfMap idealPackageDirs

            seq {
                for packageDirThatExists in packageDirNames do
                    if
                        not
                            (
                                packageDirsThatShouldExist.Contains
                                    packageDirThatExists
                            )
                    then
                        yield packageDirThatExists
            }

        let findPackagesWithMoreThanOneVersion
            (packageTree: Map<ComparableFileInfo, seq<PackageInfo>>)
            : Map<string, seq<ComparableFileInfo * PackageInfo>> =

            let getAllPackageInfos
                (packages: Map<ComparableFileInfo, seq<PackageInfo>>)
                =
                let pkgInfos =
                    seq {
                        for KeyValue(_, pkgs) in packages do
                            for pkg in pkgs do
                                yield pkg
                    }

                Set pkgInfos

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

            let packageInfos = getAllPackageInfos packageTree

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

        let packageTree = getPackageTree sol
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


    let rec findSolutions(dir: DirectoryInfo) : seq<FileInfo> =
        dir.Refresh()

        seq {
            // FIXME: avoid returning duplicates? (in case there are 2 .sln files in the same dir...)
            for file in dir.EnumerateFiles() do
                if file.Name.ToLower().EndsWith ".sln" then
                    yield file

            for subdir in dir.EnumerateDirectories().Where notSubmodule do
                for solution in findSolutions subdir do
                    yield solution
        }

    let slnFile = Path.Combine(RootDir.FullName, "conventions.sln") |> FileInfo

    if not slnFile.Exists then
        raise
        <| FileNotFoundException("Solution file not found", slnFile.FullName)

    sanityCheckNugetPackagesFromSolution slnFile


SanityCheckNugetPackages()
