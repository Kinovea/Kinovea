﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot.WindowsForms;
using System.Windows.Forms;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class PlotHelper
    {
        private PlotView plot;
        private double memoXMin;
        private double memoXMax;
        private double memoYMin;
        private double memoYMax;

        public PlotHelper(PlotView plot)
        {
            this.plot = plot;
        }

        public void Backup()
        {
            memoXMin = plot.Model.Axes[0].ActualMinimum;
            memoXMax = plot.Model.Axes[0].ActualMaximum;
            memoYMin = plot.Model.Axes[1].ActualMinimum;
            memoYMax = plot.Model.Axes[1].ActualMaximum;
        }

        public void Restore()
        {
            plot.Model.Axes[0].Zoom(memoXMin, memoXMax);
            plot.Model.Axes[1].Zoom(memoYMin, memoYMax);
            plot.InvalidatePlot(false);
        }

        public void ExportGraph(int width, int height)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export graph";
            saveFileDialog.Filter = "PNG (*.png)|*.png";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            Backup();
            PngExporter.Export(plot.Model, saveFileDialog.FileName, width, height, Brushes.White);
            Restore();
        }

        public void CopyGraph(int width, int height)
        {
            Backup();
            Bitmap bmp = PngExporter.ExportToBitmap(plot.Model, width, height, Brushes.White);
            Clipboard.SetImage(bmp);
            bmp.Dispose();
            Restore();
        }
    }
}
