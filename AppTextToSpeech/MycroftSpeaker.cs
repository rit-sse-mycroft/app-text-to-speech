using com.ptrampert.LibVLCBind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTextToSpeech
{
    public class MycroftSpeaker
    {
        private string instanceId;
        private string status;
        private int port;
        private IVLCInstance vlcInstance;

        public MycroftSpeaker(string instanceId, string status, int port, IVLCInstance vlcInstance)
        {
            this.instanceId = instanceId;
            this.status = status;
            this.port = port;
            this.vlcInstance = vlcInstance;
        }

        public IVLCInstance VlcInstance
        {
            get { return vlcInstance; }
        }

        public string Status
        {
            get { return status;  }
        }
      
    }
}
