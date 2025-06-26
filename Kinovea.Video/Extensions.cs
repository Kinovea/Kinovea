#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinovea.Video
{
    /// <summary>
    /// 提供位图和颜色处理的扩展方法
    ///	图像处理和分析
    ///	视频帧处理
    ///	模板匹配
    ///	用户界面效果
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 从位图中提取指定矩形区域
        /// </summary>
        /// <param name="image">源位图</param>
        /// <param name="region">要提取的矩形区域</param>
        /// <returns>提取出的新位图</returns>
        /// <remarks>
        /// 此方法使用不安全代码以获得最佳性能。
        /// 直接在内存中操作位图数据，避免逐像素复制带来的性能开销。
        /// </remarks>
        public static Bitmap ExtractTemplate(this Bitmap image, Rectangle region)
        {
            // TODO: test perfs by simply drawing in the new image.

            // 创建目标位图，使用32位ARGB格式
            Bitmap template = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppPArgb);

            // 锁定源图像和目标图像的位图数据以进行直接内存访问
            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                image.PixelFormat
            );
            BitmapData templateData = template.LockBits(
                new Rectangle(0, 0, template.Width, template.Height),
                ImageLockMode.ReadWrite,
                template.PixelFormat
            );

            // 每个像素4字节(ARGB)
            int pixelSize = 4;

            // 计算模板图像的步长和偏移量
            int tplStride = templateData.Stride;                   // 模板图像每行的字节数
            int templateWidthInBytes = region.Width * pixelSize;   // 模板图像实际数据宽度
            int tplOffset = tplStride - templateWidthInBytes;      // 行末对齐补充的字节数

            // 计算源图像的步长和偏移量
            int imgStride = imageData.Stride;                      // 源图像每行的字节数
            int imageWidthInBytes = image.Width * pixelSize;       // 源图像实际数据宽度
            // 计算源图像指针在行末需要移动的字节数
            int imgOffset = imgStride - (image.Width * pixelSize) + imageWidthInBytes - templateWidthInBytes;

            // 确保起始位置不会越界
            int startY = Math.Max(0, region.Top);
            int startX = Math.Max(0, region.Left);

            // 使用不安全代码直接操作内存
            //•	性能优化：
            //•	使用 unsafe 代码直接操作内存
            //•	避免使用 GetPixel/ SetPixel 等高开销操作
            //•	考虑了图像行对齐的问题
            unsafe
            {
                // 获取模板图像和源图像的起始指针
                byte* pTpl = (byte*)templateData.Scan0.ToPointer();
                byte* pImg = (byte*)imageData.Scan0.ToPointer() + (imgStride * startY) + (pixelSize * startX);

                // 逐行复制像素数据
                for (int row = 0; row < region.Height; row++)
                {
                    // 检查是否超出源图像高度
                    if (startY + row > imageData.Height - 1)
                        break;

                    // 逐像素复制数据
                    for (int col = 0; col < templateWidthInBytes; col++, pTpl++, pImg++)
                    {
                        // 检查是否超出源图像宽度
                        if (startX * pixelSize + col < imageWidthInBytes)
                            *pTpl = *pImg;
                    }

                    // 移动到下一行
                    pTpl += tplOffset;
                    pImg += imgOffset;
                }
            }

            // 解锁位图数据
            image.UnlockBits(imageData);
            template.UnlockBits(templateData);

            return template;
        }

        /// <summary>
        /// 反转颜色值
        /// </summary>
        /// <param name="color">要反转的颜色</param>
        /// <returns>反转后的颜色，保持透明度不变</returns>
        /// <remarks>
        /// 对RGB通道进行反转（255-原值），保持Alpha通道不变
        /// </remarks>
        public static Color Invert(this Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }
    }
}
