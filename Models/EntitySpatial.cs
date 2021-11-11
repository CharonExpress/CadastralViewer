using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Utilities;
using System.Xml;

namespace CadastralViewer
{
    public class SpatialData
    {

        public List<Tuple<FeatureCollection, string>> TitledFeatureCollections;

        public SpatialData()
        {
            TitledFeatureCollections = new List<Tuple<FeatureCollection,string>>();
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
            XmlNodeList nodeList = xmlDocument.SelectNodes(".//EntitySpatial|.//Entity_Spatial");

            if (nodeList == null) throw new Exception("Cannot get entity spatial nodes");

            List<EntitySpatial> entities = new List<EntitySpatial>();

            foreach (XmlNode node in nodeList)
            {
                string objectDefinition = "";

                if (node.ParentNode.SelectSingleNode("@CadastralNumber|@Definition|@Cadastral_Number") != null)
                { objectDefinition = node.ParentNode.SelectSingleNode("@CadastralNumber|@Definition|@Cadastral_Number").Value; };

                entities.Add(new EntitySpatial() { SpatialElements = GetSpatialElements(node), ParentTitle = objectDefinition });
            }
            return entities;
        }

        private List<SpatialElement> GetSpatialElements(XmlNode xmlNode)
        {
            XmlNodeList elementNodes = xmlNode.SelectNodes(".//SpatialElement|.//Spatial_Element");

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
            XmlNodeList nodeList = xmlNode.SelectNodes(".//SpelementUnit|.//Spelement_Unit");

            if (nodeList == null) throw new Exception("Cannot get spelement units nodes");

            List<SpelementUnit> spelementUnits = new();

            foreach (XmlNode node in nodeList)
            {
                spelementUnits.Add(new SpelementUnit(GetOrdinate(node)));
            }
            return spelementUnits;
        }

        private Ordinate GetOrdinate(XmlNode xmlNode)
        {
            XmlNode node = xmlNode.SelectSingleNode(".//Ordinate|.//NewOrdinate|.//New_Ordinate");

            if (node == null) throw new Exception("Cannot get ordinate node value");

            Ordinate ordinate = new Ordinate()
            {
                X = double.Parse(node.SelectSingleNode("@X").Value.ToString()),
                Y = double.Parse(node.SelectSingleNode("@Y").Value.ToString()),
                PointNumber = int.Parse(node.SelectSingleNode("@NumGeopoint|@Num_Geopoint").Value.ToString())
            };

            if (node.SelectSingleNode("@Radius") != null) { ordinate.Radius = double.Parse(node.SelectSingleNode("@Radius").Value.ToString()); };

            if (node.SelectSingleNode("@PointPref|@Point_Pref") != null)
            {
                ordinate.PointPrefix = node.SelectSingleNode("@PointPref|@Point_Pref").Value.ToString().ToCharArray()[0];
            };

            return ordinate;
        }
    }
    
    internal class EntitySpatial
    {
        public List<SpatialElement> SpatialElements { get; set; } = new();

        /// <summary>
        /// Title to disply (52:22:XXXXXXX:XX:ЗУN)
        /// </summary>
        public string ParentTitle { get; set; } = "Geometry";

        public Geometry Geometry
        {
            get
            {
                var geometryTypes = SpatialElements.Select(x => x.GeometryType).Distinct().ToList();

                if (geometryTypes.Count == 1 && geometryTypes.Single() == GeometryTypes.Polygon)
                {
                    LinearRing outerShell = (LinearRing)SpatialElements.First().Geometry;
                    Polygon polygon;

                    if (SpatialElements.Count > 1)
                    {
                        polygon = new Polygon(outerShell, SpatialElements.Skip(1).Select(x => (LinearRing)x.Geometry).ToArray());
                    }
                    else
                    {
                        polygon = new Polygon(outerShell);
                    }

                    return polygon;

                }
                return new GeometryCollection(SpatialElements.Select(g => g.Geometry).ToArray());
            }
        }

        public Feature Feature
        {
            get
            {
                return new Feature(Geometry, AttributeTable);
            }
        }

        public FeatureCollection FeatureCollection
        {
            get
            {
                FeatureCollection featureCollection = new FeatureCollection(); 
                featureCollection.Add(Feature);

                var pointsFeatures = SpatialElements.SelectMany(se => se.SpelementUnits.Select(su => su.Feature)).ToList();

                foreach (Feature feature in pointsFeatures)
                {
                    featureCollection.Add(feature);
                }

               
                
                return featureCollection;
            }
        }  
        public AttributesTable AttributeTable
        {
            get
            {
                var attributeTable = new AttributesTable();
                attributeTable.Add("label", ParentTitle);
                return attributeTable;
            }
        }

        
    }

    public class SpatialElement
    {
   
        const int circlePointsCount = 40;
    
        public List<SpelementUnit> SpelementUnits { get; set; } = new();
    
        public GeometryTypes GeometryType
        {
            get
            {
                if (SpelementUnits.Count == 1 && SpelementUnits.First().Ordinate.Radius != null) return GeometryTypes.Circle;

                var firstOrdinate = SpelementUnits.First().Ordinate;
                var lastOrdinate = SpelementUnits.Last().Ordinate;

                if (firstOrdinate.X == lastOrdinate.X && firstOrdinate.Y == lastOrdinate.Y && SpelementUnits.Count > 3) return GeometryTypes.Polygon;

                if (SpelementUnits.Count > 1) return GeometryTypes.LineString;

                return GeometryTypes.Point;
            }
        }
       
        public Geometry Geometry
        {
            get
            {
                switch (GeometryType)
                {
                    case GeometryTypes.Point:
                        return SpelementUnits.Single().Point;
                    case GeometryTypes.LineString:
                        return new LineString(SpelementUnits.Select(spelementUnits => spelementUnits.Ordinate.Coordinate).ToArray());
                    case GeometryTypes.Polygon:
                        return new LinearRing(SpelementUnits.Select(spelementUnits => spelementUnits.Ordinate.Coordinate).ToArray());
                    case GeometryTypes.Circle:                        
                        return (new GeometricShapeFactory()
                        {
                            Centre = SpelementUnits.Single().Ordinate.Coordinate,
                            Size = SpelementUnits.Single().Ordinate.Radius.Value,
                            NumPoints = circlePointsCount
                        }).CreateCircle();
                    default:
                        throw new TopologyException("Input geometry is not valid");
                }
            }
        }

    }

    public class SpelementUnit
    {
        public Ordinate Ordinate { get; set; }

        public SpelementUnit(Ordinate ordinate)
        {
            Ordinate = ordinate;
        }
        
        public Point Point
        {
            get
            {
                return new Point(Ordinate.Coordinate);
            }
        }
         
        public Feature Feature
        {
            get
            {
                return new Feature(Point, Ordinate.AttributeTable);
            }
        }
    }

    public class Ordinate
    {
        public double X { get; set; }

        public double Y { get; set; }

        public int PointNumber { get; set; }

        /// <summary>
        /// Point prefix
        /// </summary>
        public char? PointPrefix { get; set; }

        /// <summary>
        /// Circel radius (if element is a circle)
        /// </summary>
        public double? Radius { get; set; }

        /// <summary>
        /// Point label
        /// </summary>
        public string FullName { get => PointPrefix + PointNumber.ToString(); }
        
        public AttributesTable AttributeTable
        {
            get
            {
                AttributesTable attributesTable = new();
                attributesTable.Add("label", FullName);
                return attributesTable;
            }
        }
        public Coordinate Coordinate
        {
            get
            {
                return new Coordinate(Y, X);
            }
        }
    }

    public enum GeometryTypes
    {
        Polygon = 0,
        LineString,
        Circle,
        Point
    }
}
