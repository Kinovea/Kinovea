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

        public List<AbstractDrawing> Drawings
        {
            get { return chronos; }
        }

        private List<AbstractDrawing> chronos = new List<AbstractDrawing>();
        private Guid id = Guid.NewGuid();

        public override AbstractDrawing GetDrawing(Guid id)
        {
            return chronos.FirstOrDefault(c => c.Id == id);
        }

        public override void AddDrawing(AbstractDrawing drawing)
        {
            if (!(drawing is DrawingChrono))
                return;

            chronos.Add(drawing);
        }

        public override void RemoveDrawing(Guid id)
        {
            chronos.RemoveAll(c => c.Id == id);
        }
    }
}
