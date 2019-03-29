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
        private static Func<int, string> defaultValueMapper = (value) => value.ToString();

        public CameraPropertyLinearView(CameraProperty property, string text, Func<int, string> valueMapper)
        {
            this.property = property;

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
        }

        public override void Repopulate(CameraProperty property)
        {
            this.property = property;
            if (property.Supported)
                Populate();
        }

        private void Populate()
        {
            // FIXME: doesn't play well with non integer values.

            cbAuto.Enabled = property.CanBeAutomatic;
            
            int min = (int)double.Parse(property.Minimum, CultureInfo.InvariantCulture);
            int max = (int)double.Parse(property.Maximum, CultureInfo.InvariantCulture);
            int value = (int)double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            value = Math.Min(Math.Max(value, min), max);
            
            updatingValue = true;
            nud.Minimum = min;
            nud.Maximum = max;
            nud.Value = value;
            nud.Increment = 1;
            tbValue.Minimum = min;
            tbValue.Maximum = max;
            tbValue.Value = value;
            cbAuto.Checked = property.Automatic;
            updatingValue = false;

            lblValue.Text = valueMapper(value);
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            int numericValue = tbValue.Value;
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
            tbValue.Value = numericValue;
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
