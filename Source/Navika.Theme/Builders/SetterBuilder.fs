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

module SetterBuilder =

    let setterValueProperty =
        new ClrPropertyInfo(
            "Value",
            (fun setter -> unbox<Setter>(setter).Value),
            (fun setter value -> unbox<Setter>(setter).Value <- value),
            typeof<obj>
        )