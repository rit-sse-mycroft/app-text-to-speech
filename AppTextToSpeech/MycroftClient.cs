using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace AppTextToSpeech
{

  class MycroftClient
  {

    /// <summary>
    /// The connection to Mycroft server
    /// </summary>
    private NetworkStream client;
    private MycroftVoice voice;

    /// <summary>
    /// Construct a new client which has a TLS stream with the Mycroft server.
    /// </summary>
    /// <param name="host">Mycroft's hostname</param>
    /// <param name="port"><Mycroft's port</param>
    public MycroftClient(String host, int port)
    {
      InitializeConnection(host, port);
      SendManifest();
      TellMycroft("APP_UP");
    }

    private MycroftClient()
    {
      voice = new MycroftVoice();
    }

    /// <summary>
    /// Initialize the connection to Mycroft
    /// </summary>
    /// <param name="host">Mycroft's hostname</param>
    /// <param name="port">Mycroft's port</param>
    private void InitializeConnection(String host, int port)
    {
      client = new TcpClient(host, port).GetStream();
    }

    public void ListenForCommands()
    {
      System.Diagnostics.Debug.WriteLine("Listening for commands");
      while (true)
      {
        // Get the message length
        byte[] smallBuf = new byte[100];
        int i = 0;
        while (i < smallBuf.Length) // read until we find a newline
        {
          smallBuf[i] = (byte)client.ReadByte();
          i++;
          try
          {
            string soFar = Encoding.UTF8.GetString(smallBuf, 0, i);
            if (soFar.EndsWith("\n"))
            {
              break;
            }
          }
          catch (ArgumentException ex) { } // do nothing, it's just not valid yet
        }
        // make the last a null character
        smallBuf[Math.Min(smallBuf.Length - 1, i)] = (byte)'\0';
        String msgLen = Encoding.UTF8.GetString(smallBuf, 0, Math.Min(smallBuf.Length-1, i+1));
        msgLen = msgLen.Trim();

        // yay we have the message length! let's get the message
        int bufLen = int.Parse(msgLen);
        byte[] buff = new byte[bufLen];
        client.Read(buff, 0, bufLen);
        string sent = Encoding.UTF8.GetString(buff, 0, bufLen);

        // yay we have the message! split it up
        int index = sent.IndexOf(" {");
        string verb = sent.Substring(0, index);
        string json = sent.Substring(index + 1);
        JObject parsedJson = JObject.Parse(json);
        HandleMessage(verb, parsedJson);
      }
    }

    /// <summary>
    /// Handle a message sent from Mycroft
    /// </summary>
    /// <param name="type">the type of the message</param>
    /// <param name="json">the parsed json</param>
    private void HandleMessage(string type, dynamic json)
    {
      System.Diagnostics.Debug.WriteLine("got type " + type);
      if (type == "MSG_QUERY")
      {
        string id = json.id;
        string capability = json.capability;
        string remoteprocedure = json.remoteprocedure;
        JArray args = json.args;
        JArray instanceId = json.instanceId;

        System.Diagnostics.Debug.WriteLine("we want to say: " + args.First);
        String message = (args.First).ToString();
        voice.SayMessage(message);
      }
      if (type == "APP_MANIFEST_OK")
      {
        string instanceId = json.instanceId;
        System.Diagnostics.Debug.WriteLine("instanceId: " + instanceId);
      }
    }

    /// <summary>
    /// Send the Application's manifest file
    /// </summary>
    public void SendManifest()
    {
      //load path and manifest
      var assembly = Assembly.GetExecutingAssembly();
      var manifestStream = assembly.GetManifestResourceStream("AppTextToSpeech.app.json");
      StreamReader reader = new StreamReader(manifestStream);
      string manifest = reader.ReadToEnd();

      TellMycroft("APP_MANIFEST " + manifest);
    }

    /// <summary>
    /// Sends the given string to Mycroft server
    /// </summary>
    /// <param name="message">the full message to send</param>
    public void TellMycroft(String message)
    {
      int numBytes = Encoding.UTF8.GetBytes(message).Length;
      message = numBytes + "\n" + message;
      byte[] bytes = Encoding.UTF8.GetBytes(message);
      client.Write(bytes, 0, bytes.Length);
    }
  }
}