using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// Parameters for capture automation.
    /// </summary>
    public class CaptureAutomationConfiguration
    {
        public bool EnableAudioTrigger { get; set; }
        public string AudioInputDevice { get; set; }
        public float AudioTriggerThreshold { get; set; }
        public bool EnableUDPTrigger { get; set; }
        public int UDPPort { get; set; }
        public float TriggerQuietPeriod { get; set; }
        public CaptureTriggerAction TriggerAction { get; set; }
        public bool EnableAutoNumbering { get; set; }
        public bool IgnoreOverwrite { get; set; }
        public bool DefaultTriggerArmed { get; set; }

        private static CaptureAutomationConfiguration defaultConfiguration;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CaptureAutomationConfiguration()
        {
            EnableAudioTrigger = false;
            AudioInputDevice = Guid.Empty.ToString();
            AudioTriggerThreshold = 0.9f;
            EnableUDPTrigger = false;
            UDPPort = 8875;
            TriggerQuietPeriod = 0.0f;
            TriggerAction = CaptureTriggerAction.RecordVideo;
            EnableAutoNumbering = true;
            IgnoreOverwrite = false;
            DefaultTriggerArmed = true;
        }

        static CaptureAutomationConfiguration()
        {
            defaultConfiguration = new CaptureAutomationConfiguration();
        }

        public static CaptureAutomationConfiguration Default
        {
            get { return defaultConfiguration; }
        }

        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "EnableAudioTrigger":
                        EnableAudioTrigger = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "AudioInputDevice":
                        AudioInputDevice = r.ReadElementContentAsString();
                        break;
                    case "AudioTriggerThreshold":
                        string strAudioTreshold = r.ReadElementContentAsString();
                        AudioTriggerThreshold = float.Parse(strAudioTreshold, CultureInfo.InvariantCulture);
                        break;
                    case "EnableUDPTrigger":
                        EnableUDPTrigger = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "UDPPort":
                        UDPPort = r.ReadElementContentAsInt();
                        break;
                    case "AudioQuietPeriod":
                        string strAudioQuietPeriod = r.ReadElementContentAsString();
                        TriggerQuietPeriod = float.Parse(strAudioQuietPeriod, CultureInfo.InvariantCulture);
                        break;
                    case "TriggerAction":
                        TriggerAction = (CaptureTriggerAction)Enum.Parse(typeof(CaptureTriggerAction), r.ReadElementContentAsString());
                        break;
                    case "EnableAutoNumbering":
                        EnableAutoNumbering = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "IgnoreOverwriteWarning":
                        IgnoreOverwrite = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "DefaultTriggerArmed":
                        DefaultTriggerArmed = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("EnableAudioTrigger", XmlHelper.WriteBoolean(EnableAudioTrigger));
            w.WriteElementString("AudioInputDevice", AudioInputDevice);
            w.WriteElementString("AudioTriggerThreshold", AudioTriggerThreshold.ToString("0.000", CultureInfo.InvariantCulture));
            w.WriteElementString("EnableUDPTrigger", XmlHelper.WriteBoolean(EnableUDPTrigger));
            w.WriteElementString("UDPPort", UDPPort.ToString());
            w.WriteElementString("AudioQuietPeriod", TriggerQuietPeriod.ToString("0.000", CultureInfo.InvariantCulture));
            w.WriteElementString("TriggerAction", TriggerAction.ToString());
            w.WriteElementString("EnableAutoNumbering", XmlHelper.WriteBoolean(EnableAutoNumbering));
            w.WriteElementString("IgnoreOverwriteWarning", XmlHelper.WriteBoolean(IgnoreOverwrite));
            w.WriteElementString("DefaultTriggerArmed", XmlHelper.WriteBoolean(DefaultTriggerArmed));
        }
    }
}
