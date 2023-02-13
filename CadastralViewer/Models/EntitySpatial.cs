using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace CadastralViewer.Models;
public class EntitySpatial
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

            var pointsFeatures = SpatialElements.SelectMany(se =>
            se.SpelementUnits.Select(su => su.Feature)).ToList();

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