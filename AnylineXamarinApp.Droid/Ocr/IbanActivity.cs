using System;
using Android.App;
using Android.Hardware;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AT.Nineyards.Anyline.Camera;
using AT.Nineyards.Anyline.Modules.Ocr;
using AT.Nineyards.Anyline.Util;
#pragma warning disable 618

namespace AnylineXamarinApp.Ocr
{
    [Activity(Label = "Scan IBAN", MainLauncher = false, Icon = "@drawable/ic_launcher")]
    public class IbanActivity : Activity, IAnylineOcrListener
    {
        public static string TAG = typeof(IbanActivity).Name;

        private AnylineOcrScanView _scanView;
        private OcrResultView _ibanResultView;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.OCRActivity);

            _scanView = FindViewById<AnylineOcrScanView>(Resource.Id.ocr_scan_view);

            //set up the iban result view
            InitIbanResultView();

            _scanView.SetConfigFromAsset("IbanConfig.json");
            
            _scanView.CopyTrainedData("tessdata/eng_no_dict.traineddata", "d142032d86da1be4dbe22dce2eec18d7");
            _scanView.CopyTrainedData("tessdata/deu.traineddata", "2d5190b9b62e28fa6d17b728ca195776");

            SetOcrConfig(_scanView);

            // set an individual focus configuration for this example
            _scanView.FocusConfig = new FocusConfig.Builder()
                .SetDefaultMode(Camera.Parameters.FocusModeAuto) // set default focus mode to be auto focus
                .SetAutoFocusInterval(8000) // set an interval of 8 seconds for auto focus
                .SetEnableFocusOnTouch(true) // enable focus on touch functionality
                .SetEnablePhaseAutoFocus(true)  // enable phase focus for faster focusing on new devices
                .SetEnableFocusAreas(true)  // enable focus areas to coincide with the cutout
                .Build();

            _scanView.SetUseMaxFpsRange(true);

            // set sports scene mode to try and bump up the fps count even more
            _scanView.SetSceneMode(Camera.Parameters.SceneModeSports);

            _scanView.InitAnyline(MainActivity.LicenseKey, this);

            _scanView.CameraOpened += (s, e) => { Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height); };
            _scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };

        }

        private void SetOcrConfig(AnylineOcrScanView scanView)
        {
            //Configure the OCR for IBANs
            AnylineOcrConfig anylineOcrConfig = new AnylineOcrConfig();

            // use the line mode (line length and font may vary)
            anylineOcrConfig.SetScanMode(AnylineOcrConfig.ScanMode.Line);

            // set the languages used for OCR
            anylineOcrConfig.SetTesseractLanguages("eng_no_dict", "deu");

            // allow only capital letters and numbers
            anylineOcrConfig.CharWhitelist = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            // set the height range the text can have
            anylineOcrConfig.MinCharHeight = 20; //note: if you set this too low, it tries to find all the small letters too
            anylineOcrConfig.MaxCharHeight = 60;

            // The minimum confidence required to return a result, a value between 0 and 100.
            // (higher confidence means less likely to get a wrong result, but may be slower to get a result)
            anylineOcrConfig.MinConfidence = 65;

            // a simple regex for a basic validation of the IBAN, results that don't match this, will not be returned
            // (full validation is more complex, as different countries have different formats)
            anylineOcrConfig.ValidationRegex = "^[A-Z]{2}([0-9A-Z]\\s*){13,32}$";

            // removes small contours (helpful in this case as no letters with small artifacts are allowed, like i���)
            anylineOcrConfig.RemoveSmallContours = true;

            // Experimental parameter to set the minimum sharpness (value between 0-100; 0 to turn sharpness detection off)
            // The goal of the minimum sharpness is to avoid a time consuming ocr step,
            // if the image is blurry and good results are therefore not likely.
            anylineOcrConfig.MinSharpness = 66;

            // set the ocr config
            scanView.SetAnylineOcrConfig(anylineOcrConfig);
        }

        private void InitIbanResultView()
        {
            RelativeLayout mainLayout = (RelativeLayout)FindViewById(Resource.Id.main_layout);
            
            _ibanResultView = new OcrResultView(this);
            
            _ibanResultView.Visibility = ViewStates.Invisible;

            _ibanResultView.Bg.SetImageResource(Resource.Drawable.iban_background);

            //register click event
            _ibanResultView.Click += (sender, args) =>
            {
                _ibanResultView.Visibility = ViewStates.Invisible;
                _scanView.StartScanning();                
            };

            //set text properties
            _ibanResultView.ResultText.TextAlignment = TextAlignment.TextStart;
            _ibanResultView.ResultText.TextSize = 18;
            _ibanResultView.ResultText.Typeface = Android.Graphics.Typeface.Default;
            _ibanResultView.ResultText.SetTextColor(Android.Graphics.Color.Black);

            //set text position
            ((RelativeLayout.LayoutParams)_ibanResultView.ResultText.LayoutParameters)
                .SetMargins(DimensUtil.GetPixFromDp(this, 30), DimensUtil.GetPixFromDp(this, 95), 0, 0);

            //center the result in the parent
            RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            layoutParams.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);

            mainLayout.AddView(_ibanResultView, layoutParams);

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

        void IAnylineOcrBaseListener.OnAbortRun(AnylineOcrError code, string message) { }

        void IAnylineOcrBaseListener.OnReport(string identifier, Java.Lang.Object value) { }

        //callback when a result is found
        void IAnylineOcrBaseListener.OnResult(AnylineOcrResult result)
        {            
            _ibanResultView.Visibility = ViewStates.Visible;
            _ibanResultView.ResultText.Text = result.Text.Trim();
        }

        bool IAnylineOcrListener.OnTextOutlineDetected(System.Collections.Generic.IList<Android.Graphics.PointF> shape) { return false; }

    }
}