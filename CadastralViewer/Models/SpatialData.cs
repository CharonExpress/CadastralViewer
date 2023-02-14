using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Utilities;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CadastralViewer.Models;
public class SpatialData
{
    /// <summary>
    /// https://stackoverflow.com/questions/19167669/keep-only-numeric-value-from-a-string
    /// </summary>
    private static readonly Regex rxNonDigits = new Regex(@"[^\d]+");

    public List<Tuple<FeatureCollection, string>> TitledFeatureCollections;

    public SpatialData()
    {
        TitledFeatureCollections = new List<Tuple<FeatureCollection, string>>();
    }

    /// <summary>
    /// Initialize new object with list of features from xml doc
    /// </summary>
    /// <param name="xmlDoc"></param>
    public SpatialData(XmlDocument xmlDoc) : this()
    {
        AddFeaturesToList(xmlDoc);
    }

    /// <summary>
    /// Add features from xml document to exist list
    /// </summary>
    /// <param name="xmlDoc"></param>
    public void AddFeatures(XmlDocument xmlDoc)
    {
        AddFeaturesToList(xmlDoc);
    }

    private void AddFeaturesToList(XmlDocument xmlDoc)
    {
        foreach (EntitySpatial entitySpatial in GetEntitySpatials(xmlDoc))
        {
            TitledFeatureCollections.Add(new Tuple<FeatureCollection, string>(entitySpatial.FeatureCollection, entitySpatial.ParentTitle));
        }
    }

    /// <summary>
    /// Return geoJson string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {

        using (var stringWriter = new StringWriter())
        {
            var geoJsonSerializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
            geoJsonSerializer.Serialize(stringWriter, TitledFeatureCollections);
            return stringWriter.ToString();
        }
    }

    private List<EntitySpatial> GetEntitySpatials(XmlDocument xmlDocument)
    {
        XmlNodeList? nodeList = xmlDocument.SelectNodes(".//EntitySpatial|.//Entity_Spatial|.//entity_spatial|.//*[local-name() = 'EntitySpatial']");

        if (nodeList == null) throw new Exception("Cannot get entity spatial nodes");

        List<EntitySpatial> entities = new List<EntitySpatial>();

        foreach (XmlNode node in nodeList)
        {
            string objectDefinition;

            XmlNode? definitionNode = node.ParentNode?.SelectSingleNode("@CadastralNumber|@Definition|@Cadastral_Number");

            objectDefinition = definitionNode?.Value ?? string.Empty;

            entities.Add(new EntitySpatial() { SpatialElements = GetSpatialElements(node), ParentTitle = objectDefinition });
        }
        return entities;
    }

    private List<SpatialElement> GetSpatialElements(XmlNode xmlNode)
    {
        XmlNodeList? elementNodes = xmlNode.SelectNodes(".//SpatialElement|.//Spatial_Element|.//spatials_elements|.//*[local-name() = 'SpatialElement']");

        if (elementNodes == null) throw new Exception("Cannot get spatial elements nodes");

        List<SpatialElement> spatialElements = new List<SpatialElement>();

        foreach (XmlNode elementNode in elementNodes)
        {
            spatialElements.Add(new SpatialElement() { SpelementUnits = GetSpelementUnits(elementNode) });
        }

        return spatialElements;
    }

    private List<SpelementUnit> GetSpelementUnits(XmlNode xmlNode)
    {
        XmlNodeList? nodeList = xmlNode.SelectNodes(".//SpelementUnit|.//Spelement_Unit|.//ordinate|.//*[local-name() = 'SpelementUnit']");

        if (nodeList == null) throw new Exception("Cannot get spelement units nodes");

        List<SpelementUnit> spelementUnits = new();

        for (int i = 0; i < nodeList.Count; i++)
        {
            XmlNode node = nodeList[i]!;
            spelementUnits.Add(new SpelementUnit(GetOrdinate(node, i + 1)));
        }

        return spelementUnits;
    }

    /// <summary>
    /// Get coordinate data from document XmlNode
    /// </summary>
    /// <param name="xmlNode">Node with coordinate data</param>
    /// <param name="pointNumber">Point number is node doesn't have</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">No Ordinate|NewOrdinate|New_Ordinate nodes</exception>
    /// <exception cref="Exception">X or Y value is missing</exception>
    private Ordinate GetOrdinate(XmlNode xmlNode, int pointNumber = 0)
    {
        XmlNode? node = xmlNode.SelectSingleNode(".//Ordinate|.//NewOrdinate|.//New_Ordinate|.//*[local-name() = 'Ordinate']");

        if (node == null) node = xmlNode;

        XmlNode? xNode = node.SelectSingleNode("@X|x");
        XmlNode? yNode = node.SelectSingleNode("@Y|y");

        if (xNode == null || yNode == null) 
            throw new Exception("X or Y node is missing");

        string xValue = xNode.Value ?? xNode.InnerText;
        string yValue = yNode.Value ?? yNode.InnerText;

        if (xValue == null || yValue == null) 
            throw new Exception("X or Y node is missing");
        

        XmlNode? numberNode = node.SelectSingleNode("@NumGeopoint|@Num_Geopoint|ord_nmb|num_geopoint|@SuNmb");

        string? numberStringValue = numberNode?.Value ?? numberNode?.InnerText;
        int xmlPointNumber = (numberStringValue != null &&
            string.IsNullOrWhiteSpace(rxNonDigits.Replace(numberStringValue, ""))) ? 
            int.Parse(rxNonDigits.Replace(numberStringValue, "")) : pointNumber;

        Ordinate ordinate = new Ordinate()
        {
            X = double.Parse(xValue, CultureInfo.InvariantCulture),
            Y = double.Parse(yValue, CultureInfo.InvariantCulture),
            PointNumber = xmlPointNumber
        };

        XmlNode? radiusNode = node.SelectSingleNode("@Radius");

        ordinate.Radius = radiusNode != null ? double.Parse(radiusNode.Value!.ToString()) :
             null;

        if (node.SelectSingleNode("@PointPref|@Point_Pref") != null)
        {
            XmlNode? prefixNode = node.SelectSingleNode("@PointPref|@Point_Pref");
            ordinate.PointPrefix = prefixNode != null ? prefixNode.Value!.ToString().ToCharArray()[0]
                : null;
        };

        return ordinate;
    }
}