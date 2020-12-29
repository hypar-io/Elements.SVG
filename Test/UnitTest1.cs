using System;
using Xunit;
using Elements;
using Elements.Geometry;

namespace Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var shape = Polygon.Rectangle(5, 8);
            var svg = new SVG();
            svg.AddGeometry(shape, new SVG.Style
            {
                Fill = Colors.Red,
                Stroke = Colors.Blue,
                StrokeWidth = 2
            });
            svg.AddGeometry(Polygon.Rectangle(10, 10), new SVG.Style
            {
                // EnableFill = false,
                // EnableStroke = false
            });
            var svgString = svg.SvgString();
            Console.WriteLine("🔵");
            Console.WriteLine(svgString);
        }
    }
}
