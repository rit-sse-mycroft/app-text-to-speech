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
    private SslStream stream;

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
      var tcpConn = new TcpClient(host, port);
      stream = new SslStream(
        tcpConn.GetStream(),
        false,
        new RemoteCertificateValidationCallback(ValidateServerCertificate),
        new LocalCertificateSelectionCallback(SelectLocalCertificate)
        );
    }

    /// <summary>
    /// Sends the given string to Mycroft server
    /// </summary>
    /// <param name="message">the full message to send</param>
    public void TellMycroft(String message)
    {
    }

    public static bool ValidateServerCertificate(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
      return true; // we don't care if the server is valid
    }

    public static X509Certificate SelectLocalCertificate(
      object sender,
      string targetHost, 
      X509CertificateCollection localCertificates, 
      X509Certificate remoteCertificate, 
      string[] acceptableIssuers) {
      return null;
    }

  }
}