using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Elements.Geometry;

namespace Elements
{
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter() : base()
        {
        }
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

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
        private XDocument svgDoc;

        private List<Polyline> geometry;
        private List<Vector3> points;

        public void AddGeometry(Polyline p, string classes = null, Style style = null)
        {
            geometry.Add(p);
            var path = new XElement(xmlns +
                "path",
                new XAttribute("d", ToPath(p))
            );
            if (classes != null)
            {
                path.Add(new XAttribute("class", classes));
            }
            if (style != null)
            {
                path.Add(new XAttribute("style", style.ToString()));
            }
            svg.Add(path);
        }

        public void AddStyle(string selector, Style style)
        {
            var content = style.ToString();
            svg.Add(new XElement(xmlns + "style", new XAttribute("type", "text/css"), $"{selector} {{{content}}}"));
        }

        public void AddText(Vector3 location, string content, string classes = null, string anchor = null, Style style = null)
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
            if (style != null)
            {
                text.Add(new XAttribute("style", style.ToString()));
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
            var wr = new Utf8StringWriter();
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
            public double? StrokeWidth { get; set; } = null;
            public Color? Stroke { get; set; } = null;
            public Color? Fill { get; set; } = null;
            public string FontSize { get; set; } = null;
            public string FontFamily { get; set; } = null;

            public string TextAnchor { get; set; } = null;

            public string DominantBaseline { get; set; } = null;

            public Style() { }

            public Style(
                double? strokeWidth = null,
                Color? stroke = null,
                Color? fill = null,
                string fontSize = "",
                string fontFamily = ""
                )
            {
                this.StrokeWidth = strokeWidth;
                this.Stroke = stroke;
                this.Fill = fill;
                this.FontSize = fontSize;
                this.FontFamily = fontFamily;
            }

            public static string ToHex(Color color)
            {
                return $"#{(int)(color.Red * 255):X2}{(int)(color.Green * 255):X2}{(int)(color.Blue * 255):X2}";
            }

            public override string ToString()
            {
                var str = "";

                if (this.StrokeWidth != null)
                {
                    str += $"stroke-width: {this.StrokeWidth};";
                }
                if (this.Stroke != null)
                {
                    str += $"stroke: {ToHex((Color)this.Stroke)};";
                    str += $"stroke-opacity: {((Color)this.Stroke).Alpha};";
                }

                if (this.Fill != null)
                {
                    str += $"fill: {ToHex((Color)this.Fill)};";
                    str += $"fill-opacity: {((Color)this.Fill).Alpha};";
                }

                if (this.FontFamily != "")
                {
                    str += $"font-family: {this.FontFamily};";
                }

                if (this.FontSize != "")
                {
                    str += $"font-size: {this.FontSize};";
                }
                if (this.TextAnchor != null)
                {
                    str += $"text-anchor: {this.TextAnchor};";
                }
                if (this.DominantBaseline != null)
                {
                    str += $"dominant-baseline: {this.DominantBaseline};";
                }

                return str;
            }
        }
    }
}