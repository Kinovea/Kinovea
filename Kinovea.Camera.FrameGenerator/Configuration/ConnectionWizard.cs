#region License
/*
Copyright © Joan Charmant 2013.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Kinovea.Camera.FrameGenerator
{
    public partial class ConnectionWizard : UserControl, IConnectionWizard
    {
        private CameraManagerFrameGenerator manager;
        private bool testing;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ConnectionWizard(CameraManagerFrameGenerator manager)
        {
            this.manager = manager;
            InitializeComponent();
            BackColor = Color.White;
        }
        
        public CameraSummary GetResult()
        {
            string id = Guid.NewGuid().ToString();
            CameraSummary summary = manager.GetDefaultCameraSummary(id);
            summary.UpdateSpecific(CreateSpecific());
            return summary;
        }
        
        public void TestCamera()
        {
            if(testing)
                return;

            // Spawn a thread to get a snapshot.
            testing = true;
            CameraSummary testSummary = GetResult();
            SnapshotRetriever retriever = new SnapshotRetriever(testSummary);
            retriever.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            ThreadPool.QueueUserWorkItem(retriever.Run);
        }
        
        public SpecificInfo GetSpecific()
        {
            return CreateSpecific();
        }
        
        public void Populate(SpecificInfo specific)
        {
        }
        
        private SpecificInfo CreateSpecific()
        {
            return new SpecificInfo();
        }
        
        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if (retriever == null)
                return;

            FormHandshakeResult result = new FormHandshakeResult(e.Thumbnail);
            result.ShowDialog();
            result.Dispose();
                
            e.Thumbnail.Dispose();
            AfterCameraTest(retriever);
        }
        
        private void AfterCameraTest(SnapshotRetriever retriever)
        {
            retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
            testing = false;
        }
    }
}
