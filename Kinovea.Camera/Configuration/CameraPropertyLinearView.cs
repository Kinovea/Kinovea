using System;
using System.Windows.Forms;
using System.Globalization;

namespace Kinovea.Camera
{
    public partial class CameraPropertyLinearView : AbstractCameraPropertyView
    {
        private string localizationToken;
        private bool updatingValue;
        private Func<int, string> valueMapper;
        
        public CameraPropertyLinearView(CameraProperty property, string localizationToken, Func<int, string> valueMapper)
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
            cbAuto.Enabled = property.CanBeAutomatic;
            
            tbValue.Minimum = int.Parse(property.Minimum, CultureInfo.InvariantCulture);
            tbValue.Maximum = int.Parse(property.Maximum, CultureInfo.InvariantCulture);
            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            
            updatingValue = true;
            tbValue.Value = value;
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            tbValue.Enabled = !property.ReadOnly;

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
