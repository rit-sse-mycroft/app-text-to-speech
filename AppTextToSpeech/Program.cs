using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppTextToSpeech
{
  enum AudioSource { AudioStream, DefaultAudioDevice, Null, WaveFile, WaveStream };
    
  class Program
  { 
    static void Main(string[] args)
    {
      var client = new MycroftClient("localhost", 1847);
      client.ListenForCommands();
    }
  }
}