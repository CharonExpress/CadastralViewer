using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using System.Xml;

namespace CadastralViewer.Models;
public class SpatialElement
{
    const int circlePointsCount = 40;

    public SpatialElement() => SpelementUnits = new();

    public SpatialElement(IEnumerable<SpelementUnit> units) : this()
        => SpelementUnits.AddRange(units);

    public List<SpelementUnit> SpelementUnits { get; set; }

    public GeometryTypes GeometryType
    {
        get
        {
            if (SpelementUnits.Count == 1 &&
                SpelementUnits.First().Ordinate.Radius != null) return GeometryTypes.Circle;

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
                        Size = SpelementUnits.Single().Ordinate.Radius!.Value,
                        NumPoints = circlePointsCount
                    }).CreateCircle();
                default:
                    throw new TopologyException("Input geometry is not valid");
            }
        }
    }
}