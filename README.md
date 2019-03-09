# Xamarin.Android.OpenCV
Orientation Fixed for the face detection.
Please follow below steps to fix the orientation
## 1. Use Screen Orientation as Reverse landscape in your activity
eg:- [Activity(ScreenOrientation = ScreenOrientation.ReverseLandscape)]
## 2. Then, OnCameraFrame() function set Core.Flip for the give frame to -1.
  Core.Core.Flip(mGray, mGray, -1);
