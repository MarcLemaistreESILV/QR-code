using Microsoft.VisualStudio.TestTools.UnitTesting;
using nom;
namespace test_projet
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            MyImage testA = new MyImage("coco");
            
            byte[] result = testA.Convertir_Int_To_Endian(12);
            byte[] resultVrai = { 12, 0, 0, 0 };
            for(int i =0; i < 4; i++)
            {
                Assert.AreEqual(result[i], resultVrai[i]);
            }
           

        }
    }
}
