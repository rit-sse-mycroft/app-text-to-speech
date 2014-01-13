using System;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppTextToSpeech;
using Newtonsoft.Json.Linq;

namespace AppTextToSpeech.Tests
{
  [TestClass]
  public class TestMycroftClient
  {
    /// <summary>
    /// Testing:
    /// input  - create new mycroft client
    /// output - 
    /// </summary>
    [TestMethod]
    public void TestCreation()
    {
      byte[] inputBytes = new byte[2048];
      Stream input = new MemoryStream(inputBytes);
      Stream output = new MemoryStream();

      var client = new MycroftClient(input, output);

      output.Seek(0, 0);
      StreamReader reader = new StreamReader(output);
      int numBytes = int.Parse(reader.ReadLine());
      Assert.IsTrue(numBytes > 12);

      string msg = reader.ReadToEnd();
      int index = msg.IndexOf(" {");
      Assert.IsTrue(index > 0);

      string verb = msg.Substring(0, index);
      System.Diagnostics.Debug.WriteLine("verb was: " + verb);
      Assert.AreEqual(verb, "APP_MANIFEST", false);

      string body = msg.Substring(index + 1);
      System.Diagnostics.Debug.WriteLine("Message body was " + body);
      JObject.Parse(body);
    }

  }
}
