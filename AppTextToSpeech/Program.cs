using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace AppTextToSpeech
{
    enum AudioSource { AudioStream, DefaultAudioDevice, Null, WaveFile, WaveStream };
    
    class Program
    { 
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Expected arguments in the form speechrecognizer host port");
                return;
            }
            var client = new TextToSpeechClient("app.json");
            client.Connect(args[0], args[1]);
        }
    }
}