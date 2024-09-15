namespace Nava.Office.Outlook

open System
open System.Collections.Generic
open System.Collections.Frozen
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

open FSharp.Core.ValueOption
open FsToolkit.ErrorHandling

open Microsoft.Office.Interop.Outlook

open InlineIL

open AngleSharp
open AngleSharp.Dom
open AngleSharp.Html.Parser
open AngleSharp.Html.Dom

open Nava.Runtime

module EMailExtensions =
    /// <summary> <see cref="T:System.Collections.Frozen.FrozenSet`1" /> of html tags that can hold text but are empty and can be removed. </summary>
    let internal textTags =
        HashSet(
            [|
                "p"
                "div"
                "span"
                "li"
                "ul"
            |],
            StringComparer.OrdinalIgnoreCase
        )
            .ToFrozenSet()

    [<TailCall>]
    let rec internal depthSearch(element: IElement) =
        match element.FirstElementChild with
        | null -> element
        | child -> depthSearch child

    let rec internal parentSiblingSearch root (element: IElement) =
        element.ParentElement
        |> ofObj
        |> bind(fun parent ->
            if LanguagePrimitives.PhysicalEquality parent root then
                ValueNone
            else
                parent.NextElementSibling
                |> ofObj
                |> orElseWith(fun _ -> parentSiblingSearch root parent))

    let internal levelSearch root (element: IElement) =
        element.NextElementSibling
        |> ofObj
        |> map depthSearch
        |> orElseWith(fun _ ->
            element.ParentElement
            |> ofObj
            |> bind(fun element ->
                if LanguagePrimitives.PhysicalEquality element root then
                    ValueNone
                else
                    element.NextElementSibling |> ofObj)
            |> map depthSearch)

    let internal removeEmptyTags(element: IElement) =
        for item in element.QuerySelectorAll "p" do
            if item.TextContent.Trim() |> String.IsNullOrWhiteSpace then
                item.Remove()

    /// <summary> Html tags that can hold text but are empty and can be removed will be removed from the <paramref name="element"/>. <paramref name="tags"/> contains the tags to remove. </summary>
    /// <remarks> Starts a depth first search from <paramref name="element"/>. Once leaf tag is reached it will check if the tag is empty. If it is empty it will be removed.
    /// The search will continue with siblings if any else it will move up to the parent. The siblings search will continue at each level until the <paramref name="element"/> is reached. </remarks>
    let internal removeEmptyTags1 (element: IElement) (tags: #ISet<_>) =
        let mutable leaf = element |> depthSearch

        while (let nextLeaf = leaf |> levelSearch element
               let mutable tagName = leaf.LocalName

               if
                   tags.Contains(
                       match tagName.Split(':') with
                       | [| _; tagName |] -> tagName
                       | _ -> tagName
                   )
                   && leaf.TextContent.Trim() |> String.IsNullOrWhiteSpace
               then
                   leaf.Remove()

               match nextLeaf with
               | ValueSome newLeaf ->
                   leaf <- newLeaf
                   true
               | _ -> false) do
            ()

    let internal removeEmptyTags2 (element: IElement) (tags: #ISet<_>) =
        let mutable leaf = element
        let mutable tempElement = null

        while leaf.HasChildNodes
              && (tempElement <- leaf.FirstElementChild
                  tempElement |> isNull |> not) do
            leaf <- tempElement

        while not <| LanguagePrimitives.PhysicalEquality leaf element do
            tempElement <- leaf.NextElementSibling

            if tempElement |> isNull then
                tempElement <- leaf.ParentElement

            let mutable tagName = leaf.LocalName

            if
                tags.Contains(
                    match tagName.Split(':') with
                    | [| _; tagName |] -> tagName
                    | _ -> tagName
                )
                && leaf.TextContent.Trim() |> String.IsNullOrWhiteSpace
            then
                leaf.Remove()

            leaf <- tempElement

[<Extension>]
type EMailExtensions =
    [<Extension>]
    static member SenderAs(mail: MailItem, displayName) =
        let account =
            mail.Application.Session.Accounts
            |> Seq.cast<Account>
            |> Seq.find(fun account -> account.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))

        mail.SendUsingAccount <- account

    [<Extension>]
    static member RemoveEmptyLines(mail: MailItem) =
        mail.Display()
        let parser = HtmlParser()
        let document = parser.ParseDocument mail.HTMLBody
        EMailExtensions.removeEmptyTags document.Body
        mail.HTMLBody <- document.ToHtml()

module c =
    let check() =
        let outlook = ApplicationClass()
        let mail = outlook.NewEmail()
        mail.RemoveEmptyLines()