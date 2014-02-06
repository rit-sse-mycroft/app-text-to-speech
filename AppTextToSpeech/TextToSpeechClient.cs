using Mycroft.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Net;
using System.Speech.Synthesis;
using System.Net.Sockets;
using System.Threading;

namespace AppTextToSpeech
{
    /// <summary>
    /// The text to speech application
    /// </summary>
    public class TextToSpeechClient : Client
    {
        private MycroftVoice voice;
        private Dictionary<string, MycroftSpeaker> speakers;
        private string ipAddress;
        private int port;

        /// <summary>
        /// The constructor for the TextToSpeechClient
        /// </summary>
        /// <param name="manifest">The path to app manifest</param>
        public TextToSpeechClient(string manifest) : base(manifest)
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
            port = 1500;
            handler.On("APP_MANIFEST_OK", AppManifestOk);
            handler.On("APP_DEPENDENCY", AppDependency);
            handler.On("MSG_QUERY", MsgQuery);
        }
        /// <summary>
        /// Makes a connection with speakers app to play audio
        /// </summary>
        /// <param name="data">the speaker and prompt</param>
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
        #region Message Handlers
        /// <summary>
        /// Handler for APP_MANIFEST_OK
        /// </summary>
        /// <param name="message">the message received</param>
        protected async void AppManifestOk(dynamic message)
        {
            InstanceId = message["instanceId"];
            await Up();
        }

        /// <summary>
        /// Handler for APP_DEPENDENCY
        /// </summary>
        /// <param name="message">The message received</param>
        protected void AppDependency(dynamic message)
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

        /// <summary>
        /// Handler for MSG_QUERY
        /// </summary>
        /// <param name="message">The message received</param>
        protected async void MsgQuery(dynamic message)
        {
            var data = message["data"];
            MycroftSpeaker speaker = speakers[data["targetSpeaker"]];
            if (speaker.Status != "up")
            {
                await QueryFail(message["id"], "Target speaker is " + speaker.Status);
            }
            else
            {
                var text = data["text"];
                PromptBuilder prompt = new PromptBuilder(new System.Globalization.CultureInfo("en-GB"));
                prompt.StartVoice(VoiceGender.Female, VoiceAge.Adult, 0);
                foreach (var phrase in text)
                {
                    prompt.AppendText(phrase["phrase"]);
                    prompt.AppendBreak(new TimeSpan((int)(phrase["delay"] * 10000000)));
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

                await Query("audioOutput", "stream_tts", new { ip = ipAddress, port = speaker.Port }, new string[] { speaker.InstanceId });
            }
        }
        #endregion
    }
}
