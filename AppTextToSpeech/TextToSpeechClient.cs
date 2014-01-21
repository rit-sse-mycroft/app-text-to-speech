using com.ptrampert.LibVLCBind;
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
        private IVLCFactory factory;
        private Dictionary<string, MycroftSpeaker> speakers;
        private string ipAddress;
        private int port;

        public TextToSpeechClient() : base()
        {
            factory = new TwoflowerVLCFactory();
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

        protected override void Response(APP_DEPENDENCY type, dynamic message)
        {
            throw new NotImplementedException();
        }

        protected override void Response(MSG_QUERY type, dynamic message)
        {
            throw new NotImplementedException();
        }


    }
}
