using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Text;

namespace SpotifyTracktoPlaylist
{
    public partial class Program
    {
        public static SpotifyLocalAPI _spotifyLocal;
        public static Track _currentTrack;
        public static SpotifyWebAPI _spotify;
        private static PrivateProfile _profile;
        private static List<FullTrack> _savedTracks;
        private static List<SimplePlaylist> _playlists;

        public static void Main(string[] args)
        {
            _spotifyLocal = new SpotifyLocalAPI();

            AsyncContext.Run(() => WebConnect());
            
        }

        static async void InitialSetup() /*Web only */
        {
            
            _profile = await _spotify.GetPrivateProfileAsync();
           // _savedTracks = GetSavedTracks();
            //_playlists = GetPlaylists();
            string sCurrent = _spotify.GetPlayingTrack().Item.Uri;
            //4Rfx0al9GwQF9fQndihutN

            PrivateProfile user = _spotify.GetPrivateProfile();


            _spotify.AddPlaylistTrack("chibaken", "4Rfx0al9GwQF9fQndihutN", sCurrent);

        }

        static List<FullTrack> GetSavedTracks()
        {
            Paging<SavedTrack> savedTracks = _spotify.GetSavedTracks();
            List<FullTrack> list = savedTracks.Items.Select(track => track.Track).ToList();

            while (savedTracks.Next != null)
            {
                savedTracks = _spotify.GetSavedTracks(20, savedTracks.Offset + savedTracks.Limit);
                list.AddRange(savedTracks.Items.Select(track => track.Track));
            }

            return list;
        }

        static List<SimplePlaylist> GetPlaylists()
        {
            Paging<SimplePlaylist> playlists = _spotify.GetUserPlaylists(_profile.Id);
            List<SimplePlaylist> list = playlists.Items.ToList();

            while (playlists.Next != null)
            {
                playlists = _spotify.GetUserPlaylists(_profile.Id, 20, playlists.Offset + playlists.Limit);
                list.AddRange(playlists.Items);
            }

            return list;
        }

        //see https://developer.spotify.com/web-api/authorization-guide/#client_credentials_flow

        
        

        public static void LocalConnect()
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

            bool successful = _spotifyLocal.Connect();
            if (successful)
            {
                Console.WriteLine(@"Connection to Spotify successful");                
                UpdateInfos();
                _spotifyLocal.ListenForEvents = true;
            }
            else
            {
                Console.WriteLine(@"Couldn't connect to the spotify client");                
            }
        }

        public static void UpdateInfos()
        {
            StatusResponse status = _spotifyLocal.GetStatus();
            if (status == null)
                return;

            //Basic Spotify Infos
            Console.WriteLine(status.Playing);
            Console.WriteLine(status.ClientVersion);
            Console.WriteLine(status.Version.ToString());

            string sTrackURI = status.Track.TrackResource.Uri;


                       

            if (status.Track != null) //Update track infos
                Console.WriteLine(status.Track);
        }

        static async void WebConnect()
        {
            WebAPIFactory webApiFactory = new WebAPIFactory(
                "http://localhost",
                8000,
                "50276ac5aa86430cb42564346bab5b04",                
                Scope.UserReadPrivate | Scope.UserReadEmail | Scope.PlaylistReadPrivate | Scope.UserLibraryRead |
                Scope.UserReadPrivate | Scope.UserFollowRead | Scope.UserReadBirthdate | Scope.UserTopRead | Scope.PlaylistReadCollaborative |
                Scope.UserReadRecentlyPlayed | Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState | Scope.PlaylistModifyPrivate | Scope.PlaylistModifyPublic);

            //"26d287105e31491889f3cd293d85bfea", 50276ac5aa86430cb42564346bab5b04

            try
            {
                _spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_spotify == null)
                return;

            InitialSetup();
        }

        static void GetClientCredentialsAuthToken()
        {
            var spotifyClient = "50276ac5aa86430cb42564346bab5b04";
            var spotifySecret = "8b7ce096f19044d8a761039c4073c2bd";

            var webClient = new WebClient();

            var postparams = new NameValueCollection();
            postparams.Add("grant_type", "client_credentials");

            var authHeader = Convert.ToBase64String(Encoding.Default.GetBytes($"{spotifyClient}:{spotifySecret}"));
            webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + authHeader);

            var tokenResponse = webClient.UploadValues("https://accounts.spotify.com/api/token", postparams);

            var textResponse = Encoding.UTF8.GetString(tokenResponse);
        }
    }
}
