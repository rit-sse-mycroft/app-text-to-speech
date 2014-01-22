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
        private string arg;

        public TextToSpeechClient() : base()
        {
            factory = new TwoflowerVLCFactory();
            speakers = new Dictionary<string, MycroftSpeaker>();
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
            arg = ":sout=#transcode{vcodec=none,acodec=s16l,ab=128,channels=2,samplerate=8000}:rtp{sdp=rtsp://:1234/stream.sdp} :sout-keep";
        }

        private string[] GenerateArguments(int port)
        {
            return new string[] {":sout=#transcode{vcodec=none,acodec=s16l,ab=128,channels=2,samplerate=8000}:rtp{sdp=rtsp://" + ipAddress + ":" + port + "/stream.sdp}" ,":sout-keep"};
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
            var dep = message["audioOutput"];
            foreach (var kv in dep)
            {
                string instance = kv.Key;
                string state = kv.Value;

                if (!speakers.ContainsKey(instance))
                {
                    MycroftSpeaker speaker = new MycroftSpeaker(instance, state, port, factory.InitializeVLC(GenerateArguments(port)));
                    speakers[instance] = speaker;
                    port++;
                }
                speakers[instance].Status = state;
            }
        }

        protected override void Response(MSG_QUERY type, dynamic message)
        {
            throw new NotImplementedException();
        }


    }
}
