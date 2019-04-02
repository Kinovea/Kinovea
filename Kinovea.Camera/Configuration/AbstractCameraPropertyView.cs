using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Camera
{
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<AbstractCameraPropertyView, UserControl>))]
    public abstract class AbstractCameraPropertyView : UserControl
    {
        public event EventHandler ValueChanged;

        public CameraProperty Property
        {
            get { return property; }
        }

        protected CameraProperty property;
        
        protected void RaiseValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        public abstract void Repopulate(CameraProperty property);

    }
}
