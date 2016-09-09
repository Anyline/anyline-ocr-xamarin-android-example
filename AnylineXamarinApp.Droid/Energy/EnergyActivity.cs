using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using AT.Nineyards.Anyline.Models;
using AT.Nineyards.Anyline.Modules.Energy;

namespace AnylineXamarinApp.Energy
{

    [Activity(Label = "Scan Energy Meters", MainLauncher = false, Icon = "@drawable/ic_launcher")]
    public class EnergyActivity : Activity, IEnergyResultListener, IDialogInterfaceOnClickListener, IDialogInterfaceOnCancelListener
    {
        public static string TAG = typeof(EnergyActivity).Name;
        
        private EnergyScanView _scanView;

        private string _energyUseCase; //analog, digital, heat meters..
        private Dictionary<string, EnergyScanView.ScanMode> _scanList;

        protected override void OnCreate(Bundle bundle)
        {
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EnergyActivity);

            _scanView = FindViewById<EnergyScanView>(Resource.Id.energy_scan_view);

            _energyUseCase = Intent.Extras.GetSerializable("OBJECT").ToString();

            _scanView.SetConfigFromAsset(_energyUseCase.Equals(Resources.GetString(Resource.String.scan_heat_meter))
                ? "EnergyConfigHeatMeter.json"
                : "EnergyConfigAll.json");

            _scanView.InitAnyline(MainActivity.LicenseKey, this);
            _scanView.SetScanMode(EnergyScanView.ScanMode.ElectricMeter);
            
            //in our main activity, we selected which type of meters we want to scan
            //now we want to populate the radiobutton group accordingly:

            RadioGroup radioGroup = FindViewById<RadioGroup>(Resource.Id.radio_group);
            int defaultIndex = 0; //index for which radiobutton is checked at the beginning

            if (_energyUseCase.Equals(Resources.GetString(Resource.String.scan_analog_electric_meter)))
            {
                _scanList = new Dictionary<string, EnergyScanView.ScanMode>
                {
                    { Resources.GetString(Resource.String.electric_meter), EnergyScanView.ScanMode.ElectricMeter },
                    { Resources.GetString(Resource.String.electric_meter_5), EnergyScanView.ScanMode.ElectricMeter51 },
                    { Resources.GetString(Resource.String.electric_meter_6), EnergyScanView.ScanMode.ElectricMeter61 },
                    { Resources.GetString(Resource.String.analog_meter_white), EnergyScanView.ScanMode.AnalogMeterWhite },
                    { Resources.GetString(Resource.String.analog_meter_7), EnergyScanView.ScanMode.AnalogMeter7 }
                    
                };

                _scanView.SetScanMode(_scanList.First().Value);
            }
            else if (_energyUseCase.Equals(Resources.GetString(Resource.String.scan_analog_gas_meter)))
            {
                _scanList = new Dictionary<string, EnergyScanView.ScanMode>
                {
                    { Resources.GetString(Resource.String.gas_4), EnergyScanView.ScanMode.AnalogMeter4 },
                    { Resources.GetString(Resource.String.gas_5), EnergyScanView.ScanMode.GasMeter },
                    { Resources.GetString(Resource.String.gas_6), EnergyScanView.ScanMode.GasMeter6 },
                    { Resources.GetString(Resource.String.gas_7), EnergyScanView.ScanMode.AnalogMeter7 }
                };

                defaultIndex = 1;
                _scanView.SetScanMode(_scanList.ElementAt(defaultIndex).Value);
            }
            else if (_energyUseCase.Equals(Resources.GetString(Resource.String.scan_analog_water_meter)))
            {
                _scanList = new Dictionary<string, EnergyScanView.ScanMode>
                {
                    { Resources.GetString(Resource.String.water_white), EnergyScanView.ScanMode.WaterMeterWhite },
                    { Resources.GetString(Resource.String.water_black), EnergyScanView.ScanMode.WaterMeterBlack }
                };

                _scanView.SetScanMode(_scanList.First().Value);
            }
            else if (_energyUseCase.Equals(Resources.GetString(Resource.String.scan_digital_meter)))
            {
                _scanView.SetScanMode(EnergyScanView.ScanMode.DigitalMeter);
            }
            else if (_energyUseCase.Equals(Resources.GetString(Resource.String.scan_heat_meter)))
            {
                _scanList = new Dictionary<string, EnergyScanView.ScanMode>
                {
                    { Resources.GetString(Resource.String.heat_meter_4_3), EnergyScanView.ScanMode.HeatMeter4 },
                    { Resources.GetString(Resource.String.heat_meter_5_3), EnergyScanView.ScanMode.HeatMeter5 },
                    { Resources.GetString(Resource.String.heat_meter_6_3), EnergyScanView.ScanMode.HeatMeter6 }
                };

                _scanView.SetScanMode(_scanList.First().Value);
            }

            Util.PopulateRadioGroupWithList(this, radioGroup, _scanList, defaultIndex);

            // switch the scan mode depending on user selection
            radioGroup.CheckedChange += (sender, args) =>
            {
                _scanView.CancelScanning();

                var rb = FindViewById<RadioButton>(args.CheckedId);
                var scanMode = _scanList.Single(x => x.Key.Equals(rb.Text)).Value;

                _scanView.SetScanMode(scanMode);
                _scanView.StartScanning();
            };

            _scanView.CameraOpened += (s, e) => { Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height); };
            _scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };
            
            _scanView.SetCancelOnResult(false);
        }

        void IEnergyResultListener.OnResult(EnergyScanView.ScanMode scanMode, string result, AnylineImage resultImage, AnylineImage fullSizedImage)
        {
            _scanView.CancelScanning();

            // explicitly free memory
            GC.Collect(GC.MaxGeneration);

            string typeString = scanMode.ToString();

            // for short results, we create a formatted result to visualize the numbers, but for longer results
            // we just want to display the plain text

            var formattedResult = result.Length < 7 ? GetFormattedResult(result) : new SpannableString(result);

            ResultDialogBuilder rdb = (ResultDialogBuilder) new ResultDialogBuilder(this)
                .SetResultImage(resultImage)
                .SetTextSize(ComplexUnitType.Dip, 32)
                .SetTextGravity(GravityFlags.Center)
                .SetText(formattedResult)
                .SetPositiveButton(Android.Resource.String.Ok, this)
                .SetTitle(typeString)
                .SetOnCancelListener(this);

            rdb.Show();
            
        }

        void IDialogInterfaceOnClickListener.OnClick(IDialogInterface dialog, int which) { _scanView.StartScanning(); }
        
        void IDialogInterfaceOnCancelListener.OnCancel(IDialogInterface dialog) { _scanView.StartScanning(); }

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

        private ISpannable GetFormattedResult(string result)
        {
            SpannableStringBuilder sb = new SpannableStringBuilder();

            for (int i = 0, n = result.Length; i < n; i++)
            {
                sb.Append(" ");
                sb.Append(result[i]);
                sb.Append(" ");
                sb.SetSpan(new BackgroundColorSpan(Color.Black), i * 4, i * 4 + 3, SpanTypes.ExclusiveExclusive);
                sb.SetSpan(new ForegroundColorSpan(Color.White), i * 4, i * 4 + 3, SpanTypes.ExclusiveExclusive);
                sb.Append(" ");
            }
            return sb;
        }
    }
}