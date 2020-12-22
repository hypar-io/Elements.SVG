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
            points = new List<Vector3>();
        }
        private XNamespace xmlns;
        private XElement svg;
        private XElement css;
        private XDocument svgDoc;

        private List<Polyline> geometry;
        private List<Vector3> points;

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

        public void AddCss(string selector, List<(string, string)> values)
        {
            var content = "";
            foreach (var val in values)
            {
                content += $"{val.Item1}:{val.Item2};";
            }
            svg.Add(new XElement(xmlns + "style", new XAttribute("type", "text/css"), $"{selector} {{{content}}}"));
        }

        public void AddText(Vector3 location, string content, string classes = null, string anchor = null)
        {
            points.Add(location);
            var text = new XElement(xmlns + "text", content);
            text.Add(new XAttribute("x", location.X));
            text.Add(new XAttribute("y", -location.Y));

            if (classes != null)
            {
                text.Add(new XAttribute("class", classes));
            }
            if (anchor != null)
            {
                text.Add(new XAttribute("text-anchor", anchor));
            }
            svg.Add(text);
        }

        public string ToPath(Polyline p)
        {
            var resultString = "M";
            foreach (var vertex in p.Vertices)
            {
                resultString += $" {vertex.X},{-vertex.Y}";
            }
            if (p is Polygon pg)
            {
                resultString += $" {pg.Vertices[0].X},{-pg.Vertices[0].Y}";
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
            var bbox = new BBox3(new List<Vector3>(geometry.SelectMany(p => p.Vertices).ToList().Concat(points)));
            var viewBox = $"{bbox.Min.X} {-bbox.Max.Y} {bbox.Max.X - bbox.Min.X} {bbox.Max.Y - bbox.Min.Y}";
            svg.RemoveAttributes();
            svg.Add(new XAttribute("viewBox", viewBox));
        }

        public class Style
        {
            public double StrokeWidth { get; set; } = 1;
            public bool EnableFill { get; set; } = true;
            public bool EnableStroke { get; set; } = true;
            public Color Stroke { get; set; } = Colors.Black;
            public Color Fill { get; set; } = Colors.Black;

            public Style(double strokeWidth = 1, bool enableFill = true, bool enableStroke = true, Color stroke = new Color(), Color fill = new Color())
            {
                this.StrokeWidth = strokeWidth;
                this.EnableFill = enableFill;
                this.EnableStroke = enableStroke;
                this.Stroke = stroke;
                this.Fill = fill;
            }

            public static string ToHex(Color color)
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
