namespace Nava.Office.Outlook

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.Office.Interop.Outlook

[<Extension>]
type ApplicationExtensions =
    [<Extension>]
    static member NewEmail(outlook: ApplicationClass) =
        OlItemType.olMailItem |> outlook.CreateItem :?> MailItem

module OutlookApplication =
    let create() = ApplicationClass()