using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace CadastralViewer.Models;
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