using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
    
    private Stream input;
    private Stream output;
    private MycroftVoice voice;
    private string defaultSpeakerInstanceId = "speaker0";
    private List<MycroftSpeaker> speakers;
    private ConcurrentDictionary<string, MsgQuery> processed;

    /// <summary>
    /// Construct a new client with the given input and output streams.
    /// Primarily used for testing.
    /// </summary>
    /// <param name="input">what to read for input</param>
    /// <param name="output">where to write output</param>
    /// <param name="pdct">Dictionary to use for processed queries</param>
    public MycroftClient(Stream input, Stream output, ConcurrentDictionary<string, MsgQuery> pdct)
    {
      this.input = input;
      this.output = output;
      Init(pdct);
      
    }

    /// <summary>
    /// Construct a new client which has a TLS stream with the Mycroft server.
    /// </summary>
    /// <param name="host">Mycroft's hostname</param>
    /// <param name="port"><Mycroft's port</param>
    /// <param name="pdct">Dictionary to use for processed queries</param>
    public MycroftClient(String host, int port, ConcurrentDictionary<string, MsgQuery> pdct)
    {
      InitializeConnection(host, port);
      Init(pdct);
    }

    private void Init(ConcurrentDictionary<string, MsgQuery> pdct)
    {
      processed = pdct;
      processed.Clear();
      voice = new MycroftVoice();
      speakers = new List<MycroftSpeaker>();
      SendManifest();
    }

    /// <summary>
    /// Initialize the connection to Mycroft
    /// </summary>
    /// <param name="host">Mycroft's hostname</param>
    /// <param name="port">Mycroft's port</param>
    private void InitializeConnection(String host, int port)
    {
      input = new TcpClient(host, port).GetStream();
      output = input;
    }

    public void ListenForCommands()
    {
      System.Diagnostics.Debug.WriteLine("Listening for commands");
      while (true)
      {
        // Get the message length
        ReadCommand();
      }
    }

    /// <summary>
    ///  Read and handle one single command
    /// </summary>
    public void ReadCommand()
    {
      byte[] smallBuf = new byte[100];
      int i = 0;
      while (i < smallBuf.Length) // read until we find a newline
      {
        smallBuf[i] = (byte)input.ReadByte();
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
      String msgLen = Encoding.UTF8.GetString(smallBuf, 0, Math.Min(smallBuf.Length - 1, i + 1));
      msgLen = msgLen.Trim();

      // yay we have the message length! let's get the message
      int bufLen = int.Parse(msgLen);
      byte[] buff = new byte[bufLen];
      input.Read(buff, 0, bufLen);
      string sent = Encoding.UTF8.GetString(buff, 0, bufLen);
      System.Diagnostics.Debug.WriteLine("Got message: " + sent);

      // yay we have the message! split it up
      int index = sent.IndexOf(" {");
      string verb = sent.Substring(0, index);
      string json = sent.Substring(index + 1);
      JObject parsedJson = JObject.Parse(json);
      HandleMessage(verb, parsedJson);
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
        HandleMsgQuery(json);
      }
      else if (type == "APP_MANIFEST_OK")
      {
        string instanceId = json.instanceId;
        System.Diagnostics.Debug.WriteLine("instanceId: " + instanceId);
        TellMycroft("APP_UP");
      }
      else if (type == "MSG_QUERY_SUCCESS")
      {
        if(json.instanceId == speakerQueryId)
        {
          MakeSpeaker(json);
        }
      }
    }

    private void MakeSpeaker(dynamic json)
    {
      // construct a speaker object
      
    }

    private void HandleMsgQuery(dynamic json)
    {
      // construct a query object
      var msg = new MsgQuery(json);
      string id = msg.OriginalUUID;
      System.Diagnostics.Debug.WriteLine("we want to say: " + msg.Text);
      if (msg.Action == "say")
        voice.SayMessage(msg.Text);
      else if (msg.Action == "stream")
      {
        MemoryStream stream = new MemoryStream();
        msg.Output = stream;
        msg.NewUUID = System.Guid.NewGuid().ToString();

        JObject msgForSpeakers = new JObject();
        JObject data = new JObject();
        msgForSpeakers.Add("id", msg.NewUUID);
        data.Add("ip", GetSystemIP().ToString());
        data.Add("port", 32761);
        data.Add("streamType", "wav");
        msgForSpeakers.Add("data", data);
        msgForSpeakers.Add("capability", "speakers");
        JArray instanceId = new JArray();
        instanceId.Add(msg.TargetSpeakers != null ? msg.TargetSpeakers : defaultSpeakerInstanceId);
        msgForSpeakers.Add("instanceId", instanceId);
        msgForSpeakers.Add("priority", msg.Priority);
        msgForSpeakers.Add("action", "doStream");

        voice.SaveMessage(msg.Text, msg.Output);
        processed[msg.NewUUID] = msg;
        TellMycroft("MSG_QUERY " + msgForSpeakers.ToString());
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
      output.Write(bytes, 0, bytes.Length);
    }

    private static IPAddress GetSystemIP()
    {
      // totally stolen from http://stackoverflow.com/questions/1069103/how-to-get-my-own-ip-address-in-c
      IPAddress ret = null;
      IPHostEntry host;
      host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          ret = ip;
        }
      }
      return ret;
    }
  }
}