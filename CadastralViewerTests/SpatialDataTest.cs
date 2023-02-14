using CadastralViewer.Models;
using System.Xml;

namespace CadastralViewerTests;
[TestFixture]
public class SpatialDataTests
{
    private static IEnumerable<XmlDocument> XmlDocuments
    {
        get
        {
            IEnumerable<string> xmlStrings = typeof(XmlDocumentData)
            .GetFields()
            .Where(field => field.IsPublic &&
            field.IsStatic &&
            field.FieldType == typeof(string))
            .Select(field => field.GetValue(null))
            .Where(value => value != null)
            .Select(value => value!.ToString()!);

            return xmlStrings.Select(xmlString =>
            {
                XmlDocument document = new();
                document.LoadXml(xmlString);
                return document;
            });
        }
    }
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        
    }

    [SetUp]
    public void Setup()
    {
        Console.WriteLine("I'm happy I'm using NUnit😀");
    }

    [Test, TestCaseSource(nameof(XmlDocuments))]
    public void TestSpatialData(XmlDocument document)
    {
        var data = new SpatialData(document);
        Assert.IsTrue(data.TitledFeatureCollections.Count() > 0 && 
            !data.TitledFeatureCollections.Any(tfc => tfc.Item1.Any(fc => fc.Geometry.IsEmpty)));
    }
}