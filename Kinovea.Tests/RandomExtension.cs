using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.Tests
{
    public static class RandomExtension
    {
        public static double NextDouble(this Random random, double min, double max)
        {
            return min + (random.NextDouble() * (max - min));
        }

        public static bool NextBoolean(this Random random)
        {
            return random.NextDouble() < 0.5;
        }

        public static Color NextColor(this Random random, int alpha)
        {
            int r = random.Next(255);
            int g = random.Next(255);
            int b = random.Next(255);
            return Color.FromArgb(alpha, r, g, b);
        }

        public static string NextString(this Random random, int length)
        {
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string res = "";
            while (0 < length--)
                res += valid[random.Next(valid.Length)];
            return res;
        }

        public static Point NextPoint(this Random random, int minX, int maxX, int minY, int maxY)
        {
            return new Point(random.Next(minX, maxX), random.Next(minY, maxY));
        }

        public static PointF NextPointF(this Random random, int minX, int maxX, int minY, int maxY)
        {
            return new PointF((float)random.NextDouble(minX, maxX), (float)random.NextDouble(minY, maxY));
        }

        public static Size NextSize(this Random random, int min, int max)
        {
            return new Size(random.Next(min, max), random.Next(min, max));
        }

        public static Size NextSize(this Random random, int minW, int maxW, int minH, int maxH)
        {
            return new Size(random.Next(minW, maxW), random.Next(minH, maxH));
        }

        public static SizeF NextSizeF(this Random random, float minW, float maxW, float minH, float maxH)
        {
            return new SizeF((float)random.NextDouble(minW, maxW), (float)random.NextDouble(minH, maxH));
        }
    }
}
