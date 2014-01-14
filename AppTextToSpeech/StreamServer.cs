using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AppTextToSpeech
{
  class StreamServer
  {
    private ConcurrentDictionary<string, MsgQuery> messages;
    private int port;

    /// <summary>
    /// Construct a new server
    /// </summary>
    /// <param name="messages">
    ///   Concurrent dictionary of uuid:message to reference which stream a connector should get
    /// </param>
    /// <param name="port">The port to listen on</param>
    public StreamServer(ConcurrentDictionary<string, MsgQuery> messages, int port)
    {
      this.messages = messages;
      this.port = port;
    }

    /// <summary>
    /// Start listening for connections and serving stuff to them.
    /// </summary>
    public void StartServing()
    {
      TcpListener listener = new TcpListener(IPAddress.Any, port);
      listener.Start(15);
      System.Diagnostics.Debug.WriteLine("Started server on port " + port);

      while (true)
      {
        Stream stream = listener.AcceptTcpClient().GetStream();
        byte[] bytes = new byte[36];
        stream.Read(bytes, 0, bytes.Length);
        string guid = Encoding.UTF8.GetString(bytes);
        MsgQuery query = null;
        bool found = messages.TryRemove(guid, out query);
        if (!found) // we didn't find the query, wat
        {
          System.Diagnostics.Debug.WriteLine("Could not find guid " + guid);
          continue;
        }
        System.Diagnostics.Debug.WriteLine("Sending data for guid " + guid);
        query.Output.Seek(0, 0);
        Task foo = query.Output.CopyToAsync(stream);
        foo.Start();
      }
    }
  }
}