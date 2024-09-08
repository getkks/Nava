namespace Nava.Office.Outlook

open System
open Microsoft.Office.Interop.Outlook

type OutlookApplication() =
    inherit ApplicationClass()

    member this.NewEmail() =
        OlItemType.olMailItem |> this.CreateItem :?> MailItem

module Outlook =
    let check() =
        let outlook = OutlookApplication()

        let account =
            outlook.Session.Accounts
            |> Seq.cast<Account>
            |> Seq.find(fun account -> account.DisplayName = "karthikkselvan@gmail.com")

        let email = outlook.NewEmail()
        email.SendUsingAccount <- account
        email.Display()