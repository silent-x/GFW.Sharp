using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PingRecorder
{
    public class PingWorker
    {
        public event OnPingReplied PingReplied;
        public delegate void OnPingReplied(object sender, PingReply reply);

        private string _hostName;

        public string HostName
        {
            get { return _hostName; }
            //set { _hostName = value; }
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            //set { _isWorking = value; }
        }

        private Ping _ping;

        public PingWorker(string hostName)
        {
            _hostName = hostName;
            Initial();
        }

        private void Initial()
        {
            _ping = new Ping();
        }

        public void StartWork()
        {
            _isWorking = true;
            new Task(Work).Start();
        }

        public void StopWork()
        {
            _isWorking = false;
        }

        private void Work()
        {
            while (_isWorking)
            {
                //var reply = _ping.Send(HostName);
                PingReply reply = null;
                try
                {
                    reply = _ping.Send(HostName);
                }
                catch
                {

                }

                if (PingReplied != null && reply != null)
                {
                    PingReplied(this, reply);
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        
    }
}
