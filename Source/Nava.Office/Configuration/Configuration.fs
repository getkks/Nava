namespace Nava

open System
open System.IO
open System.Text.Json
open Nava.Converters
open System.Text.Json.Serialization

module Configuration =

    let defaultJsonOptions =
        JsonFSharpOptions
            .Default()
            .WithUnionUnwrapFieldlessTags()
            .WithUnionUnwrapSingleFieldCases()
            .WithSkippableOptionFields()
            .ToJsonSerializerOptions()
            .UseFileSystemConverters()