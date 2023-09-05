using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A generic and bare bone drawing manager, which is basically a collection of drawings.
    /// </summary>
    public class DrawingManager<T> : AbstractDrawingManager
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

        private List<AbstractDrawing> drawings = new List<AbstractDrawing>();
        private Guid id = Guid.NewGuid();

        public override AbstractDrawing GetDrawing(Guid id)
        {
            return drawings.FirstOrDefault(c => c.Id == id);
        }

        public override void AddDrawing(AbstractDrawing drawing)
        {
            if (!(drawing is T))
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

