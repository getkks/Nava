namespace Nava

open System
open System.IO
open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Serialization

module Converters =

    type FileInfoConverter() =
        inherit JsonConverter<FileInfo>()
        override this.Read(reader, typeToConvert, options) = reader.GetString() |> FileInfo
        override this.Write(writer, value, options) = writer.WriteStringValue value.FullName

    type DirectoryInfoConverter() =
        inherit JsonConverter<DirectoryInfo>()
        override this.Read(reader, typeToConvert, options) = reader.GetString() |> DirectoryInfo
        override this.Write(writer, value, options) = writer.WriteStringValue value.FullName

    [<Extension>]
    type JsonSerializerOptionsExtensions =
        [<Extension>]
        static member UseFileSystemConverters(options: JsonSerializerOptions) =
            let converters = options.Converters
            converters.Add(FileInfoConverter())
            converters.Add(DirectoryInfoConverter())
            options