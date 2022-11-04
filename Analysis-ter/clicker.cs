using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
// nuget => emgu.cv.bitmap
// if you're still getting errors, nuget => emgu.cv (in addition to the above)

namespace autoClickF
{

    class Clicker
    {
        // if minVal from `DetectTarget()` is below this value, the detection is *likely* to be accurate
        // "likely" because these values tend to be inconsistent across computers and especially between different
        //      templates (e.g., SparkVue's THRESHOLD is approx. double that of Kinovea's (from my own testing!))
        private static readonly double THRESHOLD = 3_000_000;
        // mouse events
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x02;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);

        // these structs are intended to be expanded if we want to return additional info
        public struct Target // using TM_SQDIFF
        {
            public Point loc; // minLoc 
            public double minVal; // minVal for client debugging
            public bool detected; // minVal < THRESHOLD
        }

        public struct Data
        {
            public Target[] targets;
            public double delay;
        }

        private static Mat CaptureScreen()
        {
            Screen[] screens = Screen.AllScreens;
            Rectangle bounds = screens[screens.Length - 1].Bounds; // hopefully the main screen's bounds :)

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) // new Bitmap
            {
                using (Graphics g = Graphics.FromImage(bitmap)) // take the screenshot
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }

                return bitmap.ToMat(); // convert Bitmap to Mat and return
            }
        }

        private static Target DetectTarget(Mat screenshot, Mat template) // using TM_SQDIFF
        {
            Mat result = new Mat(); // ref to hold MatchTemplate() result
            CvInvoke.MatchTemplate(screenshot, template, result, TemplateMatchingType.Sqdiff); // run dumb algo to find best match of template in screenshot

            double minVal = 0; // essentially, 'arbitrary' confidence value for detection accuracy
            double maxVal = 0; // used for other TemplateMatchingTypes
            Point minLoc = new Point(); // 'best guess' top-left corner location of target
            Point maxLoc = new Point(); // used for other TemplateMatchingTypes
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            Console.WriteLine(minLoc);

            return new Target() // the "detected target"
            {
                loc = minLoc,
                minVal = minVal,
                detected = minVal < THRESHOLD,
            };
        }

        private static void Click(Point loc)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)loc.X, (uint)loc.Y, 0, 0);
            // can sleep here to delay the full click event, might be useful to have the thread barrier
            //      here to further reduce time between click-event and recording-start ?
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)loc.X, (uint)loc.Y, 0, 0);
        }

        // convenience function
        private static void MoveToAndClick(Point loc)
        {
            SetCursorPos(loc.X, loc.Y);
            Click(loc);
        }

        public static Data ClickTargets(Mat kinoveaMat, Mat sparkvueMat)
        {
            // capture screen and detect targets
            Mat screenshot = CaptureScreen();
            Target kinoveaTarget = DetectTarget(screenshot, kinoveaMat);
            Target sparkvueTarget = DetectTarget(screenshot, sparkvueMat);

            // cache original mouse position
            GetCursorPos(out Point origPos);

            // move to and click targets
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MoveToAndClick(kinoveaTarget.loc);
            MoveToAndClick(sparkvueTarget.loc);
            stopwatch.Stop();

            // return to original mouse position
            SetCursorPos(origPos.X, origPos.Y);

            return new Data()
            {
                targets = new Target[] { kinoveaTarget, sparkvueTarget },
                delay = stopwatch.ElapsedMilliseconds
            };
        }

        public static void SendMsgAndClickTarget(Mat sparkvueMat)
        {
            // TODO: threads ???
            //      thread_1: preps msg (Windows Messaging System: WM_COPYDATA) to Kinovea, w/ barrier just before send
            //      thread_2: preps target detection for SparkVue, moving mouse onto button, w/ barrier just before click


            // thread_2:
            Target sparkvueTarget = DetectTarget(CaptureScreen(), sparkvueMat);
            GetCursorPos(out Point origPos);
            SetCursorPos(sparkvueTarget.loc.X, sparkvueTarget.loc.Y);
            // barrier here ???
            Click(sparkvueTarget.loc);
            SetCursorPos(origPos.X, origPos.Y);

            //return new Data { };
        }

        private static Mat LoadMat(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String); // base64 string to u8 bytes

            Bitmap bitmap;
            using (MemoryStream ms = new MemoryStream(bytes)) // stream across the bytes
            {
                bitmap = (Bitmap)Bitmap.FromStream(ms); // convert bytes to Bitmap
            }

            return bitmap.ToMat(); // convert Bitmap to Mat and return
        }

        static readonly Dictionary<string, Dictionary<bool, Mat>> encodedTemplates = new Dictionary<string, Dictionary<bool, Mat>>()
        {
            {
                "kinovea", new Dictionary<bool, Mat>()
                {
                    { true, LoadMat("iVBORw0KGgoAAAANSUhEUgAAABcAAAAWCAYAAAArdgcFAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEeSURBVEhL1dNZagJREAXQWpufoivQBehmHDYguA4bmwSTn0wQImjP/VxIpa4QKMrKIwjvIx+naPrVvdoOVBQFp/KPy8uy5FSi5V+7Hb8sFnyYzXg/Gl3hGvdw5mU0qmR4PjYbfphMOBsMXDjDjpf9QVVVsYXQfjh0SzXsXF/A6YCb8mOWRd+xhV1kbA9QXdesvS6XbkkMMrYHbsqf5nO3IAYZ2wPUyNDy8dgtiEHG9gA1TcPa3eWmB6iVod37sdgeoLZtWXtbrdyCGGRsD1AnQzvlOT9Op26JB7vI2B6gruvY+txu//wnwq7XAW45IBR7ApzFioF6Gb85y+O+r9f8LF8YfhGAa9zDmZfRqO97ToWCjFQohMCp0EVGGoG/AZ5wTaoVKQ/rAAAAAElFTkSuQmCC") },
                    { false, LoadMat("iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAIAAACQKrqGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHESURBVChTPZLPTuMwEMZtx04UNVWzpOqKUiH+VUIrQa8ceQ8ehufgMThw44y0cKYStFW7QlGrQKu0adNuHNv7JVkRWSNP/Jv5Zuyhg8GASKnjmG42thCCMcsYQkiutcRSynge830iBB32+2o+dwhxOa93OqLRsFy3QDebbLFYDod/GZOOYwUB08ulQ2mN86DXczsdynm+XsvViglROz5uXV3Zacq3W2AMuq5l/ej1kCybz8HpLDN5Dno3nfJ6/ef1tRXHJI4Z6oNuxRmtSVlotYHdhiHoRrdLkoRzQlCfSlOjFKDfNzcV6p2e/rq9RUCeJG67rZ6eGDcGKdVuVxDoV+siZfkVrlKoRPg+WmeVYiFXciVTuIWp/uAEbWUZy8s4ZtsVB91inZ39DzOGe95uNiOuSyePj8HhIc7QQZXjGyqM1rWjo+nDQxxFDK+yeH3FbeNSvxWrALhQWI9G0fMzb7UY9f3cccL7e9wiEth7eygGD4ENniB5exvd3Tnn57zdpuP3d/X1ZaJIjsfBxUXt5MRpNpEv/fhY9fufLy/u5aXd7VrNJv0zmZgsw7go0LOZDMM8irSUOBP7+zam4uAA40Jt+x9R+S+98dIxzAAAAABJRU5ErkJggg==") }
                }
            },
            {
                "sparkvue", new Dictionary<bool, Mat>()
                {
                    { true, LoadMat("iVBORw0KGgoAAAANSUhEUgAAAEwAAAAlCAYAAADobA+5AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAP5SURBVGhD7ZlNT1NBFIbv36BUJXUDCTFYY0wAsQkbmjRu2ahsEERbqlAEAlRQoAViMNSaiHyoC03AKImLxujKRKKJ+pfGeYc717nTU3qh7YVJunh6pzNzznvO2+kHF6th5A2r4x2r4QEfeKRxZJM1T2ZZeHaMdSzcZt3Lvazn6XUWW42KK55jHuvYh/1UHpPxZNi5sTxrS0+wa9lb3Jwez2A/4hBP5TWRsoa1zqRZJHuTNMQriEceKr9pWAH+QNGUesEuzyV5w9EiA45HVORDXkrPFKzAfT7QCI2tsfb5QaLpykFe5Kd0TYAb9poP/tOUytfMLAnyQ0fXNgErkOQDhYO3Id1oNYGOrm0CLsNap9K8mWp9ZpUjKvRUfRNwDGsazVf8bXhUoAddtaDTjhUY5gNO28wE2VStga6swQS4YdssmHzFIhl/T5cEutBHHSYgDGsez5DN+AX09cLAyqd99vbrX4etL39YX75A7vULYVg4nSIb8Qvo64XBrPXCbxZb3itak6Tff6+6iYnNb+LFwZVatwKJbdYx30824hfQRx2S2NKeMAuGqPM6LsOI9eOQ2LAN41dqXRjWvdRLNuIX0FeLggEwYuXjvmteBWvk25WvScPlWu7zL3Z1bteJlevIIQ3CnsyHH06MRDfOCsS37Vs0dDN+AH3UoSINcZrV1kH6nXLC7DmYgRhc8Vyaj73qHhgGYJKcB64TpsxLhGH+/VgtRZQsTj1FasMSyjAdmA0DxWm156Rh1ItR1rBG/hA94RMGfdRBIZuTp61rdtdZcwx7XnDFqGA/4tRYmRMm6vtVw/Q1IAw7DZ9hemE6MAeNqE2WMgzPMY/9kuoZdm+LdT452W9J6KOOw+ia3VFOyo6Ycxlm75MNCzNKxMWyimF2nMRlmLYGhGHhmVGyEb+Avl6YjlfDYALMgCml4io2rCW1SDbiF9BXi+rLFdizvZ+s69FBg4BqhJpzTOQ51D3CMDvfYYYhDvHIo68BYdiZ+DqLLJ7Q35JcF/p6YbJwNAtUE1TQtLoOU2COjMO6NE2evMMMAzBLxqsvBrAa7/IB5+LUONlQrYGurMEErCB/AKFkjkUyN8imagX0oCtrMAErOMQHNhcmpnkj/t1xhZ6qbwIuw8CVdIJorvpAR9c2gSLDQokca58bIJusFsgPHV3bBKzgHT7QOD+8WjPTkBf5KV0T4IZt8kExoXjOfntW7z/fyIe8lJ4plDRMcmF8mv9WquzbE/HIQ+U3jbKGgVB8jYUnHx75xy32Iw7xVF4TsYKDfOCRs0MvWcvIArs0Nco6H/eLuwwHt4ai4ornmMc69mE/lcdkuGEbfFDHK1ZwgA/qeKZu2JHYYP8AO4gg4ZYduUsAAAAASUVORK5CYII=") },
                    { false, LoadMat("iVBORw0KGgoAAAANSUhEUgAAAEgAAAAgCAIAAAA+KKknAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAARHSURBVFhH3Vd9bBRFFN/ZlmuhvaOf0IS7Hldp0RD6HT0qBCxtTMSIPUwFNTGEfzTRGE+TiqQpgcSkJhwGDIEA0X+IQKClYDSmVDSxVfpFC5Kq5evahkh7LUpbe3cte/5mZ27duxJQurs1/PLLZd6bN7Pz5r03M0dsNScEQSAhKSk4YQkEiBCCOHNIRBycm+CPNXHZcBDb9joiSc6hvi09rYlTAa7WAjfnWnYUl/8RN4/LxgKO1VuCE7tazuQPD3CddvDkrfliSSEXjIUI30xSaP6kXxSI5jRPBjH/rBCOoapCIYGQUEhzalSwDwMaMRIicJLoAfqJiI00jCLbVOqXWq0R6Q/mnw3SVHwkITsGF3WCfjM/CCJyRatLeTrkhKTzG0/UGG2oC0Nbqr7FWbI4/bz7We/29YyXt65z5dqibGZOo2usxJG220Wv7E2ft9hrToPLPvqqrpu+DVx51ssfPnf2rWdkw5lCPhUZ9YAyeZjOxWkpCabvegdbrvmiuijvNeThGCtPZhwWmuNNMaI1OfoB6S59/M1VS9CVvcDs3fFC7+Bo2afnoKcR3lCYYYlnZkc7+qoaulibDWn1jgzc/mtjUSY0Y4Gp6i8vsvgjYvLDQy/Ik6vYcLH/9zv+lVlpNOVUek9TT1V9F1YGl+zVDWV7v4XSlWs9+MqTmGXTZ81QHu3wwoEjr6/go2hohGUZliJbMnphg+E7n8/FKPQaXWPIwHdPdsA3Gpmd62tfzOcd98KGApspVjzW6aV5KwhVp7rgdr412ZVnYwaAbyxAd0GeGZawf8qRClEk3HN9gDfoNP541ef8+Js9534N3pU2FtnPvl3K9AoUS+TtyHjw/DWfounsH0mMi3U6UpkIDI76ld4bvvHglGRLmof2rL08PE2/5NScQQQQuiObS7hWhZKsNHP8HC78d8g1pmPIMPc/hTSd+77/DYWxwBzPNaohLVeHRv2TihgmM5CVCsK9jtQEpCLTzFrE/g2QZrgbnFnpXBaEwswUbMRP12nJTQdSF78dfSP4Dd9jOoFNrmJtRUHjO2sV8aXCTOzx1z/fRPvWnxNYNNIPRzzrPdHZh5qpLLIzDcYib7sHbtdf6GcGwNOPpUOPdkW+bd3yRd7hcU9jD0Ri31qf6h/b13oq586QvBYtsT97xYGlTi6EUesqeLnYztrwpPp0N12oDHfZE2+szsZthtor/6QJGlSap7JYuceOtXur6i6wNjNuuzGMTIbD0OCwdR9vZ0cosX8gO9amm2M50Y5pBXe57Nj14VcPN3OVCux1D8hx1RoRz1Ktqaw5Ss9o9FtRYzJEKWX+r0/F+2N3Y8/SbQ2vHfqBy5HQ+VQEVLtoJI0+7g0jUhH/n6UY9UWuHURBiviagaQRG42J602kL2LNcWl+RuTnjCNxvH8SK0icDJTeurLQP8YWpAkuJWW0p1inxBguGwvieI869qhBEP4GgyulNRPb7m0AAAAASUVORK5CYII=") }
                }
            }
        };

        public void Start()
        {
            bool isStart = true;
            Data data = ClickTargets(encodedTemplates["kinovea"][isStart], encodedTemplates["sparkvue"][isStart]);
        }
    }
    /*internal static class Program
    {
        // if minVal from `DetectTarget()` is below this value, the detection is *likely* to be accurate
        // "likely" because these values tend to be inconsistent across computers and especially between different
        //      templates (e.g., SparkVue's THRESHOLD is approx. double that of Kinovea's (from my own testing!))
        private static readonly double THRESHOLD = 3_000_000;
        // mouse events
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x02;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);

        // these structs are intended to be expanded if we want to return additional info
        public struct Target // using TM_SQDIFF
        {
            public Point loc; // minLoc 
            public double minVal; // minVal for client debugging
            public bool detected; // minVal < THRESHOLD
        }

        public struct Data
        {
            public Target[] targets;
            public double delay;
        }

        private static Mat CaptureScreen()
        {
            Screen[] screens = Screen.AllScreens;
            Rectangle bounds = screens[screens.Length - 1].Bounds; // hopefully the main screen's bounds :)

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) // new Bitmap
            {
                using (Graphics g = Graphics.FromImage(bitmap)) // take the screenshot
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }

                return bitmap.ToMat(); // convert Bitmap to Mat and return
            }
        }

        private static Target DetectTarget(Mat screenshot, Mat template) // using TM_SQDIFF
        {
            Mat result = new Mat(); // ref to hold MatchTemplate() result
            CvInvoke.MatchTemplate(screenshot, template, result, TemplateMatchingType.Sqdiff); // run dumb algo to find best match of template in screenshot

            double minVal = 0; // essentially, 'arbitrary' confidence value for detection accuracy
            double maxVal = 0; // used for other TemplateMatchingTypes
            Point minLoc = new Point(); // 'best guess' top-left corner location of target
            Point maxLoc = new Point(); // used for other TemplateMatchingTypes
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            Console.WriteLine(minLoc);

            return new Target() // the "detected target"
            {
                loc = minLoc,
                minVal = minVal,
                detected = minVal < THRESHOLD,
            };
        }

        private static void Click(Point loc)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)loc.X, (uint)loc.Y, 0, 0);
            // can sleep here to delay the full click event, might be useful to have the thread barrier
            //      here to further reduce time between click-event and recording-start ?
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)loc.X, (uint)loc.Y, 0, 0);
        }

        // convenience function
        private static void MoveToAndClick(Point loc)
        {
            SetCursorPos(loc.X, loc.Y);
            Click(loc);
        }

        public static Data ClickTargets(Mat kinoveaMat, Mat sparkvueMat)
        {
            // capture screen and detect targets
            Mat screenshot = CaptureScreen();
            Target kinoveaTarget = DetectTarget(screenshot, kinoveaMat);
            Target sparkvueTarget = DetectTarget(screenshot, sparkvueMat);

            // cache original mouse position
            GetCursorPos(out Point origPos);

            // move to and click targets
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MoveToAndClick(kinoveaTarget.loc);
            MoveToAndClick(sparkvueTarget.loc);
            stopwatch.Stop();

            // return to original mouse position
            SetCursorPos(origPos.X, origPos.Y);

            return new Data() {
                targets = new Target[] { kinoveaTarget, sparkvueTarget },
                delay = stopwatch.ElapsedMilliseconds
            };
        }

        public static void SendMsgAndClickTarget(Mat sparkvueMat)
        {
            // TODO: threads ???
            //      thread_1: preps msg (Windows Messaging System: WM_COPYDATA) to Kinovea, w/ barrier just before send
            //      thread_2: preps target detection for SparkVue, moving mouse onto button, w/ barrier just before click


            // thread_2:
            Target sparkvueTarget = DetectTarget(CaptureScreen(), sparkvueMat);
            GetCursorPos(out Point origPos);
            SetCursorPos(sparkvueTarget.loc.X, sparkvueTarget.loc.Y);
            // barrier here ???
            Click(sparkvueTarget.loc);
            SetCursorPos(origPos.X, origPos.Y);

            //return new Data { };
        }

        private static Mat LoadMat(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String); // base64 string to u8 bytes

            Bitmap bitmap;
            using (MemoryStream ms = new MemoryStream(bytes)) // stream across the bytes
            {
                bitmap = (Bitmap)Bitmap.FromStream(ms); // convert bytes to Bitmap
            }

            return bitmap.ToMat(); // convert Bitmap to Mat and return
        }

        static readonly Dictionary<string, Dictionary<bool, Mat>> encodedTemplates = new Dictionary<string, Dictionary<bool, Mat>>()
        {
            {
                "kinovea", new Dictionary<bool, Mat>()
                {
                    { true, LoadMat("iVBORw0KGgoAAAANSUhEUgAAABcAAAAWCAYAAAArdgcFAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEeSURBVEhL1dNZagJREAXQWpufoivQBehmHDYguA4bmwSTn0wQImjP/VxIpa4QKMrKIwjvIx+naPrVvdoOVBQFp/KPy8uy5FSi5V+7Hb8sFnyYzXg/Gl3hGvdw5mU0qmR4PjYbfphMOBsMXDjDjpf9QVVVsYXQfjh0SzXsXF/A6YCb8mOWRd+xhV1kbA9QXdesvS6XbkkMMrYHbsqf5nO3IAYZ2wPUyNDy8dgtiEHG9gA1TcPa3eWmB6iVod37sdgeoLZtWXtbrdyCGGRsD1AnQzvlOT9Op26JB7vI2B6gruvY+txu//wnwq7XAW45IBR7ApzFioF6Gb85y+O+r9f8LF8YfhGAa9zDmZfRqO97ToWCjFQohMCp0EVGGoG/AZ5wTaoVKQ/rAAAAAElFTkSuQmCC") },
                    { false, LoadMat("iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAIAAACQKrqGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHESURBVChTPZLPTuMwEMZtx04UNVWzpOqKUiH+VUIrQa8ceQ8ehufgMThw44y0cKYStFW7QlGrQKu0adNuHNv7JVkRWSNP/Jv5Zuyhg8GASKnjmG42thCCMcsYQkiutcRSynge830iBB32+2o+dwhxOa93OqLRsFy3QDebbLFYDod/GZOOYwUB08ulQ2mN86DXczsdynm+XsvViglROz5uXV3Zacq3W2AMuq5l/ej1kCybz8HpLDN5Dno3nfJ6/ef1tRXHJI4Z6oNuxRmtSVlotYHdhiHoRrdLkoRzQlCfSlOjFKDfNzcV6p2e/rq9RUCeJG67rZ6eGDcGKdVuVxDoV+siZfkVrlKoRPg+WmeVYiFXciVTuIWp/uAEbWUZy8s4ZtsVB91inZ39DzOGe95uNiOuSyePj8HhIc7QQZXjGyqM1rWjo+nDQxxFDK+yeH3FbeNSvxWrALhQWI9G0fMzb7UY9f3cccL7e9wiEth7eygGD4ENniB5exvd3Tnn57zdpuP3d/X1ZaJIjsfBxUXt5MRpNpEv/fhY9fufLy/u5aXd7VrNJv0zmZgsw7go0LOZDMM8irSUOBP7+zam4uAA40Jt+x9R+S+98dIxzAAAAABJRU5ErkJggg==") }
                }
            },
            {
                "sparkvue", new Dictionary<bool, Mat>()
                {
                    { true, LoadMat("iVBORw0KGgoAAAANSUhEUgAAAEwAAAAlCAYAAADobA+5AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAP5SURBVGhD7ZlNT1NBFIbv36BUJXUDCTFYY0wAsQkbmjRu2ahsEERbqlAEAlRQoAViMNSaiHyoC03AKImLxujKRKKJ+pfGeYc717nTU3qh7YVJunh6pzNzznvO2+kHF6th5A2r4x2r4QEfeKRxZJM1T2ZZeHaMdSzcZt3Lvazn6XUWW42KK55jHuvYh/1UHpPxZNi5sTxrS0+wa9lb3Jwez2A/4hBP5TWRsoa1zqRZJHuTNMQriEceKr9pWAH+QNGUesEuzyV5w9EiA45HVORDXkrPFKzAfT7QCI2tsfb5QaLpykFe5Kd0TYAb9poP/tOUytfMLAnyQ0fXNgErkOQDhYO3Id1oNYGOrm0CLsNap9K8mWp9ZpUjKvRUfRNwDGsazVf8bXhUoAddtaDTjhUY5gNO28wE2VStga6swQS4YdssmHzFIhl/T5cEutBHHSYgDGsez5DN+AX09cLAyqd99vbrX4etL39YX75A7vULYVg4nSIb8Qvo64XBrPXCbxZb3itak6Tff6+6iYnNb+LFwZVatwKJbdYx30824hfQRx2S2NKeMAuGqPM6LsOI9eOQ2LAN41dqXRjWvdRLNuIX0FeLggEwYuXjvmteBWvk25WvScPlWu7zL3Z1bteJlevIIQ3CnsyHH06MRDfOCsS37Vs0dDN+AH3UoSINcZrV1kH6nXLC7DmYgRhc8Vyaj73qHhgGYJKcB64TpsxLhGH+/VgtRZQsTj1FasMSyjAdmA0DxWm156Rh1ItR1rBG/hA94RMGfdRBIZuTp61rdtdZcwx7XnDFqGA/4tRYmRMm6vtVw/Q1IAw7DZ9hemE6MAeNqE2WMgzPMY/9kuoZdm+LdT452W9J6KOOw+ia3VFOyo6Ycxlm75MNCzNKxMWyimF2nMRlmLYGhGHhmVGyEb+Avl6YjlfDYALMgCml4io2rCW1SDbiF9BXi+rLFdizvZ+s69FBg4BqhJpzTOQ51D3CMDvfYYYhDvHIo68BYdiZ+DqLLJ7Q35JcF/p6YbJwNAtUE1TQtLoOU2COjMO6NE2evMMMAzBLxqsvBrAa7/IB5+LUONlQrYGurMEErCB/AKFkjkUyN8imagX0oCtrMAErOMQHNhcmpnkj/t1xhZ6qbwIuw8CVdIJorvpAR9c2gSLDQokca58bIJusFsgPHV3bBKzgHT7QOD+8WjPTkBf5KV0T4IZt8kExoXjOfntW7z/fyIe8lJ4plDRMcmF8mv9WquzbE/HIQ+U3jbKGgVB8jYUnHx75xy32Iw7xVF4TsYKDfOCRs0MvWcvIArs0Nco6H/eLuwwHt4ai4ornmMc69mE/lcdkuGEbfFDHK1ZwgA/qeKZu2JHYYP8AO4gg4ZYduUsAAAAASUVORK5CYII=") },
                    { false, LoadMat("iVBORw0KGgoAAAANSUhEUgAAAEgAAAAgCAIAAAA+KKknAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAARHSURBVFhH3Vd9bBRFFN/ZlmuhvaOf0IS7Hldp0RD6HT0qBCxtTMSIPUwFNTGEfzTRGE+TiqQpgcSkJhwGDIEA0X+IQKClYDSmVDSxVfpFC5Kq5evahkh7LUpbe3cte/5mZ27duxJQurs1/PLLZd6bN7Pz5r03M0dsNScEQSAhKSk4YQkEiBCCOHNIRBycm+CPNXHZcBDb9joiSc6hvi09rYlTAa7WAjfnWnYUl/8RN4/LxgKO1VuCE7tazuQPD3CddvDkrfliSSEXjIUI30xSaP6kXxSI5jRPBjH/rBCOoapCIYGQUEhzalSwDwMaMRIicJLoAfqJiI00jCLbVOqXWq0R6Q/mnw3SVHwkITsGF3WCfjM/CCJyRatLeTrkhKTzG0/UGG2oC0Nbqr7FWbI4/bz7We/29YyXt65z5dqibGZOo2usxJG220Wv7E2ft9hrToPLPvqqrpu+DVx51ssfPnf2rWdkw5lCPhUZ9YAyeZjOxWkpCabvegdbrvmiuijvNeThGCtPZhwWmuNNMaI1OfoB6S59/M1VS9CVvcDs3fFC7+Bo2afnoKcR3lCYYYlnZkc7+qoaulibDWn1jgzc/mtjUSY0Y4Gp6i8vsvgjYvLDQy/Ik6vYcLH/9zv+lVlpNOVUek9TT1V9F1YGl+zVDWV7v4XSlWs9+MqTmGXTZ81QHu3wwoEjr6/go2hohGUZliJbMnphg+E7n8/FKPQaXWPIwHdPdsA3Gpmd62tfzOcd98KGApspVjzW6aV5KwhVp7rgdr412ZVnYwaAbyxAd0GeGZawf8qRClEk3HN9gDfoNP541ef8+Js9534N3pU2FtnPvl3K9AoUS+TtyHjw/DWfounsH0mMi3U6UpkIDI76ld4bvvHglGRLmof2rL08PE2/5NScQQQQuiObS7hWhZKsNHP8HC78d8g1pmPIMPc/hTSd+77/DYWxwBzPNaohLVeHRv2TihgmM5CVCsK9jtQEpCLTzFrE/g2QZrgbnFnpXBaEwswUbMRP12nJTQdSF78dfSP4Dd9jOoFNrmJtRUHjO2sV8aXCTOzx1z/fRPvWnxNYNNIPRzzrPdHZh5qpLLIzDcYib7sHbtdf6GcGwNOPpUOPdkW+bd3yRd7hcU9jD0Ri31qf6h/b13oq586QvBYtsT97xYGlTi6EUesqeLnYztrwpPp0N12oDHfZE2+szsZthtor/6QJGlSap7JYuceOtXur6i6wNjNuuzGMTIbD0OCwdR9vZ0cosX8gO9amm2M50Y5pBXe57Nj14VcPN3OVCux1D8hx1RoRz1Ktqaw5Ss9o9FtRYzJEKWX+r0/F+2N3Y8/SbQ2vHfqBy5HQ+VQEVLtoJI0+7g0jUhH/n6UY9UWuHURBiviagaQRG42J602kL2LNcWl+RuTnjCNxvH8SK0icDJTeurLQP8YWpAkuJWW0p1inxBguGwvieI869qhBEP4GgyulNRPb7m0AAAAASUVORK5CYII=") }
                }
            }
        };

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isStart = true;
            Data data = ClickTargets(encodedTemplates["kinovea"][isStart], encodedTemplates["sparkvue"][isStart]);


            // boilerplate (not mine)
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }*/
}
