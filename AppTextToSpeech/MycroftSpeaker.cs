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

        public MycroftSpeaker(string instanceId, string status, int port)
        {
            this.instanceId = instanceId;
            this.status = status;
            this.port = port;
        }


        public string Status
        {
            get { return status;  }
            set { status = value; }
        }

        public string InstanceId
        {
            get { return instanceId; }
        }

        public int Port
        {
            get { return port; }
        }
      
    }
}
