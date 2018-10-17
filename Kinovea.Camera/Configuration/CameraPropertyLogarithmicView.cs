using System;
using System.Windows.Forms;
using System.Globalization;

namespace Kinovea.Camera
{
    public partial class CameraPropertyLogarithmicView : AbstractCameraPropertyView
    {
        private string localizationToken;
        private bool updatingValue;
        private LogarithmicMapper logMapper;
        private Func<int, string> valueMapper;

        public CameraPropertyLogarithmicView(CameraProperty property, string localizationToken, Func<int, string> valueMapper)
        {
            this.property = property;
            this.localizationToken = localizationToken;
            this.valueMapper = valueMapper;

            InitializeComponent();

            // TODO: retrieve localized name from the localization token.
            lblName.Text = localizationToken;

            if (property.Supported)
                Populate();
            else
                this.Enabled = false;
        }

        public override void Repopulate(CameraProperty property)
        {
            this.property = property;
            if (property.Supported)
                Populate();
        }

        private void Populate()
        {
            cbAuto.Enabled = property.CanBeAutomatic;

            double min = double.Parse(property.Minimum, CultureInfo.InvariantCulture);
            double max = double.Parse(property.Maximum, CultureInfo.InvariantCulture);
            int minValue = (int)Math.Max(1, min);
            int maxValue = (int)max;

            tbValue.Minimum = 1;
            tbValue.Maximum = 10000;

            logMapper = new LogarithmicMapper(minValue, maxValue, tbValue.Minimum, tbValue.Maximum);

            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);

            updatingValue = true;
            tbValue.Value = logMapper.Map((int)value);
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            lblValue.Text = valueMapper((int)value);
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            int value = logMapper.Unmap(tbValue.Value);

            property.CurrentValue = value.ToString(CultureInfo.InvariantCulture);
            lblValue.Text = valueMapper(value);

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
