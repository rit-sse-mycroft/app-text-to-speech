using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;

namespace SimpleSpeak
{
  enum AudioSource { AudioStream, DefaultAudioDevice, Null, WaveFile, WaveStream };
    
  class Program
  {
    static void Speech(string speechText, AudioSource outputSelection)
    {
      SpeechSynthesizer synth = new SpeechSynthesizer();
      AudioSource source = outputSelection;
      switch (source)
      {
        //case AudioSource.AudioStream:
        //  synth.SetOutputToAudioStream();
        //  break;
        case AudioSource.Null:
          synth.SetOutputToNull();
          break;
        //case AudioSource.WaveFile:
        //  synth.SetOutputToWaveFile();
        //  break;
        //case AudioSource.WaveStream:
        //  synth.SetOutputToWaveStream();
        //  break;
        case AudioSource.DefaultAudioDevice:
        default:
          synth.SetOutputToDefaultAudioDevice();
          break;
      }
      synth.Speak(speechText);
    } 
    static void Main(string[] args)
    {
      string speechText = "";
      AudioSource audioSource = AudioSource.DefaultAudioDevice;
      //TODO: MycroftClient defines text and source
      Speech(speechText, audioSource);
    }
  }
}