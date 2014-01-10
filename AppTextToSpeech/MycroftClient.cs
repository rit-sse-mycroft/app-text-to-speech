using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace AppTextToSpeech
{

  class MycroftClient
  {

    /// <summary>
    /// The connection to Mycroft server
    /// </summary>
    private NetworkStream client;

    /// <summary>
    /// Construct a new client which has a TLS stream with the Mycroft server.
    /// </summary>
    /// <param name="host">Mycroft's hostname</param>
    /// <param name="port"><Mycroft's port</param>
    public MycroftClient(String host, int port)
    {
      InitializeConnection(host, port);
    }

    /// <summary>
    /// Initialize the connection to Mycroft
    /// </summary>
    /// <param name="host">Mycroft's hostname</param>
    /// <param name="port">Mycroft's port</param>
    private void InitializeConnection(String host, int port)
    {
      client = new TcpClient(host, port).GetStream();
      SendManifest();
    }

    public void ListenForCommands()
    {
      System.Diagnostics.Debug.WriteLine("Listening for commands");
      while (true)
      {
        StreamReader reader = new StreamReader(client);
        String msgLen = reader.ReadLine();
        msgLen = msgLen.Trim();
        System.Diagnostics.Debug.WriteLine("Message length " + msgLen);
        int bufLen = int.Parse(msgLen)-20;
        byte[] buff = new byte[bufLen];
        client.Read(buff, 0, bufLen);
        string sent = Encoding.UTF8.GetString(buff, 0, bufLen);
        System.Diagnostics.Debug.WriteLine(sent);
      }
    }

    /// <summary>
    /// Send the Application's manifest file
    /// </summary>
    private void SendManifest()
    {
      String manifest = "";
      manifest = "";
      manifest += "{";
      manifest += "\"version\": \"0.0.1\",";
      manifest += "\"name\": \"text-to-speech\",";
      manifest += "\"displayname\": \"Mycroft Text to Speech\",";
      manifest += "\"instanceID\": \"text2speech\",";
      manifest += "\"capabilities\": {";
      manifest += "\"tts\": \"0.1\"";
      manifest += "},";
      manifest += "\"API\": \"0\",";
      manifest += "\"description\": \"Using .NET to provide mycroft a most excellent voice\",";
      manifest += "\"dependencies\": {";
      manifest += "  \"logger\": \"1.0\"";
      manifest += "}}";
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