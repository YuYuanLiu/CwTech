using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtMemStreamTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            


               

            using (var ms = new System.IO.MemoryStream())
            {
                for (byte idx = 0; idx < 100; idx++)
                    ms.WriteByte(idx);

                ms.Seek(0, System.IO.SeekOrigin.Current);

                ms.ReadByte();
                ms.ReadByte();
                ms.ReadByte();
                ms.ReadByte();

                Console.WriteLine("---Debug---");


            }








        }


    }
}
