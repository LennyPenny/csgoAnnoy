﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using CSGSI;
using CSGSI.Nodes;
using Rant;

namespace csgoAnnoy
{
    class Program
    {
        private static GameStateListener gsl;

        private static List<string> taunts = new List<string>();
        private static List<string> hstaunts = new List<string>();

        private static RantEngine rant = new RantEngine();

        private static Random rndm;

        private static TcpClient tn;

        private static void RunCmd(string cmd, string args = "")
        {
            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.CreateNoWindow = true;
            //psi.WindowStyle = ProcessWindowStyle.Hidden;
            //psi.FileName = "SourceCmd";
            //psi.Arguments = $"csgo.exe \"{cmd} {args};\"";
            //Process.Start(psi);
            var bytes = Encoding.UTF8.GetBytes($"{cmd} {args};\n");
            tn.GetStream().Write(bytes, 0, bytes.Length);
        }

        private static void OnKill(bool headshot = false)
        {
            Console.WriteLine($"Killed someone! headshot: {headshot}");

            if (headshot)
                RunCmd("say", rant.Do(hstaunts[rndm.Next(hstaunts.Count)]));
            else
                RunCmd("say", rant.Do(taunts[rndm.Next(taunts.Count)]));
        }

        protected static void myHandler(object sender, ConsoleCancelEventArgs args) {
            tn.Close();

            Console.WriteLine("asd");
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            // connect to game socket
            tn = new TcpClient();
            tn.Connect("127.0.0.1", 2121);
            RunCmd("echo", "hello world!!");
            
            //load taunts
            taunts.AddRange(File.ReadAllLines("taunts.txt"));
            hstaunts.AddRange(File.ReadAllLines("hstaunts.txt"));

            rndm = new Random(unchecked((int)DateTime.Now.Ticks));

            rant.LoadPackage("Rantionary");
            rant.Dictionary.IncludeHiddenClass("nsfw");

            gsl = new GameStateListener(3000);

            gsl.NewGameState += gs =>
            {
                if (gs.Previously.Player.MatchStats.Kills != -1)
                {
                    if (gs.Previously.Player.State.RoundKillHS != -1)
                        OnKill(true);
                    else
                        OnKill();
                }
            };

            if (!gsl.Start()) Environment.Exit(0);

            Console.WriteLine("Listening for game events");
        }
    }
}
