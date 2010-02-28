using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Xml;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Css;

namespace SharpVectors.Renderer.Gdi
{
    public class PatternPaintServer : PaintServer
    {
        public PatternPaintServer(SvgPatternElement patternElement)
        {
            _patternElement = patternElement;
        }

        private SvgPatternElement _patternElement;

        #region Private methods
        private XmlElement oldParent;
        private SvgSvgElement moveIntoSvgElement()
        {
            SvgDocument doc = _patternElement.OwnerDocument;
            SvgSvgElement svgElm = doc.CreateElement("", "svg", SvgDocument.SvgNamespace) as SvgSvgElement;

            XmlNodeList children = _patternElement.Children;
            if (children.Count > 0)
            {
                oldParent = children[0].ParentNode as XmlElement;
            }

            for (int i = 0; i < children.Count; i++)
            {
                svgElm.AppendChild(children[i]);
            }

            if (_patternElement.HasAttribute("viewBox"))
            {
                svgElm.SetAttribute("viewBox", _patternElement.GetAttribute("viewBox"));
            }
            svgElm.SetAttribute("x", "0");
            svgElm.SetAttribute("y", "0");
            svgElm.SetAttribute("width", _patternElement.GetAttribute("width"));
            svgElm.SetAttribute("height", _patternElement.GetAttribute("height"));

            if (_patternElement.PatternContentUnits.AnimVal.Equals(SvgUnitType.ObjectBoundingBox))
            {
                svgElm.SetAttribute("viewBox", "0 0 1 1");
            }

            _patternElement.AppendChild(svgElm);

            return svgElm;
        }

        private void moveOutOfSvgElement(SvgSvgElement svgElm)
        {
            while (svgElm.ChildNodes.Count > 0)
            {
                oldParent.AppendChild(svgElm.ChildNodes[0]);
            }

            _patternElement.RemoveChild(svgElm);
        }

        private Image getImage(RectangleF bounds)
        {
            GdiRenderer renderer = new GdiRenderer();
            renderer.Window = _patternElement.OwnerDocument.Window as SvgWindow;

            SvgSvgElement elm = moveIntoSvgElement();

            Image img = renderer.Render(elm as SvgElement);

            moveOutOfSvgElement(elm);

            return img;
        }


        private float calcPatternUnit(SvgLength length, SvgLengthDirection dir, RectangleF bounds)
        {
            int patternUnits = _patternElement.PatternUnits.AnimVal;
            if (patternUnits == (int)SvgUnitType.UserSpaceOnUse)
            {
                return (float)length.Value;
            }
            else
            {
                float calcValue = (float)length.ValueInSpecifiedUnits;
                if (dir == SvgLengthDirection.Horizontal)
                {
                    calcValue *= bounds.Width;
                }
                else
                {
                    calcValue *= bounds.Height;
                }
                if (length.UnitType == SvgLengthType.Percentage)
                {
                    calcValue /= 100F;
                }
                return calcValue;
            }
        }

        private RectangleF getDestRect(RectangleF bounds)
        {
            RectangleF result = new RectangleF(0, 0, 0, 0);
            result.Width = calcPatternUnit(_patternElement.Width.AnimVal as SvgLength, SvgLengthDirection.Horizontal, bounds);
            result.Height = calcPatternUnit(_patternElement.Height.AnimVal as SvgLength, SvgLengthDirection.Vertical, bounds);

            return result;
        }

        private Matrix getTransformMatrix(RectangleF bounds)
        {
            SvgMatrix svgMatrix = ((SvgTransformList)_patternElement.PatternTransform.AnimVal).TotalMatrix;

            Matrix transformMatrix = new Matrix(
                (float)svgMatrix.A,
                (float)svgMatrix.B,
                (float)svgMatrix.C,
                (float)svgMatrix.D,
                (float)svgMatrix.E,
                (float)svgMatrix.F);

            float translateX = calcPatternUnit(_patternElement.X.AnimVal as SvgLength, SvgLengthDirection.Horizontal, bounds);
            float translateY = calcPatternUnit(_patternElement.Y.AnimVal as SvgLength, SvgLengthDirection.Vertical, bounds);

            transformMatrix.Translate(translateX, translateY, MatrixOrder.Prepend);
            return transformMatrix;
        }
        #endregion

        #region Public methods
        public override Brush GetBrush(RectangleF bounds)
        {
            Image image = getImage(bounds);
            RectangleF destRect = getDestRect(bounds);

            TextureBrush tb = new TextureBrush(image, destRect);
            tb.Transform = getTransformMatrix(bounds);
            return tb;
        }
        #endregion
    }
}
