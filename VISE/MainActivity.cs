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
using System.Threading;

namespace VISE
{
    [Activity(Label = "VISE", MainLauncher = true, Theme = "@style/mainTheme")]
    public class MainActivity : AppCompatActivity
    {
        private CardView CardView { get; set; }

        private ImageView CardViewIcon { get; set; }
        public VlcClient Client { get; set; }
        private TextView ConnectionTextView { get; set; }
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

        private BubbleSeekBar VolumeSeekbar { get; set; }
        private SeekBar TimeShiftSeekbar { get; set; }

        private Settings Settings { get; } = new Settings();

        private ImageButton SkipButton { get; set; }

        private TextView TimeTextView { get; set; }
        private TextView TimeShiftTextView { get; set; }
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

                if (VolumeSeekbar.Progress != vlcStatus.Volume)
                    ReceivedPackagesCount++;

                if (ReceivedPackagesCount >= 2)
                {
                    VolumeSeekbar.SetProgress(vlcStatus.Volume);
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
            TimeShiftTextView = FindViewById<TextView>(Resource.Id.timeShiftText);
            TitleTextView = FindViewById<TextView>(Resource.Id.titleText);
            IpTextView = FindViewById<TextView>(Resource.Id.ipText);
            ConnectionTextView = FindViewById<TextView>(Resource.Id.connectionTextView);

            RootLayout = FindViewById<RelativeLayout>(Resource.Id.rootMainPageLayout);

            CardView = FindViewById<CardView>(Resource.Id.cardView);
            CardViewIcon = FindViewById<ImageView>(Resource.Id.cardViewIcon);

            SkipButton = FindViewById<ImageButton>(Resource.Id.skipButton);
            SkipButton.Click += SkipButtonOnClick;

            FastForwardButton = FindViewById<ImageButton>(Resource.Id.fastForwardButton);
            FastForwardButton.Click += FastForwardButtonOnClick;

            FullscreenButton = FindViewById<ImageButton>(Resource.Id.fullscreenButton);
            FullscreenButton.Click += FullscreenButtonOnClick;

            PlayPauseButton = FindViewById<MorphButton>(Resource.Id.playPauseBtn);
            PlayPauseButton.Click += PlayPauseButtonOnClick;

            VolumeSeekbar = FindViewById<BubbleSeekBar>(Resource.Id.volumeSeekBar);
            VolumeSeekbar.ProgressChanged += SeekbarOnProgressChanged;

            TimeShiftSeekbar = FindViewById<SeekBar>(Resource.Id.timeShiftBar);
            TimeShiftSeekbar.StopTrackingTouch += TimeShiftSeekbar_StopTrackingTouch;
            TimeShiftSeekbar.ProgressChanged += TimeShiftSeekbar_ProgressChanged;

            IsEnabled = false;
        }

        private void TimeShiftSeekbar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            switch (e.SeekBar.Progress)
            {
                case 0: //-10 Seconds
                    TimeShiftTextView.Text = "-10 Segundos";
                    break;
                case 1: //-30 Seconds
                    TimeShiftTextView.Text = "-30 Segundos";
                    break;
                case 2: //-1:00 Seconds
                    TimeShiftTextView.Text = "-1 Minuto";
                    break;
                case 3: //-1:30 Seconds
                    TimeShiftTextView.Text = "-1 Minuto e 30 Segundos";
                    break;
                case 4:
                    TimeShiftTextView.Text = "Mova a barra para avançar / retroceder o tempo";
                    break;
                case 5: //+10 Seconds
                    TimeShiftTextView.Text = "+10 Segundos";
                    break;
                case 6: //+30 Seconds
                    TimeShiftTextView.Text = "+30 Segundos";
                    break;
                case 7: //+1:00 Seconds
                    TimeShiftTextView.Text = "+1 Minuto";
                    break;
                case 8: //+1:30 Seconds
                    TimeShiftTextView.Text = "+1 Minuto e 30 Segundos";
                    break;
            }
        }

        private async void TimeShiftSeekbar_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            switch (e.SeekBar.Progress)
            {
                case 0: //-10 Seconds
                    await Client.SetRelativePosition(10);
                    break;
                case 1: //-30 Seconds
                    await Client.SetRelativePosition(30);
                    break;
                case 2: //-1:00 Seconds
                    await Client.SetRelativePosition(60);
                    break;
                case 3: //-1:30 Seconds
                    await Client.SetRelativePosition(-90);
                    break;
                case 5: //+10 Seconds
                    await Client.SetRelativePosition(10);
                    break;
                case 6: //+30 Seconds
                    await Client.SetRelativePosition(30);
                    break;
                case 7: //+1:00 Seconds
                    await Client.SetRelativePosition(60);
                    break;
                case 8: //+1:30 Seconds
                    await Client.SetRelativePosition(90);
                    break;
            }

            RunOnUiThread(() => TimeShiftSeekbar.SetProgress(4, true));
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

            await Client.SetVolume(VolumeSeekbar.Progress);
            ReceivedPackagesCount = 0;
        }

        private async void SkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            if ((await Client.GetPlaylist()).First().Leaves?.OrderBy(i => i.Name).Last()?.Current != null)
                await AddNextEpisode();

            await Client.Next();
        }
    }
}