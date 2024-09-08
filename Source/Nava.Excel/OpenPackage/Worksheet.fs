namespace Nava.Excel

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open Nava.Runtime.Zip
open FsToolkit.ErrorHandling
open InlineIL
open StringBuffer

type Worksheet(name: string, id, target, openPart: OpenPart) =
    inherit OpenPart(id, target, openPart)

    member this.Name = name

    override this.ToString() =
        base.ToString(stringBuffer { "Name: " + this.Name })