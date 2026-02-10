using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EasyLog;

/// <summary>
///     Logger that serializes entries to XML.
/// </summary>
public class XmlLogger<T>(string logDirectory) : AbstractLogger<T>(logDirectory, "xml")
{
    private readonly XmlWriterSettings _options = new() { Indent = false, OmitXmlDeclaration = true };

    /// <summary>
    ///     Serializes the specified log entry of type <typeparamref name="T" /> into a XML string.
    /// </summary>
    /// <param name="log">The log entry to serialize.</param>
    /// <returns>A XML string representation of the log entry.</returns>
    /// <remarks>
    ///     This method overrides a base class implementation to provide XML serialization
    ///     using the <see cref="XmlSerializer" /> class with specific serialization options.
    /// </remarks>
    protected override string Serialize(T log)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        var stringBuilder = new StringBuilder();
        using (var stringWriter = new StringWriter(stringBuilder))
        {
            using (var xmlWriter = XmlWriter.Create(stringWriter, _options))
            {
                xmlSerializer.Serialize(xmlWriter, log);
            }
        }

        return stringBuilder.ToString();
    }
}