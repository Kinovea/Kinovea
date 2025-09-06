using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kinovea.Services
{
    public class CapturePathConfiguration
    {
        /// <summary>
        /// List of capture folders defined by the user.
        /// </summary>
        public List<CaptureFolder> CaptureFolders { get; set; }
        /// <summary>
        /// Image format to use when capturing images.
        /// </summary>
        public KinoveaImageFormat ImageFormat { get; set; }
        /// <summary>
        /// Video format to use when capturing videos.
        /// </summary>
        public KinoveaVideoFormat VideoFormat { get; set; }
        /// <summary>
        /// Video format to use when capturing uncompressed videos.
        /// </summary>
        public KinoveaUncompressedVideoFormat UncompressedVideoFormat { get; set; }

        #region Obsolete
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
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public CapturePathConfiguration()
        {
            // Default configuration.
            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            root = Path.Combine(root, "Capture");

            CaptureFolder captureA = new CaptureFolder
            {
                Id = Guid.NewGuid(),
                ShortName = "Capture A",
                Path = Path.Combine(root, "Capture A")
            };

            CaptureFolder captureB = new CaptureFolder
            {
                Id = Guid.NewGuid(),
                ShortName = "Capture B",
                Path = Path.Combine(root, "Capture B")
            };

            //LeftImageRoot = root;
            //RightImageRoot = root;
            //LeftVideoRoot = root;
            //RightVideoRoot = root;

            //string subdir = @"Kinovea\%year\%year%month\%year%month%day";
            //LeftImageSubdir = subdir;
            //LeftVideoSubdir = subdir;
            //RightImageSubdir = subdir;
            //RightVideoSubdir = subdir;

            //string file = @"%datetime";
            //LeftImageFile = file;
            //LeftVideoFile = file;
            //RightImageFile = file + "-2";
            //RightVideoFile = file + "-2";

            CaptureFolders = new List<CaptureFolder> { captureA, captureB };

            ImageFormat = KinoveaImageFormat.JPG;
            VideoFormat = KinoveaVideoFormat.MP4;
            UncompressedVideoFormat = KinoveaUncompressedVideoFormat.MKV;
        }

        public CapturePathConfiguration Clone()
        {
            CapturePathConfiguration cloned = new CapturePathConfiguration();

            cloned.CaptureFolders.Clear();
            foreach (CaptureFolder folder in this.CaptureFolders)
                cloned.CaptureFolders.Add(folder.Clone());
            cloned.ImageFormat = this.ImageFormat;
            cloned.VideoFormat = this.VideoFormat;
            cloned.UncompressedVideoFormat = this.UncompressedVideoFormat;

            return cloned;
        }


        #region Serialization
        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "CaptureFolders":
                        ParseCaptureFolders(r);
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

        private void ParseCaptureFolders(XmlReader r)
        {
            CaptureFolders.Clear();
            bool empty = r.IsEmptyElement;

            r.ReadStartElement();

            if (empty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "CaptureFolder")
                {
                    CaptureFolder folder = new CaptureFolder(r);
                    CaptureFolders.Add(folder);
                }
                else
                {
                    r.ReadOuterXml();
                }
            }

            r.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            if (CaptureFolders.Count > 0)
            {
                w.WriteStartElement("CaptureFolders");
                foreach (CaptureFolder folder in CaptureFolders)
                {
                    w.WriteStartElement("CaptureFolder");
                    folder.WriteXML(w);
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }

            w.WriteElementString("ImageFormat", ImageFormat.ToString());
            w.WriteElementString("VideoFormat", VideoFormat.ToString());
            w.WriteElementString("UncompressedVideoFormat", UncompressedVideoFormat.ToString());
        }
        #endregion
    }
}
