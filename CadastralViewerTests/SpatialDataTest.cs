using CadastralViewer.Models;
using System.Globalization;
using System.Xml;

namespace CadastralViewerTests;
[TestFixture]
public class SpatialDataTests
{
    private protected XmlDocument xmlDocument;
    [SetUp]
    public void Setup()
    {
        xmlDocument = XmlDocumentData.XmlDocument;
    }

    [Test]
    public void TestSpatialData()
    {
        var data = new SpatialData(xmlDocument);
       Assert.IsTrue(data.TitledFeatureCollections
           .Any(a => a.Item1
           .Any(aa => aa.Geometry.Coordinates
           .Any(aaa => aaa.Y == 518763.23 && aaa.X == 2150703.58))));
    }
}