namespace Navika.Theme

open System
open System.Collections.Generic
open System.ComponentModel
open System.Linq
open System.Runtime.CompilerServices
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Avalonia.Markup.Xaml.XamlIl.Runtime
open CompiledAvaloniaXaml
open InlineIL
open FsToolkit.ErrorHandling

[<Extension>]
type ServiceProviderExtensions() =
    [<Extension>]
    static member GetService<'TService>(this: IServiceProvider) =
        this.GetService(typeof<'TService>) :?> 'TService

type Context<'TTarget> =
    val AvaloniaNameScope: INameScope
    val mutable BaseUri: Uri
    val innerServiceProvider: IServiceProvider
    val mutable IntermediateRoot: obj
    val ParentsStack: obj Stack
    val mutable ProvideTargetObject: obj
    val mutable ProvideTargetProperty: obj
    val mutable RootObject: 'TTarget voption
    val serviceProvider: IServiceProvider voption
    val staticNamespaceInfoProviders: obj[]

    new(rootObject, serviceProvider: IServiceProvider, staticNamespaceInfoProviders: obj[], baseUriString: string) =
        Context<'TTarget>(
            rootObject,
            serviceProvider,
            staticNamespaceInfoProviders,
            (if baseUriString <> null then Uri(baseUriString) else null),
            XamlIlRuntimeHelpers.CreateInnerServiceProviderV1 null
        )

    new(rootObject, serviceProvider: IServiceProvider, baseUriString: string) =
        Context<'TTarget>(rootObject, serviceProvider, [||], baseUriString)

    new(rootObject, serviceProvider: IServiceProvider, staticNamespaceInfoProviders: obj[], baseUri: Uri, innerServiceProvider) =
        {
            AvaloniaNameScope = serviceProvider.GetService(typeof<INameScope>) :?> _
            serviceProvider = serviceProvider |> ValueOption.ofObj
            staticNamespaceInfoProviders = staticNamespaceInfoProviders
            innerServiceProvider = innerServiceProvider
            RootObject = rootObject
            IntermediateRoot = null
            ProvideTargetObject = null
            ProvideTargetProperty = null
            ParentsStack = Stack()
            BaseUri = baseUri
        }

    interface IRootObjectProvider with
        member this.RootObject =
            match this.RootObject with
            | ValueSome rootObject -> rootObject :> obj
            | ValueNone ->
                match this.serviceProvider with
                | ValueNone -> null
                | ValueSome serviceProvider ->
                    (serviceProvider.GetService(typeof<IRootObjectProvider>) :?> IRootObjectProvider)
                        .RootObject

        member this.IntermediateRootObject = this.IntermediateRoot

    interface IAvaloniaXamlIlEagerParentStackProvider with
        member this.DirectParentsStack = this.ParentsStack.Reverse().ToArray()

        member this.ParentProvider =
            voption {
                let! serviceProvider = this.serviceProvider

                let! parentProvider =
                    serviceProvider.GetService<IAvaloniaXamlIlParentStackProvider>()
                    |> ValueOption.ofObj

                return parentProvider.AsEagerParentStackProvider()
            }
            |> ValueOption.defaultValue null

    interface IAvaloniaXamlIlParentStackProvider with
        member this.Parents =
            seq {
                yield! this.ParentsStack

                voption {
                    let! serviceProvider = this.serviceProvider

                    let! parentProvider =
                        serviceProvider.GetService<IAvaloniaXamlIlParentStackProvider>()
                        |> ValueOption.ofObj

                    return parentProvider.Parents
                }
                |> ValueOption.defaultValue Seq.empty
            }

    interface ITypeDescriptorContext with
        member _.Container = null
        member _.Instance = null
        member _.PropertyDescriptor = null
        member this.GetService serviceType = this.GetService serviceType
        member _.OnComponentChanging() = NotSupportedException() |> raise
        member _.OnComponentChanged() = NotSupportedException() |> raise

    interface IProvideValueTarget with
        member this.TargetObject = this.ProvideTargetObject
        member this.TargetProperty = this.ProvideTargetProperty

    interface IUriContext with
        member this.BaseUri
            with get () = this.BaseUri
            and set (value) = this.BaseUri <- value

    member this.PushParent(parent: obj) =
        this.ParentsStack.Push parent
        // this.ParentsStack.Add(parent)
        this.ProvideTargetObject <- parent

    member this.PopParent() =
        // let index = this.ParentsStack.Count - 1
        // this.ProvideTargetObject <- this.ParentsStack.[index]
        // this.ParentsStack.RemoveAt(index)
        this.ProvideTargetObject <- this.ParentsStack.Pop()

    member this.GetService(serviceType: Type) =
        if this.innerServiceProvider |> isNull |> not then
            match this.innerServiceProvider.GetService serviceType with
            | service when service |> isNull |> not ->
                IL.Push service
                IL.Emit.Ret()
                raise(IL.Unreachable())
            | _ ->
                IL.Emit.Br "Failure"
                raise(IL.Unreachable())

        IL.MarkLabel "Failure"

        if
            LanguagePrimitives.PhysicalEquality serviceType typeof<IRootObjectProvider>
            || LanguagePrimitives.PhysicalEquality serviceType typeof<IAvaloniaXamlIlParentStackProvider>
            || LanguagePrimitives.PhysicalEquality serviceType typeof<ITypeDescriptorContext>
            || LanguagePrimitives.PhysicalEquality serviceType typeof<IProvideValueTarget>
            || LanguagePrimitives.PhysicalEquality serviceType typeof<IUriContext>
        then
            this |> box
        elif this.staticNamespaceInfoProviders |> isNull |> not then
            for obj in this.staticNamespaceInfoProviders do
                if serviceType.IsAssignableFrom(obj.GetType()) then
                    IL.Push obj
                    IL.Emit.Ret()
                    raise(IL.Unreachable())

            null
        else
            null