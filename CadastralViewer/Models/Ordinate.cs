using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace CadastralViewer.Models;
public class Ordinate
{
    /// <summary>
    /// Coordinate X
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Coordinate Y
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Point number to show
    /// </summary>
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
            AttributesTable attributesTable = new()
                {
                    { "label", FullName }
                };
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