namespace Nava.Office

open System.Linq
open System.Runtime.InteropServices
open Microsoft.Office.Interop.Excel

module ListObject =
    /// <summary> The part of the list object range. </summary>
    type ListObjectPart =
        /// <summary> Entire list object range. </summary>
        | All
        /// <summary> Body of the list object range. </summary>
        | Body
        /// <summary> Header row of the list object range. </summary>
        | Header
        /// <summary> Totals row of the list object range. </summary>
        | Totals

    type ListObject with
        /// <summary> Copies the range of the list object to clipboard. </summary>
        /// <param name="part"> The part of the list object to copy. </param>
        member this.Copy(?part) =
            match part with
            | None
            | Some All -> this.Range.Copy()
            | Some Body -> this.DataBodyRange.Copy()
            | Some Header -> this.HeaderRowRange.Copy()
            | Some Totals -> this.TotalsRowRange.Copy()
            |> ignore

        /// <summary> Pastes the list object range at the specified <paramref name="destination" />. </summary>
        /// <param name="destination"> The range to paste at. </param>
        /// <param name="part"> The part of the list object to paste. Default value is <see cref="ListObjectPart.All" />. </param>
        /// <param name="pasteType"> The type of paste. Default value is <see cref="XlPasteType.xlPasteValues" />. </param>
        /// <param name="operation"> The operation to perform during paste. Default value is <see cref="XlPasteSpecialOperation.xlPasteSpecialOperationNone" />. </param>
        /// <param name="skipBlanks"> Whether to skip blank cells during paste. Default value is <see langword="false" />. </param>
        /// <param name="transpose"> Whether to transpose the data during paste. Default value is <see langword="false" />. </param>
        member this.PasteAt
            (
                destination: Range,
                ?part: ListObjectPart,
                ?pasteType: XlPasteType,
                ?operation: XlPasteSpecialOperation,
                ?skipBlanks: bool,
                ?transpose: bool
            ) =
            this.Copy(defaultArg part All)

            destination.PasteSpecial(
                defaultArg pasteType XlPasteType.xlPasteValues,
                defaultArg operation XlPasteSpecialOperation.xlPasteSpecialOperationNone,
                defaultArg skipBlanks false,
                defaultArg transpose false
            )
            |> ignore