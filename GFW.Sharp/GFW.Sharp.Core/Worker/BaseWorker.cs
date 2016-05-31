using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Worker
{
    public abstract class BaseWorker
    {
        //private ManualResetEvent _manualRstEvt;
        private DateTime _startupTime;
        public DateTime StartupTime
        {
            get { return _startupTime; }
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            //set { _isWorking = value; }
        }

        private int _interval;

        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        protected abstract void PerformWork();

        public BaseWorker()
        {
            _interval = 10;
            //_manualRstEvt = new ManualResetEvent(true);

        }


        public virtual void Start()
        {
            _isWorking = true;
            _startupTime = DateTime.Now;
            Task.Factory.StartNew(() => { Work(); });
        }

        public void Work()
        {
            while (_isWorking)
            {
                //_manualRstEvt.WaitOne();

                try
                {
                    PerformWork();
                }
                catch (Exception ex)
                {

                }

                Thread.Sleep(Interval);
            }
        }

        public void Pause()
        {
            //_manualRstEvt.Reset();
        }

        public void Resume()
        {
            //_manualRstEvt.Set();
        }

        public virtual void Stop()
        {
            _isWorking = false;
        }

    }
}
