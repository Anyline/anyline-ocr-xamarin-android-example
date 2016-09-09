using System;
using Android.App;
using Android.Hardware;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AT.Nineyards.Anyline.Camera;
using AT.Nineyards.Anyline.Modules.Ocr;
#pragma warning disable 618

namespace AnylineXamarinApp.Ocr
{
    [Activity(Label = "Scan License Plates (Alpha)", MainLauncher = false, Icon = "@drawable/ic_launcher")]
    public class LicensePlateActivity : Activity, IAnylineOcrListener
    {
        public static string TAG = typeof(LicensePlateActivity).Name;

        protected AnylineOcrScanView scanView;
        private OcrResultView _licensePlateResultView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.OCRActivity);

            InitLicensePlateResultView();

            scanView = FindViewById<AnylineOcrScanView>(Resource.Id.ocr_scan_view);

            scanView.SetConfigFromAsset("LicensePlateConfig.json");

            /*scanView.CopyTrainedData("tessdata/GL-Nummernschild-Mtl7_uml.traineddata", "8ea050e8f22ba7471df7e18c310430d8");
            scanView.CopyTrainedData("tessdata/Arial.traineddata", "9a5555eb6ac51c83cbb76d238028c485");
            scanView.CopyTrainedData("tessdata/Alte.traineddata", "f52e3822cdd5423758ba19ed75b0cc32");*/

            scanView.CopyTrainedData("tessdata/GL-Nummernschild-Mtl7_uml.traineddata",
                "8ea050e8f22ba7471df7e18c310430d8");
            scanView.CopyTrainedData("tessdata/Arial.traineddata", "9a5555eb6ac51c83cbb76d238028c485");
            scanView.CopyTrainedData("tessdata/Alte.traineddata", "f52e3822cdd5423758ba19ed75b0cc32");
            scanView.CopyTrainedData("tessdata/deu.traineddata", "2d5190b9b62e28fa6d17b728ca195776");

            SetOcrConfig(scanView);

            scanView.InitAnyline(MainActivity.LicenseKey, this);

            scanView.CameraOpened += (s, e) => { Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height); };
            scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };

        }

        private static void SetOcrConfig(AnylineOcrScanView scanView)
        {
            //Configure the OCR for License Plates
            AnylineOcrConfig anylineOcrConfig = new AnylineOcrConfig();

            anylineOcrConfig.CustomCmdFile = "license_plates.ale";
            
            scanView.SetAnylineOcrConfig(anylineOcrConfig);
        }

        private void InitLicensePlateResultView()
        {
            RelativeLayout mainLayout = (RelativeLayout)FindViewById(Resource.Id.main_layout);

            _licensePlateResultView = new OcrResultView(this)
            {
                Visibility = ViewStates.Invisible
            };

            _licensePlateResultView.Bg.SetImageResource(Resource.Drawable.license_plate_background);

            //register click event
            _licensePlateResultView.Click += (sender, args) =>
            {
                _licensePlateResultView.Visibility = ViewStates.Invisible;
                scanView.StartScanning();
            };

            //set text properties
            _licensePlateResultView.ResultText.TextAlignment = TextAlignment.Center;
            _licensePlateResultView.ResultText.TextSize = 36;
            _licensePlateResultView.ResultText.Typeface = Android.Graphics.Typeface.DefaultBold;
            _licensePlateResultView.ResultText.SetTextColor(Android.Graphics.Color.Black);

            var textParams = (RelativeLayout.LayoutParams)_licensePlateResultView.ResultText.LayoutParameters;

            //center text
            textParams.AddRule(LayoutRules.CenterHorizontal, (int)LayoutRules.True);
            textParams.AddRule(LayoutRules.CenterVertical, (int)LayoutRules.True);

            _licensePlateResultView.ResultText.LayoutParameters = textParams;

            //center the result in the parent
            RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            layoutParams.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);

            mainLayout.AddView(_licensePlateResultView, layoutParams);
        }

        protected override void OnResume()
        {
            base.OnResume();
            scanView.StartScanning();
        }

        protected override void OnPause()
        {
            base.OnPause();
            scanView.CancelScanning();
            scanView.ReleaseCameraInBackground();
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

        void IAnylineOcrBaseListener.OnAbortRun(AnylineOcrError code, string message) { }

        void IAnylineOcrBaseListener.OnReport(string identifier, Java.Lang.Object value)
        {

        }

        //callback when a result is found
        void IAnylineOcrBaseListener.OnResult(AnylineOcrResult result)
        {
            _licensePlateResultView.Visibility = ViewStates.Visible;
            _licensePlateResultView.ResultText.Text = result.Text.Split('-')[1];
        }

        bool IAnylineOcrListener.OnTextOutlineDetected(System.Collections.Generic.IList<Android.Graphics.PointF> shape) { return false; }

    }
}