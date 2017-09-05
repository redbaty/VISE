using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using Com.XW.Repo;
using Sof.Vlc.Http;
using Sof.Vlc.Http.Data;
using VectorCompat;

namespace VISE
{
    [Activity(Label = "VISE", MainLauncher = true, Theme = "@style/mainTheme")]
    public class MainActivity : AppCompatActivity
    {
        private CardView CardView { get; set; }

        private ImageView CardViewIcon { get; set; }
        public VlcClient Client { get; set; }
        private TextView ConnectionTextView { get; set; }
        private ImageButton ExitFullscreenpButton { get; set; }
        private ImageButton FastForwardButton { get; set; }
        private ImageButton FullscreenButton { get; set; }
        private TextView IpTextView { get; set; }

        private bool IsEnabled
        {
            set
            {
                for (var i = 0; i < RootLayout.ChildCount; i++)
                {
                    var child = RootLayout.GetChildAt(i);
                    child.Enabled = value;
                }
            }
        }

        private MorphButton PlayPauseButton { get; set; }

        private int ReceivedPackagesCount { get; set; }
        private RelativeLayout RootLayout { get; set; }

        private BubbleSeekBar Seekbar { get; set; }

        private Settings Settings { get; } = new Settings();

        private ImageButton SkipButton { get; set; }
        private ImageButton TimeSkipButton { get; set; }

        private TextView TimeTextView { get; set; }
        private TextView TitleTextView { get; set; }

        private async Task AddNextEpisode()
        {
            try
            {
                var current = await GetCurrentLeaf();
                var digitsRegex = new Regex(@"\b(\d+)\b");
                var path = Path.GetDirectoryName(WebUtility.UrlDecode(current?.Uri).Replace("file:///", ""));
                var dir = await Client.GetDirectoryContents(path);
                var episodes = new Dictionary<int, VlcDirectoryItem>();

                if (current != null)
                {
                    var currentEpisode = Convert.ToInt32(digitsRegex.Match(current.Name).ToString());

                    foreach (var directoryItem in dir)
                    {
                        var match = digitsRegex.Match(directoryItem.Name);
                        if (match.Length > 0)
                            episodes.Add(Convert.ToInt32(match.ToString()), directoryItem);
                    }

                    episodes = episodes.OrderBy(i => i.Key).ToDictionary(t => t.Key, t => t.Value);

                    var setNextEpisode = false;

                    if (episodes.Last().Key == currentEpisode)
                        return;

                    foreach (var pair in episodes)
                    {
                        if (setNextEpisode)
                        {
                            await Client.Add(pair.Value.Path);
                            break;
                        }

                        if (pair.Key == currentEpisode)
                            setNextEpisode = true;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ClientOnConnectionChanged(object sender, bool b)
        {
            RunOnUiThread(() =>
            {
                IsEnabled = b;
                if (b)
                {
                    ConnectionTextView.SetText(Resource.String.app_connected_string);
                    CardView.SetCardBackgroundColor(ContextCompat.GetColor(this,
                        Resource.Color.cardview_connected_color));
                    CardViewIcon.SetImageResource(Resource.Drawable.ic_done_black_18dp);
                    IpTextView.Text = Settings.Ip;
                }
                else
                {
                    ConnectionTextView.SetText(Resource.String.app_disconnected_string);
                    CardView.SetCardBackgroundColor(ContextCompat.GetColor(this, Resource.Color.cardview_error_color));
                    CardViewIcon.SetImageResource(Resource.Drawable.ic_error_outline_black_18dp);
                    IpTextView.Text = "";
                    TitleTextView.Text = "";
                }
            });
        }

        private void ClientOnStatusUpdated(object sender, VlcStatus vlcStatus)
        {
            var time = new TimeSpan(0, 0, 0, vlcStatus.Time);

            RunOnUiThread(() =>
            {
                TimeTextView.Text = time.ToString();

                if (Seekbar.Progress != vlcStatus.Volume)
                    ReceivedPackagesCount++;

                if (ReceivedPackagesCount >= 2)
                {
                    Seekbar.SetProgress(vlcStatus.Volume);
                    ReceivedPackagesCount = 0;
                }

                try
                {
                    var title = "";
                    try
                    {
                        title = vlcStatus.Information.Category.First().Info.First(i => i.Name == "title").Text;
                    }
                    catch
                    {
                        try
                        {
                            title = Path.GetFileNameWithoutExtension(vlcStatus.Information.Category.First()
                                .Info
                                .First(i => i.Name == "filename").Text);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    TitleTextView.Text = title;
                }
                catch
                {
                    TitleTextView.Text = "";
                }

                if (vlcStatus.State == VlcPlaybackState.Playing)
                {
                    if (PlayPauseButton.State != MorphButton.MorphState.Start)
                        PlayPauseButton.SetState(MorphButton.MorphState.Start, true);
                }
                else
                {
                    if (PlayPauseButton.State != MorphButton.MorphState.End)
                        PlayPauseButton.SetState(MorphButton.MorphState.End, true);
                }
            });
        }

        private async void ExitFullscreenpButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Client.Status.IsFullScreen)
                await Client.ToggleFullscreen();
        }

        private async void FastForwardButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Client.Status.Rate > 1)
                await Client.SetRate(1);
            else
                await Client.SetRate(4);
        }

        private async void FullscreenButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (!Client.Status.IsFullScreen)
                await Client.ToggleFullscreen();
        }

        private async Task<VlcPlaylistLeaf> GetCurrentLeaf()
        {
            var playlist = await Client.GetPlaylist();
            VlcPlaylistLeaf current = null;

            foreach (var playlistNode in playlist)
                if (playlistNode.Leaves != null && playlistNode.Leaves.Any(i => i.Current != null))
                    current = playlistNode.Leaves.First(i => i.Current != null);
            return current;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);

            Client = new VlcClient(Settings.Ip, 8080, "88134165");
            Client.ConnectionChanged += ClientOnConnectionChanged;
            Client.StatusUpdated += ClientOnStatusUpdated;

            TimeTextView = FindViewById<TextView>(Resource.Id.timeTextView);
            TitleTextView = FindViewById<TextView>(Resource.Id.titleText);
            IpTextView = FindViewById<TextView>(Resource.Id.ipText);
            ConnectionTextView = FindViewById<TextView>(Resource.Id.connectionTextView);

            RootLayout = FindViewById<RelativeLayout>(Resource.Id.rootMainPageLayout);

            CardView = FindViewById<CardView>(Resource.Id.cardView);
            CardViewIcon = FindViewById<ImageView>(Resource.Id.cardViewIcon);

            SkipButton = FindViewById<ImageButton>(Resource.Id.skipButton);
            FastForwardButton = FindViewById<ImageButton>(Resource.Id.fastForwardButton);
            TimeSkipButton = FindViewById<ImageButton>(Resource.Id.skipTimeButton);
            FullscreenButton = FindViewById<ImageButton>(Resource.Id.fullScreenButton);
            ExitFullscreenpButton = FindViewById<ImageButton>(Resource.Id.exitFullscreenButton);

            PlayPauseButton = FindViewById<MorphButton>(Resource.Id.playPauseBtn);

            Seekbar = FindViewById<BubbleSeekBar>(Resource.Id.volumeSeekBar);

            PlayPauseButton.Click += PlayPauseButtonOnClick;
            SkipButton.Click += SkipButtonOnClick;
            TimeSkipButton.Click += TimeSkipButtonOnClick;
            FastForwardButton.Click += FastForwardButtonOnClick;
            Seekbar.ProgressChanged += SeekbarOnProgressChanged;
            FullscreenButton.Click += FullscreenButtonOnClick;
            ExitFullscreenpButton.Click += ExitFullscreenpButtonOnClick;
            IsEnabled = false;
        }

        private async void PlayPauseButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Client.Status.State == VlcPlaybackState.Playing)
                await Client.Pause();
            else
                await Client.Play();
        }

        private async void SeekbarOnProgressChanged(object sender,
            BubbleSeekBar.ProgressChangedEventArgs progressChangedEventArgs)
        {
            if (!Client.IsConnected)
                return;

            await Client.SetVolume(Seekbar.Progress);
            ReceivedPackagesCount = 0;
        }

        private async void SkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            if ((await Client.GetPlaylist()).First().Leaves?.OrderBy(i => i.Name).Last()?.Current != null)
                await AddNextEpisode();

            await Client.Next();
        }

        private async void TimeSkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            await Client.SetRelativePosition(Settings.SkipTime);
        }
    }
}