using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace AppTextToSpeech
{
    class MycroftVoice
    {
        private SpeechSynthesizer synth;
        public MycroftVoice(){
            InitializeVoice();
        }

        private void InitializeVoice()
        {
            synth = new SpeechSynthesizer();
            synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult, 0,
                new System.Globalization.CultureInfo("en-GB"));
        }

        public void SayMessage(string message)
        {
            synth.Speak(message);
        }
    }
}
