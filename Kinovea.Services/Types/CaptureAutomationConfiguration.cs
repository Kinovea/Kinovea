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
        public float AudioQuietPeriod { get; set; }
        public AudioTriggerAction TriggerAction { get; set; }
        public float RecordingSeconds { get; set; }
        public bool IgnoreOverwrite { get; set; }

        private static CaptureAutomationConfiguration defaultConfiguration;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CaptureAutomationConfiguration()
        {
            EnableAudioTrigger = false;
            AudioInputDevice = Guid.Empty.ToString();
            AudioTriggerThreshold = 0.9f;
            AudioQuietPeriod = 0.0f;
            TriggerAction = AudioTriggerAction.RecordVideo;
            RecordingSeconds = 0;
            IgnoreOverwrite = false;
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
                    case "AudioQuietPeriod":
                        string strAudioQuietPeriod = r.ReadElementContentAsString();
                        AudioQuietPeriod = float.Parse(strAudioQuietPeriod, CultureInfo.InvariantCulture);
                        break;
                    case "TriggerAction":
                        TriggerAction = (AudioTriggerAction)Enum.Parse(typeof(AudioTriggerAction), r.ReadElementContentAsString());
                        break;
                    case "RecordingSeconds":
                        string strRecordingSeconds = r.ReadElementContentAsString();
                        RecordingSeconds = float.Parse(strRecordingSeconds, CultureInfo.InvariantCulture);
                        break;
                    case "IgnoreOverwriteWarning":
                        IgnoreOverwrite = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
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
            w.WriteElementString("EnableAudioTrigger", EnableAudioTrigger ? "true" : "false");
            w.WriteElementString("AudioInputDevice", AudioInputDevice);
            w.WriteElementString("AudioTriggerThreshold", AudioTriggerThreshold.ToString("0.000", CultureInfo.InvariantCulture));
            w.WriteElementString("AudioQuietPeriod", AudioQuietPeriod.ToString("0.000", CultureInfo.InvariantCulture));
            w.WriteElementString("TriggerAction", TriggerAction.ToString());
            w.WriteElementString("RecordingSeconds", RecordingSeconds.ToString("0.000", CultureInfo.InvariantCulture));
            w.WriteElementString("IgnoreOverwriteWarning", IgnoreOverwrite ? "true" : "false");
        }
    }
}
