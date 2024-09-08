namespace Nava.Benchmark.Configuration

open System
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Loggers
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Columns
open BenchmarkDotNet.Exporters
open BenchmarkDotNet.Reports
open BenchmarkDotNet.Order
open BenchmarkDotNet.Toolchains.CsProj
open BenchmarkDotNet.Diagnostics.Windows

[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Assembly)>]
type CoreAffinityAttribute() =
    inherit Attribute()
    let on = "1"
    let off = "0"

    interface IConfigSource with
        member _.Config =
            let config =
                let job =
                    Job.RyuJitX64.WithToolchain(
                        if OperatingSystem.IsWindows() then
                            CsProjCoreToolchain.NetCoreApp90
                        else
                            CsProjCoreToolchain.NetCoreApp80
                    )

                ManualConfig
                    .CreateEmpty()
                    .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
                    .WithOrderer(DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
                    .AddJob(
                        job
                            .WithAffinity(nativeint(1 <<< 1))
                            .WithMinIterationTime(Perfolizer.Horology.TimeInterval.FromMilliseconds(850.0))
                            .WithMinWarmupCount(5)
                            .WithMaxWarmupCount(10)
                            .WithMinIterationCount(5)
                            // .WithMaxIterationCount(20)
                            .WithGcServer(true)
                            .WithGcConcurrent(true)
                            .WithGcRetainVm(true)
                            .WithPowerPlan(BenchmarkDotNet.Environments.PowerPlan.HighPerformance)
                            .WithEnvironmentVariables(
                                [|
                                    EnvironmentVariable("DOTNET_TieredPGO", on)
                                    EnvironmentVariable("DOTNET_TieredCompilation", on)
                                    EnvironmentVariable("DOTNET_TC_QuickJit", on)
                                    EnvironmentVariable("DOTNET_TC_QuickJitForLoops", on)
                                    EnvironmentVariable("DOTNET_ReadyToRun", off)
                                |]
                            )
                            .Apply()
                    )
                    .AddColumn(RankColumn.Arabic, CategoriesColumn.Default) //,BaselineColumn.Default)
                    //.AddColumn(StatisticColumn.AllStatistics)
                    .AddLogger(ConsoleLogger.Default)
                    .AddDiagnoser(
                        MemoryDiagnoser.Default,
                        DisassemblyDiagnoser(
                            DisassemblyDiagnoserConfig(
                                printSource = true,
                                exportHtml = true,
                                exportCombinedDisassemblyReport = true,
                                exportDiff = true
                            )
                        )
                    //, NativeMemoryProfiler()
                    )

            if OperatingSystem.IsWindows() then
                let config =
                    if false then
                        config
                            .AddHardwareCounters(HardwareCounter.BranchMispredictions)
                            .AddHardwareCounters(HardwareCounter.BranchInstructions)
                    else
                        config

                config
            // .AddDiagnoser(InliningDiagnoser(), TailCallDiagnoser())
            else
                config