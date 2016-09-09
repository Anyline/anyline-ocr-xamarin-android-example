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
    [Activity(Label = "Scan Austrian License Plates (Alpha)", MainLauncher = false, Icon = "@drawable/ic_launcher")]
    public class LicensePlateActivityAustria : LicensePlateActivity, IAnylineOcrListener
    {        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //Configure the OCR for License Plates
            AnylineOcrConfig anylineOcrConfig = new AnylineOcrConfig();

            anylineOcrConfig.CustomCmdFile = "license_plates_a.ale";

            scanView.SetAnylineOcrConfig(anylineOcrConfig);
        }
    }
}