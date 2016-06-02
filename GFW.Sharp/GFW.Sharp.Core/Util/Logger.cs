using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Util
{
    public class Logger
    {
        public static void ThreadWrite(string message)
        {
            Console.WriteLine(string.Format("{0}:Thd-{2}\t{1}", DateTime.Now.TimeOfDay, message,System.Threading.Thread.CurrentThread.ManagedThreadId));
        }

        public static void Write(string message)
        {
            Console.WriteLine(string.Format("{0}:\t{1}", DateTime.Now.TimeOfDay, message));
        }
    }
}
