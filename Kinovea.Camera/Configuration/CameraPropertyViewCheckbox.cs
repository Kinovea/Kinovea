using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.Camera
{
    public partial class CameraPropertyViewCheckbox : AbstractCameraPropertyView
    {
        public CameraPropertyViewCheckbox(CameraProperty property, string text)
        {
            this.prop = property;

            InitializeComponent();
            cb.Text = text;

            if (property.Supported)
                Populate();
            else
                this.Enabled = false;
        }

        public override void Repopulate(CameraProperty property)
        {
            this.prop = property;
            if (property.Supported)
                Populate();
        }

        private void Populate()
        {
            bool value = bool.Parse(prop.CurrentValue);
            cb.Checked = value;
        }
        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            prop.CurrentValue = cb.Checked ? "true" : "false";
            RaiseValueChanged();
        }

    }
}
