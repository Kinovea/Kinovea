using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class DrawingToolSeparator : AbstractDrawingTool
    {
        public override string Name => throw new NotImplementedException();

        public override string DisplayName => throw new NotImplementedException();

        public override Bitmap Icon => throw new NotImplementedException();

        public override bool Attached => throw new NotImplementedException();

        public override bool KeepTool => throw new NotImplementedException();

        public override bool KeepToolFrameChanged => throw new NotImplementedException();

        public override StyleElements StyleElements { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override StyleElements DefaultStyleElements => throw new NotImplementedException();

        public override AbstractDrawing GetNewDrawing(PointF origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }
}
