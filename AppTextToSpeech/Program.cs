﻿using System;
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
      var messages = new ConcurrentDictionary<string, MsgQuery>();

      var client = new MycroftClient("localhost", 1847, messages);
      var server = new StreamServer(messages, 32761);

      Thread serverThread = new Thread(new ThreadStart(server.StartServing));
      client.ListenForCommands();
    }
  }
}