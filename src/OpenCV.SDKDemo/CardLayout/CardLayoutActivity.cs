using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenCV.SDKDemo.Utilities;
using OpenCV.Core;
using OpenCV.ObjDetect;
using OpenCV.Android;
using Java.IO;
using OpenCV.SDKDemo.ColorBlobDetection;
using Android.Util;
using Size = OpenCV.Core.Size;
using OpenCV.ImgProc;
using Java.Lang;
using OpenCV.SDKDemo.FaceDetect;
using Android.Content.PM;
using OpenCV;
using Android.Graphics;

namespace OpenCV.SDKDemo.CardLayout
{
    [Activity(Label = "CardLayoutActivity",
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation
        //,Theme="@android:style/Theme.NoTitleBar.FullScreen"
        )]
    public class CardLayoutActivity : Activity, ILoaderCallbackInterface, CameraBridgeViewBase.ICvCameraViewListener
    {
        private CameraBridgeViewBase _openCvCameraView;
        Mat mGray;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.CardLayout);
            _openCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.surface_view);
            _openCvCameraView.Visibility = ViewStates.Visible;
            _openCvCameraView.SetCvCameraViewListener(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_openCvCameraView != null)
            {
                _openCvCameraView.DisableView();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!OpenCVLoader.InitDebug())
            {
                Log.Debug(ActivityTags.CameraPreview, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, this);
            }
            else
            {
                Log.Debug(ActivityTags.CameraPreview, "OpenCV library found inside package. Using it!");
                OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_openCvCameraView != null)
            {
                _openCvCameraView.DisableView();
            }
        }

        public void OnManagerConnected(int p0)
        {
            switch (p0)
            {
                case LoaderCallbackInterface.Success:
                    Log.Info(ActivityTags.CameraPreview, "OpenCV loaded successfully");
                    _openCvCameraView.EnableView();
                    break;
                default:
                    break;
            }
        }

        public void OnPackageInstall(int p0, IInstallCallbackInterface p1)
        {

        }

        public void OnCameraViewStarted(int p0, int p1)
        {

        }

        public void OnCameraViewStopped()
        {

        }

        public Mat OnCameraFrame(Mat p0)
        {
            //mGray = inputFrame.Gray();

            //mGray = Po.p0();
            /*Point firstPoint = new Point(100, 200);
            Point secondPoint = new Point(100, 400);
            Point middlePoint = new Point(firstPoint.x,
                    firstPoint.y + 0.5 * (secondPoint.y - firstPoint.y));

            Scalar lineColor = new Scalar(255, 0, 0, 255);
            int lineWidth = 3;

            Scalar textColor = new Scalar(255, 0, 0, 255);

            Imgproc.line(mGray, firstPoint, secondPoint, lineColor, lineWidth);
            Imgproc.putText(mGray, " Text", middlePoint,
                    Core.FONT_HERSHEY_PLAIN, 1.5, textColor);*/
            return p0;
        }
    }
}