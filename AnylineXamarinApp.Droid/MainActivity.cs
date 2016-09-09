using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AnylineXamarinApp.Energy;

namespace AnylineXamarinApp
{
    [Activity(Label = "Anyline Xamarin Examples", MainLauncher = true, Icon = "@drawable/ic_launcher", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {

        //INSERT YOUR LICENSE KEY HERE
        public const string LicenseKey = "eyJzY29wZSI6WyJBTEwiXSwicGxhdGZvcm0iOlsiaU9TIiwiQW5kcm9pZCIsIldpbmRvd3MiXSwidmFsaWQiOiIyMDE3LTA1LTAzIiwibWFqb3JWZXJzaW9uIjoiMyIsImlzQ29tbWVyY2lhbCI6ZmFsc2UsInRvbGVyYW5jZURheXMiOjYwLCJpb3NJZGVudGlmaWVyIjpbIkFULk5pbmV5YXJkcy5BbnlsaW5lLkFueWxpbmVYYW1hcmluQXBwLkRyb2lkIl0sImFuZHJvaWRJZGVudGlmaWVyIjpbIkFULk5pbmV5YXJkcy5BbnlsaW5lLkFueWxpbmVYYW1hcmluQXBwLkRyb2lkIl0sIndpbmRvd3NJZGVudGlmaWVyIjpbIkFULk5pbmV5YXJkcy5BbnlsaW5lLkFueWxpbmVYYW1hcmluQXBwLkRyb2lkIl19Cmw3WXptWE1GZHNEWTJVWFFRWFdUVjgwUlh6T3kxN0RyV0hRbW1CYUJmNUw0NFpseFk2K2ZZRWdSQlBwS2xBUHlCRExPeUJqVkJvekxqeUhPendBN3dPcEYrTHBRTFhoeTg4WFBVanZINTR5aFVmRGloK0VLSnpFUUZab2tQUGFQTXZjdWlEQzB0a2M2bDRoVDJKWGY4YjVSOGw3VkpNWlVVOXVEU0xpcHczUVJaNVBBb0l1UGN5Y1E1eUVPeUZJTG84SkR0Mmt1T0p3UUxrNHdhNGk0OVN6ZVo2VTdjcEtSQSs1OW9qa3JndkNGNDBzNklKSURUY0p3eXA0UjBxeC9ZNzF1MGY1emZ2eXZxVDVOcHNqRnF2RlEvdFhGSEU5d3V0eGhoa2EzclBKUXUzQ3NKb3hvR1dPNURLRXBPelJnRzArUG1maDNxaGIxSzR0Sm9lMmlEdz09";
        
        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {

            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainActivity);

            ListView listView = FindViewById<ListView>(Resource.Id.listView);

            ActivityListAdapter listAdapter = new ActivityListAdapter(this);
            listView.Adapter = listAdapter;

            //adapt height of listView so it fits.
            Util.SetListViewHeightBasedOnChildren(listView);

            listView.ItemClick += (s,a) =>
            {
                if (listAdapter.GetItemViewType(a.Position) == ActivityListAdapter.TypeHeader)
                    return;
                
                //starts activity from the given class name in example_activities.xml
                var type = Type.GetType(listAdapter.ClassName(a.Position));

                if (type == null)
                    return;
                try
                {
                    var intent = new Intent(ApplicationContext, type);

                    // we generate the energy activity with different radiobuttons, depending on the use-case
                    // therefore we add which scan modes should be selectable
                    if (type == typeof(EnergyActivity))
                        intent.PutExtra("OBJECT", listAdapter.GetItem(a.Position).ToString());                        
                    
                    StartActivity(intent);
                }
                catch(Java.Lang.ClassNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }

            };
        }
                
        /// <summary>
        /// Kills the App when Back is pressed
        /// </summary>
        public override void OnBackPressed()
        {
            Finish();
            Process.KillProcess(Process.MyPid());
        }
    }
}

