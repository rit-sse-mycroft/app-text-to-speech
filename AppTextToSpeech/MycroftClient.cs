using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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
    }

    /// <summary>
    /// Sends the given string to Mycroft server
    /// </summary>
    /// <param name="message">the full message to send</param>
    public void TellMycroft(String message)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(message);
      byte[] header = Encoding.UTF8.GetBytes(bytes.Length + "\n");
      client.Write(header, 0, header.Length);
      client.Write(bytes, 0, bytes.Length);
    }
  }
}