using Akavache;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VISE.Models;

namespace VISE.Activies
{
    [Activity(MainLauncher = true)]
    public class SplashScreen : AppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    Globals.ComputerModels =
                        await BlobCache.UserAccount.GetObject<List<ComputerModel>>(Globals.Keys.Fixedmodels);
                }
                catch
                {
                }

                Globals.CurrentComputerModel = new ComputerModel("192.168.1.100", "88134165");
                StartActivity(typeof(MainActivity));
            });
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.splashscreen);
            BlobCache.ApplicationName = "VISE";
        }
    }
}