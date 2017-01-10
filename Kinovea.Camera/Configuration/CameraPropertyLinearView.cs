using System;
using System.Windows.Forms;
using System.Globalization;
using Kinovea.Camera.Languages;

namespace Kinovea.Camera
{
    public partial class CameraPropertyLinearView : AbstractCameraPropertyView
    {
        private bool updatingValue;
        private Func<int, string> valueMapper;

        public CameraPropertyLinearView(CameraProperty property, string text, Func<int, string> valueMapper)
        {
            this.property = property;
            this.valueMapper = valueMapper;

            InitializeComponent();

            // TODO: retrieve localized name from the localization token.
            lblName.Text = text;

            this.Enabled = property.Supported;

            if (property.Supported)
                Populate();
        }

        private void Populate()
        {
            // FIXME: doesn't play well with non integer values.

            cbAuto.Enabled = property.CanBeAutomatic;
            
            tbValue.Minimum = (int)double.Parse(property.Minimum, CultureInfo.InvariantCulture);
            tbValue.Maximum = (int)double.Parse(property.Maximum, CultureInfo.InvariantCulture);
            int value = (int)double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            
            updatingValue = true;
            tbValue.Value = Math.Min(Math.Max(value, tbValue.Minimum), tbValue.Maximum);
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            // We allow the change of the properties even if they are readonly.
            // Sometimes the properties are readonly because we are currently streaming, but we'll have a chance to write them during the disconnection/reconnection.
            //tbValue.Enabled = !property.ReadOnly;

            lblValue.Text = valueMapper(value);
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            property.CurrentValue = tbValue.Value.ToString(CultureInfo.InvariantCulture);
            lblValue.Text = valueMapper(tbValue.Value);

            property.Automatic = false;
            
            updatingValue = true;
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            RaiseValueChanged();
        }

        private void cbAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            property.Automatic = cbAuto.Checked;

            RaiseValueChanged();
        }
    }
}
