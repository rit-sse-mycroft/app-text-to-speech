using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;

namespace AppTextToSpeech
{
  class Program
  {
    static void Main(string[] args)
    {
      var client = new MycroftClient("localhost", 1847);
      client.ListenForCommands();
    }
  }
}