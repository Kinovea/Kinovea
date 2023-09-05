using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Tests.Metadata;
using Kinovea.Tests.HistoryStackTester;
using System.Threading;

namespace Kinovea.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TestKVAFuzzer();
            //TestKSVFuzzer();
            //TestHistoryStack();
            //TestLineClipping();

            TestTime();

            // Performance
            //ImageCopy.Test();
        }
        private static void TestKVAFuzzer()
        {
            KVAFuzzer20 fuzzer = new KVAFuzzer20();
            fuzzer.CreateKVA(@"");
        }

        private static void TestKSVFuzzer()
        {
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                KSVFuzzer fuzzer = new KSVFuzzer();
                fuzzer.CreateKSV(@"");
                Thread.Sleep(1000);
            }
        }

        private static void TestHistoryStack()
        {
            try
            {
                HistoryStackSimpleTester tester = new HistoryStackSimpleTester();
                tester.Test();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void TestLineClipping()
        {
            LineClippingTester tester = new LineClippingTester();
            tester.Test();
        }

        private static void TestTime()
        {
            //TimeTester tester = new TimeTester();
            //tester.TestSliderSpeed();

            TimecodeFormatTest tester = new TimecodeFormatTest();
            tester.Test();
        }
    }
}
