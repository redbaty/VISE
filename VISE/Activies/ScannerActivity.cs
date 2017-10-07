using AFollestad.MaterialDialogs;
using Akavache;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Widget;
using Sof.Vlc.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VISE.Adapters;
using VISE.Models;

namespace VISE.Activies
{
    [Activity]
    public class ScannerActivity : AppCompatActivity
    {
        private FloatingActionButton AddComputerButton { get; set; }
        private VlcScanner Scanner { get; } = new VlcScanner();
        private List<ComputerModel> IpAddresses { get; set; } = new List<ComputerModel>();
        private ComputerAdapter ComputerAdapter { get; set; }
        private RecyclerView ComputerList { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.scan);
            var x = new LinearLayoutManager(this);
            IpAddresses.AddRange(Globals.ComputerModels);
            ComputerList = FindViewById<RecyclerView>(Resource.Id.computerList);
            ComputerAdapter = new ComputerAdapter(IpAddresses);
            ComputerList.SetAdapter(ComputerAdapter);
            ComputerList.SetLayoutManager(x);
            ComputerAdapter.ConnectClick += ComputerAdapterOnConnectClick;
            ComputerAdapter.RemoveClick += ComputerAdapterOnRemoveClick;
            ComputerAdapter.PinClick += ComputerAdapterOnPinClick;

            AddComputerButton = FindViewById<FloatingActionButton>(Resource.Id.addComputerButton);
            AddComputerButton.Click += AddComputerButtonOnClick;

            Scanner.VlcHostFound += ScannerOnVlcHostFound;
            Scanner.Start();
        }

        private void ComputerAdapterOnPinClick(object sender, int i1)
        {
            IpAddresses[i1].IsPinned = true;
            Globals.ComputerModels.Add(IpAddresses[i1]);
            Globals.SaveComputerModels();
        }

        private void ComputerAdapterOnRemoveClick(object sender, int i1)
        {
            IpAddresses.Remove(IpAddresses[i1]);
            ComputerAdapter.NotifyDataSetChanged();
        }

        private void ComputerAdapterOnConnectClick(object sender, int i1)
        {
            if (IpAddresses[i1].Password.Trim() == "")
            {
                new MaterialDialog.Builder(this)
                    .Title(Resource.String.app_authentication_string)
                    .Content(Resource.String.app_authentication_message_string)
                    .InputType(InputTypes.TextVariationPassword)
                    .InputRange(2, 16)
                    .PositiveText(Resource.String.app_addcomputer_connect)
                    .Input(Resource.String.app_addcomputer_dialog_password, 0, false,
                        (dialog, input) =>
                        {
                            IpAddresses[i1].Password = input;
                            Globals.CurrentComputerModel = IpAddresses[i1];
                            if (IpAddresses[i1].IsPinned)
                            {
                                Globals.ComputerModels.First(i => i == IpAddresses[i1]).Password = input;
                                Globals.SaveComputerModels();
                            }

                            StartActivity(typeof(MainActivity));
                        }).Show();
            }
            else
            {
                Globals.CurrentComputerModel = IpAddresses[i1];
                StartActivity(typeof(MainActivity));
            }
        }

        protected override void OnStop()
        {
            Scanner.Stop();
            base.OnStop();
        }

        protected override void OnResume()
        {
            Scanner.Start();
            base.OnResume();
        }

        protected override void OnPause()
        {
            Scanner.Stop();
            base.OnPause();
        }

        private void AddComputerButtonOnClick(object sender, EventArgs eventArgs)
        {
            var dialogBuilder = new MaterialDialog.Builder(this)
                .Title(Resource.String.app_addcomputer_title)
                .CustomView(Resource.Layout.dialog_addcomputer, true)
                .PositiveText(Resource.String.app_addcomputer_connect)
                .NegativeText(Resource.String.app_addcomputer_cancel)
                .OnPositive((dialog, w) =>
                {
                    IpAddresses.Add(new ComputerModel(
                        dialog.CustomView.FindViewById<EditText>(Resource.Id.ipAddress).Text,
                        dialog.CustomView.FindViewById<EditText>(Resource.Id.password).Text));
                    BlobCache.UserAccount.InsertObject("ipaddresses", IpAddresses);
                }).Build();

            var positiveAction = dialogBuilder.GetActionButton(DialogAction.Positive);
            var ipControl = dialogBuilder.CustomView.FindViewById<EditText>(Resource.Id.ipAddress);
            var token = new CancellationTokenSource();

            positiveAction.Enabled = false;

            ipControl.TextChanged += (o, args) =>
            {
                token.Cancel();
                token = new CancellationTokenSource();
                if (ValidateIPv4(ipControl.Text) && IpAddresses.All(i => i.Ip != ipControl.Text))
                    Task.Factory.StartNew(
                        async () => positiveAction.Enabled = await Scanner.CheckHostForVlc(ipControl.Text),
                        token.Token);
                else
                    positiveAction.Enabled = false;
            };

            dialogBuilder.Show();
        }

        private void ScannerOnVlcHostFound(object sender, string s)
        {
            if (ComputerAdapter.Computers.Any(i => i.Ip == s)) return;

            IpAddresses.Add(new ComputerModel(s));
            RunOnUiThread(() => ComputerAdapter.NotifyDataSetChanged());
        }

        public bool ValidateIPv4(string ipString)
        {
            if (string.IsNullOrWhiteSpace(ipString))
                return false;

            var splitValues = ipString.Split('.');
            return splitValues.Length == 4 && splitValues.All(r => byte.TryParse(r, out _));
        }
    }
}