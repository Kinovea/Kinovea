using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kinovea.Services
{
    public class CapturePathConfiguration
    {
        public string LeftImageRoot { get; set; }
        public string LeftVideoRoot { get; set; }
        public string RightImageRoot { get; set; }
        public string RightVideoRoot { get; set; }
        
        public string LeftImageSubdir { get; set; }
        public string LeftVideoSubdir { get; set; }
        public string RightImageSubdir { get; set; }
        public string RightVideoSubdir { get; set; }
        
        public string LeftImageFile { get; set; }
        public string LeftVideoFile { get; set; }
        public string RightImageFile { get; set; }
        public string RightVideoFile { get; set; }

        public KinoveaImageFormat ImageFormat { get; set; }
        public KinoveaVideoFormat VideoFormat { get; set; }
        public KinoveaUncompressedVideoFormat UncompressedVideoFormat { get; set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public CapturePathConfiguration()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LeftImageRoot = root;
            RightImageRoot = root;
            LeftVideoRoot = root;
            RightVideoRoot = root;

            string subdir = @"Kinovea\%year\%year%month\%year%month%day";
            LeftImageSubdir = subdir;
            LeftVideoSubdir = subdir;
            RightImageSubdir = subdir;
            RightVideoSubdir = subdir;
            
            string file = @"%year%month%day-%hour%minute%second";
            LeftImageFile = file;
            LeftVideoFile = file;
            RightImageFile = file;
            RightVideoFile = file;

            ImageFormat = KinoveaImageFormat.JPG;
            VideoFormat = KinoveaVideoFormat.MP4;
            UncompressedVideoFormat = KinoveaUncompressedVideoFormat.MKV;
        }

        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "LeftImageRoot":
                        LeftImageRoot = r.ReadElementContentAsString();
                        break;
                    case "LeftImageSubdir":
                        LeftImageSubdir = r.ReadElementContentAsString();
                        break;
                    case "LeftImageFile":
                        LeftImageFile = r.ReadElementContentAsString();
                        break;
                    case "LeftVideoRoot":
                        LeftVideoRoot = r.ReadElementContentAsString();
                        break;
                    case "LeftVideoSubdir":
                        LeftVideoSubdir = r.ReadElementContentAsString();
                        break;
                    case "LeftVideoFile":
                        LeftVideoFile = r.ReadElementContentAsString();
                        break;
                    case "RightImageRoot":
                        RightImageRoot = r.ReadElementContentAsString();
                        break;
                    case "RightImageSubdir":
                        RightImageSubdir = r.ReadElementContentAsString();
                        break;
                    case "RightImageFile":
                        RightImageFile = r.ReadElementContentAsString();
                        break;
                    case "RightVideoRoot":
                        RightVideoRoot = r.ReadElementContentAsString();
                        break;
                    case "RightVideoSubdir":
                        RightVideoSubdir = r.ReadElementContentAsString();
                        break;
                    case "RightVideoFile":
                        RightVideoFile = r.ReadElementContentAsString();
                        break;
                    case "ImageFormat":
                        ImageFormat = (KinoveaImageFormat)Enum.Parse(typeof(KinoveaImageFormat), r.ReadElementContentAsString());
                        break;
                    case "VideoFormat":
                        VideoFormat = (KinoveaVideoFormat)Enum.Parse(typeof(KinoveaVideoFormat), r.ReadElementContentAsString());
                        break;
                    case "UncompressedVideoFormat":
                        UncompressedVideoFormat = (KinoveaUncompressedVideoFormat)Enum.Parse(typeof(KinoveaUncompressedVideoFormat), r.ReadElementContentAsString());
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
            w.WriteElementString("LeftImageRoot", LeftImageRoot);
            w.WriteElementString("LeftImageSubdir", LeftImageSubdir);
            w.WriteElementString("LeftImageFile", LeftImageFile);
            w.WriteElementString("LeftVideoRoot", LeftVideoRoot);
            w.WriteElementString("LeftVideoSubdir", LeftVideoSubdir);
            w.WriteElementString("LeftVideoFile", LeftVideoFile);
            w.WriteElementString("RightImageRoot", RightImageRoot);
            w.WriteElementString("RightImageSubdir", RightImageSubdir);
            w.WriteElementString("RightImageFile", RightImageFile);
            w.WriteElementString("RightVideoRoot", RightVideoRoot);
            w.WriteElementString("RightVideoSubdir", RightVideoSubdir);
            w.WriteElementString("RightVideoFile", RightVideoFile);

            w.WriteElementString("ImageFormat", ImageFormat.ToString());
            w.WriteElementString("VideoFormat", VideoFormat.ToString());
            w.WriteElementString("UncompressedVideoFormat", UncompressedVideoFormat.ToString());
        }

        public CapturePathConfiguration Clone()
        {
            CapturePathConfiguration cloned = new CapturePathConfiguration();

            cloned.LeftImageRoot = this.LeftImageRoot;
            cloned.LeftImageSubdir = this.LeftImageSubdir;
            cloned.LeftImageFile = this.LeftImageFile;
            cloned.RightImageRoot = this.RightImageRoot;
            cloned.RightImageSubdir = this.RightImageSubdir;
            cloned.RightImageFile = this.RightImageFile;
            cloned.LeftVideoRoot = this.LeftVideoRoot;
            cloned.LeftVideoSubdir = this.LeftVideoSubdir;
            cloned.LeftVideoFile = this.LeftVideoFile;
            cloned.RightVideoRoot = this.RightVideoRoot;
            cloned.RightVideoSubdir = this.RightVideoSubdir;
            cloned.RightVideoFile = this.RightVideoFile;
            cloned.ImageFormat = this.ImageFormat;
            cloned.VideoFormat = this.VideoFormat;
            cloned.UncompressedVideoFormat = this.UncompressedVideoFormat;

            return cloned;
        }
    }
}
