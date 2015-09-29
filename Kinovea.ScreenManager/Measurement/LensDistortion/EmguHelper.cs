using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System.Drawing;
using Emgu.CV.Util;

namespace Kinovea.ScreenManager
{
    public static class EmguHelper
    {
        public static Matrix<float> ToMatrix(MCvPoint3D32f[][] data)
        {
            int elementCount = 0;
            foreach (MCvPoint3D32f[] d in data)
                elementCount += d.Length;

            Matrix<float> res = new Matrix<float>(elementCount, 3);

            Int64 address = res.MCvMat.data.ToInt64();

            foreach (MCvPoint3D32f[] d in data)
            {
                int lengthInBytes = d.Length * StructSize.MCvPoint3D32f;
                GCHandle handle = GCHandle.Alloc(d, GCHandleType.Pinned);
                Emgu.Util.Toolbox.memcpy(new IntPtr(address), handle.AddrOfPinnedObject(), lengthInBytes);
                handle.Free();
                address += lengthInBytes;
            }

            return res;
        }

        public static Matrix<float> ToMatrix(PointF[][] data)
        {
            int elementCount = 0;
            foreach (PointF[] d in data)
                elementCount += d.Length;

            Matrix<float> res = new Matrix<float>(elementCount, 2);
            Int64 address = res.MCvMat.data.ToInt64();

            foreach (PointF[] d in data)
            {
                int lengthInBytes = d.Length * StructSize.PointF;
                GCHandle handle = GCHandle.Alloc(d, GCHandleType.Pinned);
                Emgu.Util.Toolbox.memcpy(new IntPtr(address), handle.AddrOfPinnedObject(), lengthInBytes);
                handle.Free();
                address += lengthInBytes;
            }

            return res;
        }

        public static Matrix<float> ToMatrix(PointF[] data)
        {
            Matrix<float> res = new Matrix<float>(data.Length, 1, 2);
            Int64 address = res.MCvMat.data.ToInt64();

            int lengthInBytes = data.Length * 2 * sizeof(float);
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Emgu.Util.Toolbox.memcpy(new IntPtr(address), handle.AddrOfPinnedObject(), lengthInBytes);
            handle.Free();

            return res;
        }

        public static Matrix<float> ToMatrix(PointF data)
        {
            Matrix<float> res = new Matrix<float>(1, 1, 2);
            Int64 address = res.MCvMat.data.ToInt64();

            int lengthInBytes = 2 * sizeof(float);
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Emgu.Util.Toolbox.memcpy(new IntPtr(address), handle.AddrOfPinnedObject(), lengthInBytes);
            handle.Free();

            return res;
        }
    }
}
