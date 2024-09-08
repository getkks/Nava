namespace Nava.Benchmark.CodingPatterns.Xml

open System
open System.IO
open System.Linq
open System.Xml
open System.Xml.Linq
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Running

open Nava.Benchmark.Configuration
open Nava.Benchmark.CodingPatterns.CS.Xml

type Workspace = EasyBuild.FileSystemProvider.RelativeFileSystem<".">

module ReaderExtensions =

    let inline foldElements ([<InlineIfLambda>] folder) state elementName (reader: XmlReader) =
        let mutable state = state

        while reader.ReadToFollowing elementName do
            state <- folder state reader

        state

open ReaderExtensions

[<CoreAffinity>]
type Enumeration() =
    let dataPath = Workspace.``..``.``..``.Data.``sheet1.xml``
    let data = File.ReadAllText dataPath

    let elements elementName (reader: XmlReader) =
        seq {
            while reader.ReadToFollowing elementName do
                reader
        }

    [<Benchmark>]
    member _.EnumerateElementsFS() =
        use file = StringReader data
        use reader = XmlReader.Create file

        reader
        |> elements "v"
        |> Seq.length

    [<Benchmark>]
    member _.EnumerateElementsCS() =
        use file = StringReader data
        use reader = XmlReader.Create file

        reader
            .Elements("v")
            .Count()

    [<Benchmark>]
    member _.FoldElements() =
        use file = StringReader data
        use reader = XmlReader.Create file
        foldElements (fun state _ -> state + 1) 0 "v" reader

    [<Benchmark(Baseline = true)>]
    member _.Explicit() =
        use file = StringReader data
        use reader = XmlReader.Create file
        let mutable count = 0

        while reader.ReadToFollowing "v" do
            count <- count + 1