using CadastralViewer.Models;

namespace CadastralViewerTests
{
    [TestFixture]
    public class SpatialElementTest
    {
        private static Ordinate ordinate1 = new() { X = 0, Y = 0 };
        private static Ordinate ordinate2 = new() { X = 1, Y = 2 };
        private static Ordinate ordinate3 = new() { X = 3, Y = 3, Radius = 999 };
        private static Ordinate ordinate4 = new() { X = 0, Y = 0 };

        private static IEnumerable<Tuple<List<SpelementUnit>, GeometryTypes>> TestGeometryTypesTestData()
        {
            yield return new(new() { new(ordinate1) }, GeometryTypes.Point);
            yield return new(new() { new(ordinate1), new(ordinate2), new(ordinate3), new(ordinate4) },
                GeometryTypes.Polygon);
            yield return new(new() { new(ordinate1), new(ordinate2), new(ordinate3) },
                GeometryTypes.LineString);
            yield return new(new() { new(ordinate3) }, GeometryTypes.Circle);
        }


        [Test, TestCaseSource(nameof(TestGeometryTypesTestData))]
        public void TestGeometryTypes(Tuple<List<SpelementUnit>, GeometryTypes> tuple)
        {
            SpatialElement element = new(tuple.Item1);
            Assert.That(element.GeometryType == tuple.Item2);
        }
    }
}
