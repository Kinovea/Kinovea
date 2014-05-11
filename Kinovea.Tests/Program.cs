using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Tests.Metadata;
using Kinovea.Tests.HistoryStackTester;

namespace Kinovea.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //KVAFuzzer20 fuzzer = new KVAFuzzer20();
            //fuzzer.CreateKVA(@"C:\Users\Joan\Dev  Prog\Videa\Experiments\KVAFuzzer");

            try
            {
                HistoryStackSimpleTester tester = new HistoryStackSimpleTester();
                tester.Test();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
