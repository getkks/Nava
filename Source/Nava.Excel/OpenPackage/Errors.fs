namespace Nava.Excel

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling
open InlineIL

type Errors =
    | RelationsNotFound of PartName: string * RelationPath: string
    | WorkbookPartNotFound of Path: string
    | WorkbookPartPathNotFound