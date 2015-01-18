using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Camera.DirectShow
{
    public partial class CameraPropertyView : UserControl
    {
        public event EventHandler ValueChanged;

        public CameraProperty Property
        {
            get { return property; }
        }

        private CameraProperty property;
        private string localizationToken;
        private bool updatingValue;
        private Func<int, string> valueMapper;
        
        public CameraPropertyView(CameraProperty property, string localizationToken, Func<int, string> valueMapper)
        {
            this.property = property;
            this.localizationToken = localizationToken;
            this.valueMapper = valueMapper;

            InitializeComponent();

            // TODO: retrieve localized name from the localization token.
            lblName.Text = localizationToken;

            this.Enabled = property.Supported;

            if (property.Supported)
                Populate();
        }

        private void Populate()
        {

            tbValue.Minimum = property.Minimum;
            tbValue.Maximum = property.Maximum;

            updatingValue = true;
            tbValue.Value = property.Value;
            cbAuto.Checked = property.Automatic;
            lblValue.Text = valueMapper(property.Value);
            updatingValue = false;
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            if (tbValue.Value < property.Minimum || tbValue.Value > property.Maximum)
                return;

            property.Value = tbValue.Value;
            lblValue.Text = valueMapper(property.Value);

            property.Automatic = false;
            
            updatingValue = true;
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        private void cbAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            property.Automatic = cbAuto.Checked;

            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }
    }
}
