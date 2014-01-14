using System;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppTextToSpeech;
using Newtonsoft.Json.Linq;
using System.Reflection;

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

    /// <summary>
    /// Tests that a manifest_ok message is handled correctly
    /// input - manifest_ok
    /// output - app_up
    /// </summary>
    [TestMethod]
    public void TestManifestOk()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var manifestStream = assembly.GetManifestResourceStream("AppTextToSpeech.Tests.inputs.manifest_ok.message");
      StreamReader reader = new StreamReader(manifestStream);
      string manifest_ok = reader.ReadToEnd();

      Stream input = new MemoryStream(Encoding.UTF8.GetBytes(manifest_ok));
      Stream output = new MemoryStream();

      var client = new MycroftClient(input, output);

      // reset the output
      output.Seek(0, 0);
      output.SetLength(0);

      output.Seek(0, 0);
      client.ReadCommand();
      output.Seek(0, 0);

      // make sure it sent APP_UP
      byte[] app_up = new byte[20];
      output.Read(app_up, 0, app_up.Length);
      string parsedVerb = Encoding.UTF8.GetString(app_up);
      Assert.AreEqual("6\nAPP_UP", parsedVerb, false);
    }
  }
}
