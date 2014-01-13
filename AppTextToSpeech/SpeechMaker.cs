using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace AppTextToSpeech
{
  /// <summary>
  /// A Collection of static utilities for speech generation
  /// </summary>
  class SpeechMaker
  {
    /// <summary>
    /// Generate speech out of this system's speakers
    /// </summary>
    /// <param name="speechText">What to say</param>
    public static void Speech(string speechText)
    {
      AudioSource audioSource = AudioSource.DefaultAudioDevice;
      Speech(speechText, audioSource);
    }

    /// <summary>
    /// Generate speech
    /// </summary>
    /// <param name="speechText">What to say</param>
    /// <param name="outputSelection">where to output</param>
    public static void Speech(string speechText, AudioSource outputSelection)
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
  }
}
