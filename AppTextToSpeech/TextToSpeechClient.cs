using DZ.MediaPlayer;
using DZ.MediaPlayer.Vlc;
using DZ.MediaPlayer.Vlc.Deployment;
using DZ.MediaPlayer.Vlc.Common;
using DZ.MediaPlayer.Vlc.Io;
using Mycroft.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Net;
using Mycroft.App.Message;

namespace AppTextToSpeech
{
    public class TextToSpeechClient : Server
    {
        private VlcMediaLibraryFactory factory;
        private string ipAddress;
        private int port;

        public TextToSpeechClient() : base()
        {
            factory = new VlcMediaLibraryFactory(new string[] { });
            factory.CreateSinglePlayers = true;
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                ipAddress = stream.ReadToEnd();
            }

            //Search for the ip in the html
            int first = ipAddress.IndexOf("Address: ") + 9;
            int last = ipAddress.LastIndexOf("</body>");
            ipAddress = ipAddress.Substring(first, last - first);
            port = 3000;
        }

        protected async override void Response(APP_MANIFEST_OK type, dynamic message)
        {
            InstanceId = message["instanceId"];
            Console.WriteLine("Recieved: " + type);
            await SendJson("APP_UP", new { });
            return;
        }
    }
}
