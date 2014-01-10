using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;

namespace SimpleSpeak
{
  class Program
  {
    static void Main(string[] args)
    {
      SpeechSynthesizer s = new SpeechSynthesizer();
      s.Speak("Hello. My name is Microsoft Anna.");

    }
  }
}