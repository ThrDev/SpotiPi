﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace spotipi
{
    class Program
    {
        private static SpotySong song = new SpotySong();
        private static spotify m = new spotify();
        private static string songname = null;
        static void Main(string[] args)
        {

            //listen for HTTP requests.
            new Thread(new ThreadStart(delegate()
            {
                Socket listen1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listen1.Bind(new IPEndPoint(IPAddress.Any, 5555));
                listen1.Listen(1000);
                Console.WriteLine("Listening on port 5555.");
                while (true)
                    new Thread(new ParameterizedThreadStart(PortHandler)).Start(listen1.Accept());

            })).Start();
        }

        static public void PortHandler(object _socket)
        {
            Socket client = (Socket)_socket;
            string request = "";
            try
            {
                byte[] buffer = new byte[1024];
                int got = client.Receive(buffer);
                if (got < 0)
                {
                    try
                    {
                        client.Close();
                    }
                    catch { }
                    return;
                }
                request = ASCIIEncoding.ASCII.GetString(buffer, 0, got).Trim();
            }
            catch
            {
                try
                {
                    client.Close();
                }
                catch { }
                return;
            }
            if (m.Nowplaying() == "Spotify")
            {
                song.artist = "";
                song.songtitle = "Not Playing.";
            }
            try
            {
                if (songname == null || song.songtitle == "Not Playing." || songname != m.Nowplaying().Replace("Spotify - ", ""))
                {
                    songname = m.Nowplaying().Replace("Spotify - ", "");
                    string[] x = m.Nowplaying().Replace("Spotify - ", "").Split('–');
                    song.songtitle = x[1].Substring(1, x[1].Length - 1);
                    song.artist = x[0].Substring(0, x[0].Length - 1);
                }
            }
            catch
            {
                song.artist = "";
                song.songtitle = "Not Playing.";
            }
            //return last two messages.
            string lastshit = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nConnection: keepalive\r\nKeep-Alive: 300\r\nContent-Length: {0}\r\n\r\n{1}";
            //send shit to client.
            string output = song.artist + "\n" + song.songtitle;
            client.Send(Encoding.ASCII.GetBytes(string.Format(lastshit, output.Length, output)));
            client.Close();
            return;
        }
    }
    public class SpotySong
    {
        public string songtitle { get; set; }
        public string artist { get; set; }
    }
    public class spotify
    {
        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Ansi)]
        public static extern bool GetWindowText(IntPtr hWnd, [OutAttribute()] StringBuilder strNewWindowName,
            Int32 maxCharCount);

        private IntPtr w;
        public spotify()
        {
            w = FindWindow("SpotifyMainWindow", null);
        }

        public string Nowplaying()
        {
            StringBuilder sbWinText = new StringBuilder(256);
            GetWindowText(w, sbWinText, 256);
            return sbWinText.ToString();
        }


    }
}