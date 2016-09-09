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
    [Activity(Label = "Scan German License Plates (Alpha)", MainLauncher = false, Icon = "@drawable/ic_launcher")]
    public class LicensePlateActivityGermany : LicensePlateActivity, IAnylineOcrListener
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //Configure the OCR for License Plates
            AnylineOcrConfig anylineOcrConfig = new AnylineOcrConfig();

            anylineOcrConfig.CustomCmdFile = "license_plates_d.ale";

            scanView.SetAnylineOcrConfig(anylineOcrConfig);
        }
    }
}