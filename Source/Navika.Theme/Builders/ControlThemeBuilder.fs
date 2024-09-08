namespace Navika.Theme.Builders

open System
open System.Collections.Generic
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Data
open Avalonia.Data.Core
open Avalonia.Input
open Avalonia.Markup.Xaml
open Avalonia.Markup.Xaml.MarkupExtensions
open Avalonia.Markup.Xaml.XamlIl.Runtime
open Avalonia.Media
open Avalonia.Styling

open Navika.Theme

type ControlThemeBuilder<'T, 'TControl when 'TControl :> Control> =
    val mutable context: 'T Context
    val mutable controlTheme: ControlTheme

    new(context: 'T Context) =
        let controlTheme = ControlTheme()
        context.PushParent controlTheme
        {
            context = context
            controlTheme = controlTheme
        }