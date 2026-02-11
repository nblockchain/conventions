module Workflow

open System.IO
open System.Text.RegularExpressions

open YamlDotNet.RepresentationModel

let private DetectInconsistentVersionsInYamlFiles
    (fileInfos: seq<FileInfo>)
    (extractVersionsFunction: YamlNode -> seq<string * string>)
    =
    let envVarRegex =
        Regex(@"\s*\$\{\{\s*([^\s\}]+)\s*\}\}\s*", RegexOptions.Compiled)

    let yamlDocuments =
        Seq.map
            (fun (fileInfo: FileInfo) ->
                let yaml = YamlStream()
                use reader = new StreamReader(fileInfo.FullName)
                yaml.Load reader
                yaml.Documents[0].RootNode
            )
            fileInfos

    let versionMap =
        Seq.fold
            (fun mapping (yamlDoc: YamlNode) ->
                let matches =
                    Seq.collect extractVersionsFunction yamlDoc.AllNodes

                matches
                |> Seq.fold
                    (fun acc (key, value) ->
                        let actualValue =
                            let variableRegexMatch = envVarRegex.Match value

                            if variableRegexMatch.Success then
                                let yamlDict = yamlDoc :?> YamlMappingNode

                                match yamlDict.Children.TryGetValue "env" with
                                | true, (:? YamlMappingNode as envDict) ->
                                    let referenceString =
                                        variableRegexMatch.Groups.[1].Value

                                    let envVarName =
                                        if referenceString.StartsWith "env." then
                                            referenceString.[4..]
                                        else
                                            referenceString

                                    match
                                        envDict.Children.TryGetValue envVarName
                                        with
                                    | true, envVarValue ->
                                        (envVarValue :?> YamlScalarNode).Value
                                    | false, _ -> value
                                | _ -> value
                            else
                                value

                        match Map.tryFind key acc with
                        | Some prevSet ->
                            Map.add key (Set.add actualValue prevSet) acc
                        | None -> Map.add key (Set.singleton actualValue) acc
                    )
                    mapping
            )
            Map.empty
            yamlDocuments

    versionMap
    |> Seq.map(fun item -> Seq.length item.Value > 1)
    |> Seq.contains true

let DetectInconsistentVersionsInGitHubCIWorkflow(fileInfos: seq<FileInfo>) =
    fileInfos
    |> Seq.iter(fun fileInfo -> assert (fileInfo.FullName.EndsWith ".yml"))

    let extractVersions(node: YamlNode) =
        match node with
        | :? YamlMappingNode as yamlDict ->
            yamlDict.Children
            |> Seq.collect(fun (KeyValue(keyNode, valueNode)) ->
                match keyNode, valueNode with
                | (:? YamlScalarNode as keyScalar),
                  (:? YamlScalarNode as valueScalar) when
                    keyScalar.Value = "uses"
                    ->
                    match valueScalar.Value.Split "@v" with
                    | [| name; version |] -> Seq.singleton(name, version)
                    | _ -> Seq.empty
                | (:? YamlScalarNode as keyScalar),
                  (:? YamlMappingNode as valueMapping) when
                    keyScalar.Value = "with"
                    ->
                    valueMapping.Children
                    |> Seq.choose(fun (KeyValue(innerKeyNode, innerValueNode)) ->
                        match innerKeyNode, innerValueNode with
                        | (:? YamlScalarNode as keyScalar),
                          (:? YamlScalarNode as valueScalar) ->
                            match keyScalar.Value.Split '-' with
                            | [| name; "version" |] ->
                                Some(name, valueScalar.Value)
                            | _ -> None
                        | _ -> None
                    )
                | _ -> Seq.empty
            )
        | _ -> Seq.empty

    DetectInconsistentVersionsInYamlFiles fileInfos extractVersions

let DetectInconsistentVersionsInGitHubCI(dir: DirectoryInfo) =
    let ymlFiles = dir.GetFiles("*.yml", SearchOption.AllDirectories)

    if Seq.isEmpty ymlFiles then
        false
    else
        DetectInconsistentVersionsInGitHubCIWorkflow ymlFiles
