##  AnylineSDK for Xamarin.Android  ##

- AnylineXamarinSDK.Droid.dll: contains the Anyline.framework & AnylineResources.bundle
- AnylineXamarinApp.Droid:  contains a simple app where each module is implemented, it can be installed right away
- LICENSE_ANDROID.md: contains third party license agreements
- README_ANDROID.md: this readme

For detailed information and guides, please visit https://documentation.anyline.io/

### Requirements

- Android device with SDK >= 15
- Decent camera functionality (recommended: 720p and adequate auto focus)
- Xamarin account
- Visual Studio / Xamarin Studio


### Quick Start - Setup


##### 1. Add AnylineXamarinSDK.Droid.dll to your References


##### 2. Add necessary permissions and features to AndroidManifest.xml and set API Level >= 15

- Permissions
	- CAMERA
	- VIBRATE
	- WRITE_EXTERNAL_STORAGE
	- READ_EXTERNAL_STORAGE
- Features
	- android.hardware.camera
	- android.hardware.camera.flash
	- android.hardware.camera.autofocus

##### 3. Add a config file to your "Assets" folder or set configuration in XML

###### .json file option:

> Example barcode_view_config.json:

```json
{
	"captureResolution":"720p",
  	"cutout": {
		"style": "rect",
		"maxWidthPercent": "80%",
		"alignment": "center",
		"ratioFromSize": {
	     	"width": 100,
	     	"height": 80
		},
		"strokeWidth": 2,
		"cornerRadius": 4,
		"strokeColor": "FFFFFF",
		"outerColor": "000000",
		"outerAlpha": 0.3
	 },
	 "flash": {
		"mode": "manual",
	 	"alignment": "bottom_right"
	 },
	 "beepOnResult": true,
	 "vibrateOnResult": true,
	 "blinkAnimationOnResult": true,
	 "cancelOnResult": true
}
```

###### XML config option:

```xml
<RelativeLayout
	 xmlns:android="http://schemas.android.com/apk/res/android"
	 xmlns:app="http://schemas.android.com/apk/res-auto"
	 android:layout_width="match_parent"
	 android:layout_height="match_parent">
	<at.nineyards.anyline.modules.energy.EnergyScanView
	    android:id="@+id/energy_scan_view"
	    android:layout_width="match_parent"
	    android:layout_height="match_parent"
	    app:cutout_alignment="top"
	    app:cutout_style="rect"
	    app:cutout_outside_color="#55000000"
	    app:cutout_offset_y="120"
	    app:cutout_rect_corner_radius_in_dp="4"
	    app:cutout_stroke_width_in_dp="2"
	    app:cutout_stroke_color="#FFFFFF"
	    app:flash_mode="manual"
	    app:flash_alignment="bottom_right"
	    app:beep_on_result="true"
	    app:vibrate_on_result="true"
	    app:blink_animation_on_result="true"
	    app:cancel_on_result="true"
	 />
</RelativeLayout>
```

##### 4. Init an Anyline scan view in your Activity
There are module specific options - take a look at the description in the desired module description below.


##### 5. Enjoy scanning and have fun :)


### Modules

### Barcode Module

With the Anyline-Barcode-Module any kind of bar- and qr-codes can be scanned.
The result will be simply a String representation of the code.


Init Anyline:
- Set configuration if you use a .json file
- Init a BarcodeScanView with your license key and a module-specific result listener
- Optional you may also limit the barcode scanning to one or multiple barcode formats (see also 'Available Barcode Formats' below)
- Call StartScanning() at OnResume() or later.
- Implement the interface IBarcodeResultListener and handle results

Example Code:

    protected override void OnCreate(Bundle bundle)
    {

        base.OnCreate(bundle);
        SetContentView(Resource.Layout.Barcode);

        scanView = FindViewById<BarcodeScanView>(Resource.Id.barcode_scan_view);

        scanView.SetConfigFromAsset("BarcodeConfig.json");

        scanView.InitAnyline(licenseKey, this);

        // for example, limit the barcode scanner to QR codes or CODE_128 codes
        scanView.SetBarcodeFormats(BarcodeScanView.BarcodeFormat.QR_CODE, BarcodeScanView.BarcodeFormat.CODE_128);

        scanView.CameraOpened += (s, e) => { Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height); };
        scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };
    }

    public void OnResult(string result, BarcodeScanView.BarcodeFormat barcodeFormat, AnylineImage anylineImage)
    {
        // handle result here
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

Available Barcode Formats:
        AZTEC
        CODABAR
        CODE_39
        CODE_93
        CODE_128
        DATA_MATRIX
        EAN_8
        EAN_13
        ITF
        PDF_417
        QR_CODE
        RSS_14
        RSS_EXPANDED
        UPC_A
        UPC_E
        UPC_EAN_EXTENSION




### MRZ Module

The Anyline-MRZ-Module provides the functionality to scan passports and other IDs with a MRZ (machine-readable-zone).
For each scan result the module generates an Identification Object, containing all relevant information
(e.g. document type and number, name, day of birth, etc.) as well as the image of the scanned document.


Init Anyline:
- Set configuration if you use a .json file
- Init a MrzScanView with your license key and a module-specific result listener
- Call StartScanning() at OnResume() or later.
- Implement the interface IMrzResultListener and handle results

Example Code:

	protected override void OnCreate(Bundle bundle)
    {
		base.OnCreate(bundle);
        SetContentView(Resource.Layout.Mrz);

        scanView = FindViewById<MrzScanView>(Resource.Id.mrz_scan_view);

        scanView.SetConfigFromAsset("MrzConfig.json");

        scanView.InitAnyline(licenseKey, this);

        scanView.CameraOpened += (s, e) => { Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height); };
        scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };
    }

    void OnResult(Identification result, AnylineImage resultImage)
    {
        // handle result here
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

### Energy Module

The Anyline-Energy-Module is capable of scanning analog electric- and gas-meter-readings.
Moreover, it is possible to scan bar- and qr-codes for meter identification.

For each successful scan, you will receive four result-attributes (images will be null for bar/qr code mode):
    ScanMode: the mode the result belongs to
    result (for meter reading): the detected value as a String
    resultImage (for meter reading): the cropped image that has been used to scan the meter value
    fullImage (for meter reading): the full image (before cropping)

Init Anyline:
- Set configuration if you use a .json file
- Init a EnergyScanView with your license key and a module-specific result listener
- Call StartScanning() at OnResume() or later.
- Implement the interface IEnergyResultListener and handle results

Example Code:

    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);
        SetContentView(Resource.Layout.Energy);

        scanView = FindViewById<EnergyScanView>(Resource.Id.energy_scan_view);
        scanView.SetConfigFromAsset("EnergyConfig.json");

        scanView.InitAnyline(licenseKey, this);

        scanView.SetScanMode(EnergyScanView.ScanMode.ElectricMeter);

        scanView.CameraOpened += (s, e) => { Log.Debug(TAG, "Camera opened successfully. Frame resolution " + e.Width + " x " + e.Height); };
        scanView.CameraError += (s, e) => { Log.Error(TAG, "OnCameraError: " + e.Event.Message); };
    }

    void IEnergyResultListener.OnResult(EnergyScanView.ScanMode scanMode, string result, AnylineImage resultImage, AnylineImage fullSizedImage)
    {
        // handle result here
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