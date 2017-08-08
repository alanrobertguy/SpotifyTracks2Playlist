using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System;
using System.Diagnostics;
using System.Globalization;


namespace SpotifyTracktoPlaylist
{
    public partial class Program
    {
        public static SpotifyLocalAPI _spotify;
        public static Track _currentTrack;

        public static void Main(string[] args)
        {
            _spotify = new SpotifyLocalAPI();
            Connect();
        }

        public static void Connect()
        {
            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                Console.WriteLine(@"Spotify isn't running!");
                return;
            }
            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                Console.WriteLine(@"SpotifyWebHelper isn't running!");
                return;
            }

            bool successful = _spotify.Connect();
            if (successful)
            {
                Console.WriteLine(@"Connection to Spotify successful");                
                UpdateInfos();
                _spotify.ListenForEvents = true;
            }
            else
            {
                Console.WriteLine(@"Couldn't connect to the spotify client");                
            }
        }

        public static void UpdateInfos()
        {
            StatusResponse status = _spotify.GetStatus();
            if (status == null)
                return;

            //Basic Spotify Infos
            Console.WriteLine(status.Playing);
            Console.WriteLine(status.ClientVersion);
            Console.WriteLine(status.Version.ToString());            

            if (status.Track != null) //Update track infos
                Console.WriteLine(status.Track);
        }
    }
}
