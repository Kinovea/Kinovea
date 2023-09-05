using System;
using System.Windows.Forms;
using System.Globalization;
using Kinovea.Camera.Languages;
using Kinovea.Services;

namespace Kinovea.Camera
{
    public partial class CameraPropertyViewLinear : AbstractCameraPropertyView
    {
        private bool updatingValue;
        private Func<int, string> valueMapper;
        private static Func<int, string> defaultValueMapper = (value) => value.ToString();

        public CameraPropertyViewLinear(CameraProperty property, string text, Func<int, string> valueMapper)
        {
            this.prop = property;

            bool useDefaultMapper = valueMapper == null;
            this.valueMapper = useDefaultMapper ? defaultValueMapper : valueMapper;;
            
            InitializeComponent();
            lblValue.Left = nud.Left;

            // TODO: retrieve localized name from the localization token.
            lblName.Text = text;

            if (property.Supported)
                Populate();
            else
                this.Enabled = false;

            nud.Visible = useDefaultMapper;
            lblValue.Visible = !useDefaultMapper;
            NudHelper.FixNudScroll(nud);
        }

        public override void Repopulate(CameraProperty property)
        {
            this.prop = property;
            if (property.Supported)
                Populate();
        }

        private void Populate()
        {
            // FIXME: doesn't play well with non integer values.

            cbAuto.Enabled = prop.CanBeAutomatic;
            
            int min = (int)double.Parse(prop.Minimum, CultureInfo.InvariantCulture);
            int max = (int)double.Parse(prop.Maximum, CultureInfo.InvariantCulture);
            int value = (int)double.Parse(prop.CurrentValue, CultureInfo.InvariantCulture);
            value = Math.Min(max, Math.Max(min, value));
            
            updatingValue = true;
            nud.Minimum = min;
            nud.Maximum = max;
            nud.Value = value;
            nud.Increment = 1;
            tbValue.Minimum = min;
            tbValue.Maximum = max;
            tbValue.Value = value;
            cbAuto.Checked = prop.Automatic;
            updatingValue = false;

            lblValue.Text = valueMapper(value);
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            int numericValue = tbValue.Value;
            string strValue = numericValue.ToString(CultureInfo.InvariantCulture);

            prop.CurrentValue = strValue;
            lblValue.Text = valueMapper(numericValue);

            prop.Automatic = false;
            
            updatingValue = true;
            cbAuto.Checked = prop.Automatic;
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

            prop.CurrentValue = strValue;
            lblValue.Text = valueMapper(numericValue);

            prop.Automatic = false;

            updatingValue = true;
            cbAuto.Checked = prop.Automatic;
            tbValue.Value = numericValue;
            updatingValue = false;

            RaiseValueChanged();
        }

        private void cbAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            prop.Automatic = cbAuto.Checked;

            RaiseValueChanged();
        }
    }
}
