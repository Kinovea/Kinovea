using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class TrackManager : AbstractDrawingManager
    {
        public override Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        public List<AbstractDrawing> Drawings
        {
            get { return drawings; }
        }

        public bool Tracking
        {
            get { return Drawings.Any(t => ((DrawingTrack)t).Status == TrackStatus.Edit); }
        }

        private List<AbstractDrawing> drawings = new List<AbstractDrawing>();
        private Guid id = Guid.NewGuid();

        public override AbstractDrawing GetDrawing(Guid id)
        {
            return drawings.FirstOrDefault(c => c.Id == id);
        }

        public override void AddDrawing(AbstractDrawing drawing)
        {
            if (!(drawing is DrawingTrack))
                return;

            drawings.Add(drawing);
        }

        public override void RemoveDrawing(Guid id)
        {
            drawings.RemoveAll(c => c.Id == id);
        }

        public override void Clear()
        {
            drawings.Clear();
        }
    }
}
