using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using CheeseBind;
using Sof.Vlc.Http;
using Sof.Vlc.Http.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using VectorCompat;
using VISE.Managers;
using VISE.Models;

namespace VISE.Activies
{
    [Activity(Label = "VISE", Theme = "@style/mainTheme")]
    public class MainActivity : AppCompatActivity
    {
        public VlcClient Client { get; set; }
        private CardView CardView { get; set; }

        private ImageView CardViewIcon { get; set; }
        private TextView ConnectionTextView { get; set; }
        private FloatingActionButton FastForwardButton { get; set; }
        private FloatingActionButton FullscreenButton { get; set; }
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

        private TextView ProgressTextView { get; set; }
        private RelativeLayout RootLayout { get; set; }

        private Settings Settings { get; } = new Settings();
        private FloatingActionButton SkipButton { get; set; }
        private SeekBar TimeShiftSeekbar { get; set; }
        private TextView TimeShiftTextView { get; set; }
        private TextView TimeTextView { get; set; }
        private TextView TimeTextView2 { get; set; }
        private TextView TitleTextView { get; set; }
        private SeekBar VolumeSeekbar { get; set; }

        private bool CanProgressBeSet { get; set; } = true;

        private DirectoryManager DirectoryManager { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Globals.CurrentComputerModel = new ComputerModel("192.168.1.100", "88134165");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);

            Client = new VlcClient(Globals.CurrentComputerModel.Ip, 8080, Globals.CurrentComputerModel.Password);
            Client.ConnectionChanged += ClientOnConnectionChanged;
            Client.StatusUpdated += ClientOnStatusUpdated;

            DirectoryManager = new DirectoryManager(Client);

            TimeTextView = FindViewById<TextView>(Resource.Id.timeTextView);
            TimeTextView2 = FindViewById<TextView>(Resource.Id.timeTextView2);
            TimeShiftTextView = FindViewById<TextView>(Resource.Id.timeShiftText);
            TitleTextView = FindViewById<TextView>(Resource.Id.titleText);
            IpTextView = FindViewById<TextView>(Resource.Id.ipText);
            ProgressTextView = FindViewById<TextView>(Resource.Id.progressText);
            ConnectionTextView = FindViewById<TextView>(Resource.Id.connectionTextView);
            RootLayout = FindViewById<RelativeLayout>(Resource.Id.rootMainPageLayout);
            CardView = FindViewById<CardView>(Resource.Id.cardView);
            CardViewIcon = FindViewById<ImageView>(Resource.Id.cardViewIcon);
            SkipButton = FindViewById<FloatingActionButton>(Resource.Id.skipButton);
            FastForwardButton = FindViewById<FloatingActionButton>(Resource.Id.fastForwardButton);
            FullscreenButton = FindViewById<FloatingActionButton>(Resource.Id.fullscreenButton);
            PlayPauseButton = FindViewById<MorphButton>(Resource.Id.playPauseBtn);

            VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.volumeSeekBar);
            VolumeSeekbar.StopTrackingTouch += VolumeSeekbar_StopTrackingTouch;
            VolumeSeekbar.StartTrackingTouch += VolumeSeekbarOnStartTrackingTouch;
            VolumeSeekbar.ProgressChanged += VolumeSeekbar_ProgressChanged;

            TimeShiftSeekbar = FindViewById<SeekBar>(Resource.Id.timeShiftBar);
            TimeShiftSeekbar.StopTrackingTouch += TimeShiftSeekbar_StopTrackingTouch;
            TimeShiftSeekbar.ProgressChanged += TimeShiftSeekbar_ProgressChanged;

            Cheeseknife.Bind(this);

            IsEnabled = false;
        }

        private void VolumeSeekbarOnStartTrackingTouch(object sender,
            SeekBar.StartTrackingTouchEventArgs startTrackingTouchEventArgs)
        {
            CanProgressBeSet = false;
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
                    IpTextView.Text = Globals.CurrentComputerModel.Ip;
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
            Task.Factory.StartNew(() =>
            {
                var currentLeaf = GetCurrentLeaf().Result;
                if (currentLeaf != null)
                    RunOnUiThread(() => TimeTextView2.Text = TimeSpan.FromSeconds(currentLeaf.Duration).ToString());
                else

                    RunOnUiThread(() => TimeTextView2.SetText(Resource.String.app_unknown_duration));
            });

            RunOnUiThread(() =>
            {
                TimeTextView.Text = time.ToString();

                var value = (int) Math.Round((decimal) (125.0 * vlcStatus.Volume / 320.0));

                if (CanProgressBeSet)
                    VolumeSeekbar.SetProgress(value, true);

                string title;
                try
                {
                    title = Path.GetFileNameWithoutExtension(vlcStatus.Information.Category.First().Info
                        .First(i => i.Name == "filename").Text);
                }
                catch
                {
                    try
                    {
                        title = Path.GetFileNameWithoutExtension(vlcStatus.Information.Category.First().Info
                            .First(i => i.Name == "name").Text);
                    }
                    catch
                    {
                        title = "";
                    }
                }

                if (title != "")
                    TitleTextView.Text = title;
                else
                    TitleTextView.SetText(Resource.String.app_unknown_title);

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

        [OnClick(Resource.Id.fastForwardButton)]
        private async void FastForwardButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Client.Status.Rate > 1)
                await Client.SetRate(1);
            else
                await Client.SetRate(4);
        }

        [OnClick(Resource.Id.fullscreenButton)]
        private async void FullscreenButtonOnClick(object sender, EventArgs eventArgs)
        {
            await Client.ToggleFullscreen();
        }

        [OnClick(Resource.Id.playPauseBtn)]
        private async void PlayPauseButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Client.Status.State == VlcPlaybackState.Playing)
                await Client.Pause();
            else
                await Client.Play();
        }

        [OnClick(Resource.Id.skipButton)]
        private async void SkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            if ((await Client.GetPlaylist()).First().Leaves?.OrderBy(i => i.Name).Last()?.Current != null)
                await DirectoryManager.PlayNextEpisode();
            else
                await Client.Next();
        }

        private void TimeShiftSeekbar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            switch (e.SeekBar.Progress)
            {
                case 3: //-10 Seconds
                    TimeShiftTextView.Text = "-10 Segundos";
                    break;

                case 2: //-30 Seconds
                    TimeShiftTextView.Text = "-30 Segundos";
                    break;

                case 1: //-1:00 Seconds
                    TimeShiftTextView.Text = "-1 Minuto";
                    break;

                case 0: //-1:30 Seconds
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
                case 3: //-10 Seconds
                    await Client.SetRelativePosition(-10);
                    break;

                case 2: //-30 Seconds
                    await Client.SetRelativePosition(-30);
                    break;

                case 1: //-1:00 Seconds
                    await Client.SetRelativePosition(-60);
                    break;

                case 0: //-1:30 Seconds
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

        private void VolumeSeekbar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            ProgressTextView.Text = $"{e.Progress}%";
        }

        private async void VolumeSeekbar_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            CanProgressBeSet = true;
            await Client.SetPercentageVolume(VolumeSeekbar.Progress);
        }
    }
}