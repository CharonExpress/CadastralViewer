using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Utilities;
using System.Xml;
using System.Globalization;

namespace CadastralViewer.Models;
public class SpatialData
{

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
        XmlNodeList? nodeList = xmlDocument.SelectNodes(".//EntitySpatial|.//Entity_Spatial");

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
        XmlNodeList? elementNodes = xmlNode.SelectNodes(".//SpatialElement|.//Spatial_Element");

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
        XmlNodeList? nodeList = xmlNode.SelectNodes(".//SpelementUnit|.//Spelement_Unit");

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
        XmlNode? node = xmlNode.SelectSingleNode(".//Ordinate|.//NewOrdinate|.//New_Ordinate");

        if (node == null) throw new NullReferenceException("Coordinate data node is missing");

        XmlNode? xNode = node.SelectSingleNode("@X");
        XmlNode? yNode = node.SelectSingleNode("@Y");

        if (xNode == null || yNode == null ||
            xNode.Value == null || yNode.Value == null) throw new Exception("X or Y node is missing");

        XmlNode? numberNode = node.SelectSingleNode("@NumGeopoint|@Num_Geopoint");

        Ordinate ordinate = new Ordinate()
        {
            X = double.Parse(xNode.Value.ToString(), CultureInfo.InvariantCulture),
            Y = double.Parse(yNode.Value.ToString(), CultureInfo.InvariantCulture),

            PointNumber = numberNode?.Value != null ?
            int.Parse(numberNode.Value.ToString()) : pointNumber
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