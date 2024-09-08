namespace Navika

open System
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Input
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Styling
open Avalonia.Markup.Xaml.MarkupExtensions
open Avalonia.Markup.Xaml.XamlIl.Runtime
open Avalonia.Markup.Xaml

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Elmish

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentAvalonia.Styling.FluentAvaloniaTheme())
        // this.Styles.Add(Themes.Simple.SimpleTheme())
        // this |> Theme |> ignore

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- Shell.MainWindow()
        | _ -> ()

module Program =
    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)