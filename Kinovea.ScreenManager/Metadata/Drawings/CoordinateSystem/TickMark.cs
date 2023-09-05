using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TickMark
    {
        public float Value { get; private set; }
        public PointF ImageLocation { get; private set; }
        public TextAlignment TextAlignment { get; private set; }

        public TickMark(float value, PointF imageLocation, TextAlignment textAlignment)
        {
            this.Value = value;
            this.ImageLocation = imageLocation;
            this.TextAlignment = textAlignment;
        }

        public void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer,
            SolidBrush brushFill, SolidBrush fontBrush, Font font, int margin, bool precise)
        {
            string label = "";
            if (precise)
                label = String.Format("{0:0.000}", Value);
            else
                label = String.Format("{0}", Math.Round(Value, 3));

            PointF location;
            if (distorter != null && distorter.Initialized)
                location = distorter.Distort(ImageLocation);
            else
                location = ImageLocation;

            PointF transformed = transformer.Transform(location);
            SizeF labelSize = canvas.MeasureString(label, font);
            PointF textPosition = GetTextPosition(transformed, TextAlignment, labelSize, margin);
            RectangleF backRectangle = new RectangleF(textPosition, labelSize);

            RoundedRectangle.Draw(canvas, backRectangle, brushFill, font.Height / 4, false, false, null);
            canvas.DrawString(label, font, fontBrush, backRectangle.Location);
        }

        private PointF GetTextPosition(PointF tickPosition, TextAlignment textAlignment, SizeF textSize, int textMargin)
        {
            PointF textPosition = tickPosition;

            switch (textAlignment)
            {
                case TextAlignment.Top:
                    textPosition = new PointF(tickPosition.X - textSize.Width / 2, tickPosition.Y - textSize.Height - textMargin);
                    break;
                case TextAlignment.Left:
                    textPosition = new PointF(tickPosition.X - textSize.Width - textMargin, tickPosition.Y - textSize.Height / 2);
                    break;
                case TextAlignment.Right:
                    textPosition = new PointF(tickPosition.X + textMargin, tickPosition.Y - textSize.Height / 2);
                    break;
                case TextAlignment.Bottom:
                    textPosition = new PointF(tickPosition.X - textSize.Width / 2, tickPosition.Y + textMargin);
                    break;
                case TextAlignment.BottomRight:
                    textPosition = new PointF(tickPosition.X + textMargin, tickPosition.Y + textMargin);
                    break;
            }

            return textPosition;
        }
    }
}
