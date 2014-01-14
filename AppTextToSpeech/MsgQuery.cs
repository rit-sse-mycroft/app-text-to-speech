using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AppTextToSpeech
{

  class MsgQuery
  {
    public string TargetSpeakers;
    public string Text;
    public string UUID;
    public string Procedure;
    public Stream Output;
  }
}
