using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class ChronoManager : AbstractDrawingManager
    {
        public override Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        private List<DrawingChrono> chronos = new List<DrawingChrono>();
        private Guid id = Guid.NewGuid();
        private Metadata metadata;

        public override AbstractDrawing GetDrawing(Guid id)
        {
            return chronos.FirstOrDefault(c => c.Id == id);
        }

        public override void AddDrawing(AbstractDrawing drawing)
        {
            if (!(drawing is DrawingChrono))
                return;

            chronos.Add(drawing as DrawingChrono);
        }

        public override void RemoveDrawing(Guid id)
        {
            chronos.RemoveAll(c => c.Id == id);

            // Temporary hack while the list of chronometers is duplicated between the chronoManager and the extra drawing.
            metadata.ExtraDrawings.RemoveAll(c => c.Id == id);
        }

        public void SetMetadata(Metadata metadata)
        {
            // Temporary hack while the list of chronometers is duplicated between the chronoManager and the extra drawing.
            this.metadata = metadata;
        }
    }
}
