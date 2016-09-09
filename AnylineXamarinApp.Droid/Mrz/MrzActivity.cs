using System;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using AT.Nineyards.Anyline.Models;
using AT.Nineyards.Anyline.Modules.Mrz;

namespace AnylineXamarinApp.Mrz
{
    [Activity(Label = "Scan MRZ of Passport or ID", MainLauncher = false, Icon = "@drawable/ic_launcher")]
    public class MrzActivity : Activity, IMrzResultListener, View.IOnClickListener
    {
        public static string TAG = typeof(MrzActivity).Name;
        
        private MrzResultView _resultView;
        private MrzScanView _scanView;

        protected override void OnCreate(Bundle bundle)
        {
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MrzActivity);

            _scanView = FindViewById<MrzScanView>(Resource.Id.mrz_scan_view);
            _resultView = FindViewById<MrzResultView>(Resource.Id.mrz_result);

            _resultView.SetOnClickListener(this);

            _scanView.SetConfigFromAsset("MrzConfig.json");

            _scanView.InitAnyline(MainActivity.LicenseKey, this);
                        
            _scanView.SetCancelOnResult(true);
            
            _scanView.CameraOpened += (s, e) => 
            {
                Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height);
                _resultView.Visibility = ViewStates.Invisible;
                _scanView.StartScanning();
            };

            _scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };
            
        }

        void IMrzResultListener.OnResult(Identification result, AnylineImage resultImage)
        {            
            _resultView.SetIdentification(result);
            _resultView.Visibility = ViewStates.Visible;
        }

        void View.IOnClickListener.OnClick(View v)
        {
            _resultView.Visibility = ViewStates.Invisible;
            _scanView.StartScanning();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _scanView.StartScanning();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _scanView.CancelScanning();
            _scanView.ReleaseCameraInBackground();
        }
        
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // explicitly free memory to avoid leaks
            GC.Collect(GC.MaxGeneration);
        }
    }

}