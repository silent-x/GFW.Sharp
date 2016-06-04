using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Forward.GFWPress;
using GFW.Sharp.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;


namespace GFW.Sharp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("");
            string lip="127.0.0.1";
            int lport = 8558;
            int rport = 0;
            string rip = null, p = null;

            var arguments = CommandLineArgumentParser.Parse(args);
            if (arguments.Has("-lip") && arguments.Get("-lip").Next != null && arguments.Get("-lip").Next.ToString()!=string.Empty && IsLocalIP(arguments.Get("-lip").Next))
            {
                lip = arguments.Get("-lip").Next;
            }

            if (arguments.Has("-lport") && arguments.Get("-lport").Next != null)
            {
                int newPort = 0;
                if(int.TryParse(arguments.Get("-lport").Next, out newPort) && newPort > 0 && newPort < 65536)
                {
                    lport = newPort;
                }
            }

            if (arguments.Has("-rip") && arguments.Get("-rip").Next != null && IsIP(arguments.Get("-rip").Next))
            {
                rip = arguments.Get("-rip").Next;
            }
            else
            {
                ShowHelp();
                return;
            }

            if (arguments.Has("-rport") && arguments.Get("-rport").Next != null && int.TryParse(arguments.Get("-rport").Next, out rport) && rport > 0 && rport < 65536)
            {

            }
            else
            {
                ShowHelp();
                return;
            }

            if (arguments.Has("-p") && arguments.Get("-p").Next != null && arguments.Get("-p").Next.ToString() != string.Empty && Core.Ciphering.Encrypt.isPassword(arguments.Get("-p").Next))
            {
                p = arguments.Get("-p").Next;
            }
            else
            {
                ShowHelp();
                return;
            }


            if (arguments.Has("-server"))
            {
                Logger.Write("Server is running! Press enter/ctrl+c to exit.");
                Logger.Write(string.Format("local ip:\t{0}", lip));
                Logger.Write(string.Format("local port:\t{0}", lport));
                Logger.Write(string.Format("remote ip:\t{0}", rip));
                Logger.Write(string.Format("remote port:\t{0}", rport));
                SecretKey key = new Encrypt().getPasswordKey(p);
                var server = new GFWPressForwardServerListener(IPAddress.Parse(lip), lport, IPAddress.Parse(rip), rport, key);
                server.Start();
            }
            else if (arguments.Has("-client"))
            {
                Logger.Write("Client is running! Press enter/ctrl+c to exit.");
                Logger.Write(string.Format("local ip:\t{0}", lip));
                Logger.Write(string.Format("local port:\t{0}", lport));
                Logger.Write(string.Format("remote ip:\t{0}", rip));
                Logger.Write(string.Format("remote port:\t{0}", rport));
                SecretKey key = new Encrypt().getPasswordKey(p);
                var client = new GFWPressForwardClientListener(IPAddress.Parse(lip), lport, IPAddress.Parse(rip), rport, key);
                client.Start();
            }
            else
            {
                ShowHelp();
                return;
            }
            System.Console.ReadLine();
        }

        private static void ShowHelp()
        {
            System.Console.WriteLine("GFW.Sharp, a c# port of gfw.press. Thanks to @chinashiyu.");
            System.Console.WriteLine("Parameters:");
            System.Console.WriteLine("");
            System.Console.WriteLine("-server/-client\t\truns under server/client mode.");
            System.Console.WriteLine("-lip\t\t\tlocal listen ip, default is 127.0.0.1.");
            System.Console.WriteLine("-lport\t\t\tlocal listen port, default is 8558.");
            System.Console.WriteLine("-rip\t\t\tremote destination ip, must be assigned.");
            System.Console.WriteLine("-rport\t\t\tremote port ip, must be assigned.");
            System.Console.WriteLine("-p\t\t\tpassword, 8+ characters with at least 1 digit, 1 upper case and 1 lower case, no space is allowed.");
            //System.Console.ReadLine();
        }

        private static bool IsLocalIP(string ip)
        {
            return true;
        }

        public static bool IsIP(string ip)
        {
            //判断是否为IP
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
    }
}
