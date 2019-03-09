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
using Core = OpenCV.Core;
using OpenCV.ImgProc;
using Java.Lang;
using Android.Content.PM;
using Math = Java.Lang.Math;
using ArrayList1 = Java.Util.ArrayList;
using List1 = Java.Util.IList;

namespace OpenCV.SDKDemo.FaceDetect
{
    [Activity(Label = ActivityTags.FaceDetect,
        ScreenOrientation = ScreenOrientation.ReverseLandscape,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation,
        Theme = "@style/MyTheme")]

    public class FaceDetectActivity : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        private static readonly Scalar FACE_RECT_COLOR = new Scalar(0, 255, 0, 255);
        public static readonly int JAVA_DETECTOR = 0;
        public static readonly int NATIVE_DETECTOR = 1;

        private IMenuItem mItemFace50;
        private IMenuItem mItemFace40;
        private IMenuItem mItemFace30;
        private IMenuItem mItemFace20;
        private IMenuItem mItemType;

        private Mat mRgba;
        private Mat mGray;
        public File mCascadeFile { get; set; }
        public CascadeClassifier mJavaDetector { get; set; }
        public DetectionBasedTracker mNativeDetector { get; set; }

        private int mDetectorType = JAVA_DETECTOR;
        private string[] mDetectorName;

        private float mRelativeFaceSize = 0.2f;
        private int mAbsoluteFaceSize = 0;

        private CameraBridgeViewBase mOpenCvCameraView;

        private Callback mLoaderCallback;
        private Toolbar toolbar;

        public FaceDetectActivity()
        {
            mDetectorName = new string[2];
            mDetectorName[JAVA_DETECTOR] = "Java";
            mDetectorName[NATIVE_DETECTOR] = "Native (tracking)";

            Log.Info(ActivityTags.FaceDetect, "Instantiated new " + GetType().ToString());
        }

        /** Called when the activity is first created. */

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(ActivityTags.FaceDetect, "called onCreate");
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.face_detect_surface_view);


            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.fd_activity_surface_view);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, this, mOpenCvCameraView);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!OpenCVLoader.InitDebug())
            {
                Log.Debug(ActivityTags.FaceDetect, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, mLoaderCallback);
            }
            else
            {
                Log.Debug(ActivityTags.FaceDetect, "OpenCV library found inside package. Using it!");
                mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mOpenCvCameraView.DisableView();
        }

        public void OnCameraViewStarted(int width, int height)
        {
            mGray = new Mat(height, width, CvType.Cv8uc1);
            mRgba = new Mat(height, width, CvType.Cv8uc4);
            mIntermediateMat = new Mat(height, width, CvType.Cv8uc4);
            //mGray = new Mat(height, width, CvType.CV_8UC1);
            hierarchy = new Mat();
        }

        public void OnCameraViewStopped()
        {
            mGray.Release();
            mRgba.Release();
            mIntermediateMat.Release();
            hierarchy.Release();
        }


        IList<MatOfPoint> contours;
        private Mat mIntermediateMat;
        Mat hierarchy;
        MatOfPoint2f approxCurve;
        MatOfPoint2f contour2f;
        double approxDistance;
        double approx;
        MatOfPoint points;
        Rect rect;
        Mat lines;
        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {

            mRgba = inputFrame.Rgba();
            //Core.flip(mRgba, mRgba, 0);
            Core.Core.Flip(mRgba, mRgba, -1);
            mGray = inputFrame.Gray();
            Core.Core.Flip(mGray, mGray, -1);

            Size sizeRgba = mGray.Size();

            Mat rgbaInnerWindow;
            Mat tophat = mGray;
            Mat rectKern;

            int rows = (int)sizeRgba.Height;
            int cols = (int)sizeRgba.Width;

            //int left = cols / 8;
            //int top = rows / 8;

            int top = cols / 8;
            int left = rows / 8;

            int width = cols * 3 / 4;
            int height = rows * 3 / 4;
            contours = new JavaList<MatOfPoint>();


            /*Size kern =new Size(9,3);

            rectKern = Imgproc.GetStructuringElement(Imgproc.MorphRect, kern);

            Imgproc.MorphologyEx(tophat, mGray, Imgproc.MorphTophat, rectKern);*/
            lines = new Mat();
            Imgproc.HoughLines(mGray, lines, 1, Math.Pi / 180, 50);

            hierarchy = new Mat();
            rgbaInnerWindow = mGray.Submat(top, top + height, left, left + width);
            Imgproc.Canny(rgbaInnerWindow, mIntermediateMat, 100, 150, 7, false);
            Imgproc.Threshold(rgbaInnerWindow, mIntermediateMat, 127, 255, 0);
            Imgproc.FindContours(mIntermediateMat, contours, hierarchy, Imgproc.RetrList, Imgproc.ChainApproxSimple, new Point(top, left));
            hierarchy.Release();
            rgbaInnerWindow.Release();

            Imgproc.DrawContours(mRgba, contours, -1, new Scalar(0, 255, 0, 255), 2);//, 8, hierarchy, 0, new Point(0, 0));//, 2, 8, hierarchy, 0, new Point());
            

            //for (int contourIdx = 0; contourIdx < contours.Count; contourIdx++)
            //{
            //    // Minimum size allowed for consideration
            //   /* approxCurve = new MatOfPoint2f();
            //    contour2f = new MatOfPoint2f(contours[contourIdx].ToArray());
            //    //Processing on mMOP2f1 which is in type MatOfPoint2f
            //    approxDistance = Imgproc.ArcLength(contour2f, true) * 0.02;
            //    Imgproc.ApproxPolyDP(contour2f, approxCurve, approxDistance, true);

            //    //Convert back to MatOfPoint
            //    points = new MatOfPoint(approxCurve.ToArray());

            //    // Get bounding rect of contour
            //    rect = Imgproc.BoundingRect(points);

            //    //if((rect.Width * rect.Height) > 40000)
            //       // Imgproc.Rectangle(mRgba, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), new Scalar(255, 0, 0, 255), 3);

            //   */

            //}


            /*if (mAbsoluteFaceSize == 0)
            {
                int height = mGray.Rows();
                if (Java.Lang.Math.Round(height * mRelativeFaceSize) > 0)
                {
                    mAbsoluteFaceSize = Java.Lang.Math.Round(height * mRelativeFaceSize);
                }
                mNativeDetector.setMinFaceSize(mAbsoluteFaceSize);
            }

            MatOfRect faces = new MatOfRect();

            if (mDetectorType == JAVA_DETECTOR)
            {
                if (mJavaDetector != null)
                    mJavaDetector.DetectMultiScale(mGray, faces, 1.1, 2, 2, // TODO: objdetect.CV_HAAR_SCALE_IMAGE
                            new Size(mAbsoluteFaceSize, mAbsoluteFaceSize), new Size());
            }
            else if (mDetectorType == NATIVE_DETECTOR)
            {
                if (mNativeDetector != null)
                    mNativeDetector.detect(mGray, faces);
            }
            else
            {
                Log.Error(ActivityTags.FaceDetect, "Detection method is not selected!");
            }

            Rect[] facesArray = faces.ToArray();
            for (int i = 0; i < facesArray.Length; i++)
                Imgproc.Rectangle(mRgba, facesArray[i].Tl(), facesArray[i].Br(), FACE_RECT_COLOR, 3);
            */
            return mRgba;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            Log.Info(ActivityTags.FaceDetect, "called onCreateOptionsMenu");
            mItemFace50 = menu.Add("Face size 50%");
            mItemFace40 = menu.Add("Face size 40%");
            mItemFace30 = menu.Add("Face size 30%");
            mItemFace20 = menu.Add("Face size 20%");
            mItemType = menu.Add(mDetectorName[mDetectorType]);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Log.Info(ActivityTags.FaceDetect, "called onOptionsItemSelected; selected item: " + item);
            if (item == mItemFace50)
                setMinFaceSize(0.5f);
            else if (item == mItemFace40)
                setMinFaceSize(0.4f);
            else if (item == mItemFace30)
                setMinFaceSize(0.3f);
            else if (item == mItemFace20)
                setMinFaceSize(0.2f);
            else if (item == mItemType)
            {
                int tmpDetectorType = (mDetectorType + 1) % mDetectorName.Length;
                item.SetTitle(mDetectorName[tmpDetectorType]);
                setDetectorType(tmpDetectorType);
            }
            return true;
        }

        private void setMinFaceSize(float faceSize)
        {
            mRelativeFaceSize = faceSize;
            mAbsoluteFaceSize = 0;
        }

        private void setDetectorType(int type)
        {
            if (mDetectorType != type)
            {
                mDetectorType = type;

                if (type == NATIVE_DETECTOR)
                {
                    Log.Info(ActivityTags.FaceDetect, "Detection Based Tracker enabled");
                    mNativeDetector.start();
                }
                else
                {
                    Log.Info(ActivityTags.FaceDetect, "Cascade detector enabled");
                    mNativeDetector.stop();
                }
            }
        }
    }

    class Callback : BaseLoaderCallback
    {
        private readonly FaceDetectActivity _activity;
        private readonly CameraBridgeViewBase mOpenCvCameraView;
        public Callback(FaceDetectActivity activity, Context context, CameraBridgeViewBase view)
            : base(context)
        {
            _activity = activity;
            mOpenCvCameraView = view;
        }

        public override void OnManagerConnected(int status)
        {
            switch (status)
            {
                case LoaderCallbackInterface.Success:
                    {
                        Log.Info(ActivityTags.FaceDetect, "OpenCV loaded successfully");

                        // Load native library after(!) OpenCV initialization
                        JavaSystem.LoadLibrary("detection_based_tracker");

                        try
                        {
                            File cascadeDir;
                            // load cascade file from application resources
                            using (var istr = _activity.Resources.OpenRawResource(Resource.Raw.lbpcascade_frontalface))
                            {
                                cascadeDir = _activity.GetDir("cascade", FileCreationMode.Private);
                                _activity.mCascadeFile = new File(cascadeDir, "lbpcascade_frontalface.xml");

                                using (FileOutputStream os = new FileOutputStream(_activity.mCascadeFile))
                                {
                                    int byteRead;
                                    while ((byteRead = istr.ReadByte()) != -1)
                                    {
                                        os.Write(byteRead);
                                    }
                                }
                            }

                            _activity.mJavaDetector = new CascadeClassifier(_activity.mCascadeFile.AbsolutePath);
                            if (_activity.mJavaDetector.Empty())
                            {
                                Log.Error(ActivityTags.FaceDetect, "Failed to load cascade classifier");
                                _activity.mJavaDetector = null;
                            }
                            else
                                Log.Info(ActivityTags.FaceDetect, "Loaded cascade classifier from " + _activity.mCascadeFile.AbsolutePath);

                            _activity.mNativeDetector = new DetectionBasedTracker(_activity.mCascadeFile.AbsolutePath, 0);

                            cascadeDir.Delete();

                        }
                        catch (IOException e)
                        {
                            e.PrintStackTrace();
                            Log.Error(ActivityTags.FaceDetect, "Failed to load cascade. Exception thrown: " + e);
                        }

                        mOpenCvCameraView.EnableView();
                    }
                    break;
                default:
                    {
                        base.OnManagerConnected(status);
                    }
                    break;
            }
        }
    }
}