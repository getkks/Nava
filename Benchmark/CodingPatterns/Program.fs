namespace Nava.Benchmark.CodingPatterns

open BenchmarkDotNet.Running

module Program =
    type Marker = class end

    [<EntryPoint>]
    let main arg =
        BenchmarkSwitcher.FromAssembly(typeof<Marker>.Assembly).Run arg |> ignore
        0