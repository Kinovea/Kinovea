using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the router to specialized exporters for the different image export formats.
    /// The "formats" at this level are the different types of image export we have (single, sequence, etc.),
    /// not the final file format. All exporters should be able to export to all the supported file formats,
    /// which will be chosen in the save file dialog.
    /// </summary>
    public class ImageExporter
    {
        private ImageExportFormat format;
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ImageExporter()
        {

        }

        public void Export(ImageExportFormat format, PlayerScreen player1, PlayerScreen player2)
        {
            if (player1 == null)
                return;

            switch (format)
            {
                case ImageExportFormat.Image:
                    ExporterImage exporterImage = new ExporterImage();
                    exporterImage.Export(player1);
                    break;
            }

        }
    }
}
