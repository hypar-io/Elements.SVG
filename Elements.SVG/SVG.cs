using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Elements.Geometry;

namespace Elements
{
    public class SVG
    {
        public SVG()
        {
            xmlns = "http://www.w3.org/2000/svg";
            svg = new XElement(xmlns + "svg");

            svgDoc = new XDocument(new XDeclaration("1.0", "utf-8", "no"), svg);
            geometry = new List<Polyline>();
        }
        private XNamespace xmlns;
        private XElement svg;
        private XDocument svgDoc;

        private List<Polyline> geometry;

        public void AddGeometry(Polyline p, Style style = null)
        {
            geometry.Add(p);
            var path = new XElement(xmlns +
                "path",
                new XAttribute("d", ToPath(p))
            );
            if (style != null)
            {
                path.Add(new XAttribute("style", style.ToString()));
            }
            svg.Add(path);
        }

        public string ToPath(Polyline p)
        {
            var resultString = "M";
            foreach (var vertex in p.Vertices)
            {
                resultString += $" {vertex.X},{vertex.Y}";
            }
            if (p is Polygon pg)
            {
                resultString += $" {pg.Vertices[0].X},{pg.Vertices[0].Y}";
            }
            return resultString;
        }
        public string SvgString()
        {
            CalcViewBox();
            var wr = new StringWriter();
            svgDoc.Save(wr);
            return wr.ToString();
        }

        private void CalcViewBox()
        {
            var bbox = new BBox3(geometry.SelectMany(p => p.Vertices).ToList());
            var viewBox = $"{bbox.Min.X} {bbox.Min.Y} {bbox.Max.X - bbox.Min.X} {bbox.Max.Y - bbox.Min.Y}";
            svg.Add(new XAttribute("viewBox", viewBox));
        }

        public class Style
        {
            public double StrokeWidth { get; set; } = 1;
            public bool EnableFill { get; set; } = true;
            public bool EnableStroke { get; set; } = true;
            public Color Stroke { get; set; } = Colors.Black;
            public Color Fill { get; set; } = Colors.Black;

            public string ToHex(Color color)
            {
                return $"#{(int)(color.Red * 255):X2}{(int)(color.Green * 255):X2}{(int)(color.Blue * 255):X2}";
            }

            public override string ToString()
            {
                return $"stroke:{(EnableStroke ? ToHex(Stroke) : "none")};" +
                       $"fill:{(EnableFill ? ToHex(Fill) : "none")};" +
                       $"fill-opacity:{Fill.Alpha};" +
                       $"stroke-opacity:{Stroke.Alpha};" +
                       $"stroke-width:{StrokeWidth}";
            }
        }
    }
}
