using System.Xml;
namespace Nava.Benchmark.CodingPatterns.CS.Xml;
/// <summary>
/// C# version of F# extensions.
/// </summary>
public static class XmlReaderExtensions
{
	public static IEnumerable<XmlReader> Elements(this XmlReader reader, string name)
	{
		while (reader.ReadToFollowing(name))
		{
			yield return reader;
		}
	}
}
