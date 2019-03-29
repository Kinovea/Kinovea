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
        private static Func<int, string> defaultValueMapper = (value) => value.ToString();

        public CameraPropertyLogarithmicView(CameraProperty property, string localizationToken, Func<int, string> valueMapper)
        {
            this.property = property;
            this.localizationToken = localizationToken;
            bool useDefaultMapper = valueMapper == null;
            this.valueMapper = useDefaultMapper ? defaultValueMapper : valueMapper;

            InitializeComponent();
            lblValue.Left = nud.Left;

            // TODO: retrieve localized name from the localization token.
            lblName.Text = localizationToken;

            if (property.Supported)
                Populate();
            else
                this.Enabled = false;

            nud.Visible = useDefaultMapper;
            lblValue.Visible = !useDefaultMapper;
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
            int sliderMin = 1;
            int sliderMax = 10000;
            logMapper = new LogarithmicMapper(minValue, maxValue, sliderMin, sliderMax);

            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            int sliderValue = logMapper.Map((int)value);

            updatingValue = true;
            nud.Minimum = minValue;
            nud.Maximum = maxValue;
            nud.Value = (int)value;
            tbValue.Minimum = sliderMin;
            tbValue.Maximum = sliderMax;
            tbValue.Value = sliderValue;
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            lblValue.Text = valueMapper((int)value);
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            int numericValue = logMapper.Unmap(tbValue.Value);
            string strValue = numericValue.ToString(CultureInfo.InvariantCulture);

            property.CurrentValue = strValue;
            lblValue.Text = valueMapper(numericValue);

            property.Automatic = false;
            
            updatingValue = true;
            cbAuto.Checked = property.Automatic;
            nud.Value = numericValue;
            updatingValue = false;

            RaiseValueChanged();
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            int numericValue = (int)nud.Value;
            string strValue = numericValue.ToString(CultureInfo.InvariantCulture);

            property.CurrentValue = strValue;
            lblValue.Text = valueMapper(numericValue);

            property.Automatic = false;

            updatingValue = true;
            cbAuto.Checked = property.Automatic;
            tbValue.Value = logMapper.Map(numericValue);
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
