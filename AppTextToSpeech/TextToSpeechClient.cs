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
using System.Speech.Synthesis;
using System.Net.Sockets;
using System.Threading;

namespace AppTextToSpeech
{
    public class TextToSpeechClient : Server
    {
        private MycroftVoice voice;
        private Dictionary<string, MycroftSpeaker> speakers;
        private string ipAddress;
        private int port;

        public TextToSpeechClient() : base()
        {
            speakers = new Dictionary<string, MycroftSpeaker>();
            voice = new MycroftVoice();
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
            await SendData("APP_UP", "");
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
                    MycroftSpeaker speaker = new MycroftSpeaker(instance, state, port);
                    speakers[instance] = speaker;
                    port++;
                }
                speakers[instance].Status = state;
            }
        }

        protected async override void Response(MSG_QUERY type, dynamic message)
        {
            var data = message["data"];
            MycroftSpeaker speaker = speakers[data["targetSpeaker"]];
            if (speaker.Status != "up")
            {
                await SendJson("MSG_QUERY_FAIL", new { id = message["id"], message = "Target speaker is " + speaker.Status });
            }
            else
            {
                var text = data["text"];
                PromptBuilder prompt = new PromptBuilder(new System.Globalization.CultureInfo("en-GB"));
                prompt.StartVoice(VoiceGender.Female, VoiceAge.Adult, 0);
                foreach (var phrase in text)
                {
                    prompt.AppendText(phrase["phrase"]);
                    prompt.AppendBreak(new TimeSpan(phrase["delay"] * 10000000));
                }
                prompt.EndVoice();
                try
                {
                    prompt.AppendAudio("lutz.wav");
                }
                catch
                {

                }
                Thread t = new Thread(Listen);
                t.Start(new { speaker = speaker, prompt = prompt });

                SendJson("MSG_QUERY", new
                {
                    id = Guid.NewGuid(),
                    capability = "audioOutput",
                    instanceId = new string[] { speaker.InstanceId },
                    data = new { ip = ipAddress, port = speaker.Port },
                    priority = 30,
                    action = "stream_tts"
                });
            }
        }

        protected void Listen(dynamic data)
        {
            var speaker = data.speaker;
            TcpListener listener = new TcpListener(speaker.Port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            MemoryStream ms = new MemoryStream();
            voice.SaveMessage(data.prompt, ms);
            client.GetStream().Write(ms.GetBuffer(), 0, (int)ms.Length);
            client.Close();
            listener.Stop();
        }
    }
}
