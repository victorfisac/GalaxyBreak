//  ---------------------------------------------------------------------------
//  AdColony.cs
//
//  AdColony Plugin for Unity.
//
//  Copyright © 2015 AdColony, Inc.  All rights reserved.
//
//  ---------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public enum IAPEngagementType {NONE, OVERLAY, END_CARD, AUTOMATIC}

public class AdColonyAd {
  public bool shown;
  public bool iapEnabled;
  public string productID;
  public IAPEngagementType iapEngagementType;

  public string toString() {
    return "AdColonyAdInfo- Shown:" + shown + ", IAPEnabled: " + iapEnabled + ", productID:" + productID + ", IAPEngagementType: " + iapEngagementType;
  }

  public AdColonyAd(string[] split_args) {
    this.shown = split_args[0].Equals("true");
    this.iapEnabled = split_args[1].Equals("true");
    string type = split_args[2];

    if(type == "END_CARD") {
      this.iapEngagementType = IAPEngagementType.END_CARD;
    } else if(type == "OVERLAY") {
      this.iapEngagementType = IAPEngagementType.OVERLAY;
    } else if(type == "AUTOMATIC") {
      this.iapEngagementType = IAPEngagementType.AUTOMATIC;
    } else {
      this.iapEngagementType = IAPEngagementType.NONE;
    }

    this.productID = split_args[3];
  }
}


public class AdColony : MonoBehaviour
{
  private static AdColony instance;
  public static string version = "2.1.4.1";

  //We need to make sure we have an instance so that we have an object to send messages to.
  private static void ensureInstance() {
    if(instance == null) {
      instance = FindObjectOfType(typeof(AdColony)) as AdColony;
      if(instance==null) {
        instance = new GameObject("AdColony").AddComponent<AdColony>();
      }
    }
  }


  // DELEGATE TYPE SPECIFICATIONS
  // Your class can define methods matching these signatures and assign them
  // to the 'OnVideoStarted', 'OnVideoFinished', and and 'OnV4VCResult'
  // delegates.
  public delegate void VideoStartedDelegate();
  public delegate void VideoFinishedDelegate( bool ad_shown );
  public delegate void VideoFinishedWithInfoDelegate( AdColonyAd ad_shown );
  public delegate void V4VCResultDelegate( bool success, string name, int amount );
  public delegate void AdAvailabilityChangeDelegate( bool available, string zone_id );

  // DELEGATE PROPERTIES
  public static VideoStartedDelegate          OnVideoStarted;
  public static VideoFinishedDelegate         OnVideoFinished;
  public static VideoFinishedWithInfoDelegate OnVideoFinishedWithInfo;
  public static V4VCResultDelegate            OnV4VCResult;
  public static AdAvailabilityChangeDelegate  OnAdAvailabilityChange;

  static bool configured;

  void Awake() {
    // Set the name to allow UnitySendMessage to find this object.
    name = "AdColony";
    // Make sure this GameObject persists across scenes
    DontDestroyOnLoad(transform.gameObject);
  }

  public void OnAdColonyVideoStarted( string args ) {
    if (OnVideoStarted != null) {
      OnVideoStarted();
    }
  }

  public void OnAdColonyVideoFinished( string args ) {
    //ad_shown | iapenabled | engagementType | iapproductid
    string[] split_args = args.Split('|');

    if (OnVideoFinished != null) {
      OnVideoFinished( split_args[0].Equals("true") );
    }
    if (OnVideoFinishedWithInfo != null) {
      OnVideoFinishedWithInfo(new AdColonyAd(split_args));
    }
  }

  public void OnAdColonyV4VCResult( string args ) {
    if (OnV4VCResult != null) {
      //success | amount | name
      string[] split_args = args.Split('|');
      bool success = split_args[0].Equals("true");
      int amount = int.Parse(split_args[1]);
      string name = split_args[2];
      OnV4VCResult(success, name, amount);
    }
  }

  public void OnAdColonyAdAvailabilityChange( string args ) {
    if (OnAdAvailabilityChange != null) {
      //available | zone
      string[] split_args = args.Split('|');
      OnAdAvailabilityChange( split_args[0].Equals("true"), split_args[1] );
    }
  }

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IPHONE)
  static public void Configure( string app_version, string app_id, params string[] zone_ids )
  {
    if (!configured) {
      Debug.LogWarning( "AdColony has been stubbed out." );
      configured = true;
    }
    ensureInstance();
  }

  static public void   SetCustomID( string custom_id ) { }
  static public string GetCustomID() { return "undefined"; }
  static public bool   IsVideoAvailable() { return false; }
  static public bool   IsVideoAvailable( string zone_id ) { return false; }
  static public bool   IsV4VCAvailable() { return false; }
  static public bool   IsV4VCAvailable( string zone_id ) { return false; }
  static public string GetDeviceID() { return "undefined"; }
  static public string GetOpenUDID() { return "undefined"; }
  static public string GetODIN1() { return "undefined"; }
  static public int    GetV4VCAmount() { return 0; }
  static public int    GetV4VCAmount( string zone_id ) { return 0; }
  static public string GetV4VCName() { return "undefined"; }
  static public string GetV4VCName( string zone_id ) { return "undefined"; }
  static public bool   ShowVideoAd() { return false; }
  static public bool   ShowVideoAd( string zone_id ) { return false; }
  static public bool   ShowV4VC( bool popup_result ) { return false; }
  static public bool   ShowV4VC( bool popup_result, string zone_id ) { return false; }
  static public void   OfferV4VC( bool popup_result ) { }
  static public void   OfferV4VC( bool popup_result, string zone_id ) { }
  static public string StatusForZone( string zone_id ) { return "undefined"; }
  static public void NotifyIAPComplete( string product_id, string trans_id, string currency_code, double price, int quantity) { }

#elif UNITY_IPHONE && !UNITY_EDITOR
  static public void Configure( string app_version, string app_id, params string[] zone_ids ) {
    if (configured) {
      return;
    }
    ensureInstance();

    if (app_version.Contains("version:")) {
      string[] delims = new string[] {"version:", ","};
      string[] app_version_split = app_version.Split(delims, StringSplitOptions.RemoveEmptyEntries);
      app_version = app_version_split[0];
    }

    IOSConfigure( app_version, app_id, zone_ids.Length, zone_ids );
    configured = true;
  }

  [DllImport ("__Internal")]
    extern static public void SetCustomID( string custom_id );
  [DllImport ("__Internal")]
    extern static public string GetCustomID();
  [DllImport ("__Internal")]
    extern static public void IOSConfigure( string app_version, string app_id,
        int zone_id_count, string[] zone_ids );
  [DllImport ("__Internal")]
    extern static public bool IsVideoAvailable( string zone_id );
  [DllImport ("__Internal")]
    extern static public bool IsV4VCAvailable( string zone_id );
  [DllImport ("__Internal")]
    extern static public string GetOpenUDID();
  [DllImport ("__Internal")]
    extern static public string GetDeviceID();
  [DllImport ("__Internal")]
    extern static public string GetODIN1();
  [DllImport ("__Internal")]
    extern static public int GetV4VCAmount( string zone_id );
  [DllImport ("__Internal")]
    extern static public string GetV4VCName( string zone_id );
  [DllImport ("__Internal")]
    extern static public bool ShowVideoAd( string zone_id );
  [DllImport ("__Internal")]
    extern static public bool ShowV4VC( bool popup_result, string zone_id );
  [DllImport ("__Internal")]
    extern static public void OfferV4VC( bool popup_result, string zone_id );
  [DllImport ("__Internal")]
    extern static public string StatusForZone( string zone_id );
  [DllImport ("__Internal")]
    extern static public void NotifyIAPComplete(string product_id, string trans_id, string currency_code, double price, int quantity );
  [DllImport ("__Internal")]
    extern static public void SetOption(string option, bool val);

#elif UNITY_ANDROID && !UNITY_EDITOR
  static public void Configure( string app_version, string app_id, params string[] zone_ids ) {
    if (configured) {
      return;
    }
    ensureInstance();

    AndroidConfigure( app_version, app_id, zone_ids );
  }

  static bool adr_initialized = false;
  static AndroidJavaClass class_UnityPlayer;
  static IntPtr class_UnityADC           = IntPtr.Zero;
  static IntPtr method_configure         = IntPtr.Zero;
  static IntPtr method_setCustomID       = IntPtr.Zero;
  static IntPtr method_getCustomID       = IntPtr.Zero;
  static IntPtr method_isVideoAvailable  = IntPtr.Zero;
  static IntPtr method_isV4VCAvailable   = IntPtr.Zero;
  static IntPtr method_getDeviceID       = IntPtr.Zero;
  static IntPtr method_getV4VCAmount     = IntPtr.Zero;
  static IntPtr method_getV4VCName       = IntPtr.Zero;
  static IntPtr method_showVideo         = IntPtr.Zero;
  static IntPtr method_showV4VC          = IntPtr.Zero;
  static IntPtr method_offerV4VC         = IntPtr.Zero;
  static IntPtr method_statusForZone     = IntPtr.Zero;
  static IntPtr method_getAvailableViews = IntPtr.Zero;
  static IntPtr method_notifyIAPComplete = IntPtr.Zero;

  static void AndroidInitializePlugin() {
    bool success = true;
    IntPtr local_class_UnityADC = AndroidJNI.FindClass("com/jirbo/unityadc/UnityADC");
    if (local_class_UnityADC != IntPtr.Zero) {
      class_UnityADC = AndroidJNI.NewGlobalRef( local_class_UnityADC );
      AndroidJNI.DeleteLocalRef( local_class_UnityADC );
      var local_class_AdColony = AndroidJNI.FindClass("com/jirbo/adcolony/AdColony");
      if (local_class_AdColony != IntPtr.Zero) {
        AndroidJNI.DeleteLocalRef( local_class_AdColony );
      } else {
        success = false;
      }
    } else {
      success = false;
    }

    if (success) {

      class_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      // Get additional method IDs for later use.
      method_configure = AndroidJNI.GetStaticMethodID( class_UnityADC, "configure",
          "(Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;)V" );

      method_setCustomID = AndroidJNI.GetStaticMethodID( class_UnityADC, "setCustomID", "(Ljava/lang/String;)V" );
      method_getCustomID = AndroidJNI.GetStaticMethodID( class_UnityADC, "getCustomID", "()Ljava/lang/String;" );
      method_isVideoAvailable = AndroidJNI.GetStaticMethodID( class_UnityADC, "isVideoAvailable", "(Ljava/lang/String;)Z" );
      method_isV4VCAvailable = AndroidJNI.GetStaticMethodID( class_UnityADC, "isV4VCAvailable", "(Ljava/lang/String;)Z" );
      method_getDeviceID = AndroidJNI.GetStaticMethodID( class_UnityADC, "getDeviceID", "()Ljava/lang/String;" );
      method_getV4VCAmount = AndroidJNI.GetStaticMethodID( class_UnityADC, "getV4VCAmount", "(Ljava/lang/String;)I" );
      method_getV4VCName = AndroidJNI.GetStaticMethodID( class_UnityADC, "getV4VCName", "(Ljava/lang/String;)Ljava/lang/String;" );
      method_showVideo = AndroidJNI.GetStaticMethodID( class_UnityADC, "showVideo", "(Ljava/lang/String;)Z" );
      method_showV4VC = AndroidJNI.GetStaticMethodID( class_UnityADC, "showV4VC", "(ZLjava/lang/String;)Z" );
      method_offerV4VC = AndroidJNI.GetStaticMethodID( class_UnityADC, "offerV4VC", "(ZLjava/lang/String;)V" );
      method_statusForZone = AndroidJNI.GetStaticMethodID( class_UnityADC, "statusForZone", "(Ljava/lang/String;)Ljava/lang/String;" );
      method_getAvailableViews = AndroidJNI.GetStaticMethodID( class_UnityADC, "getAvailableViews", "(Ljava/lang/String;)I" );
      method_notifyIAPComplete = AndroidJNI.GetStaticMethodID( class_UnityADC, "notifyIAPComplete", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;D)V");

      adr_initialized = true;
    } else {
      // adcolony.jar and unityadc.jar most both be in Assets/Plugins/Android/ !
      Debug.LogError( "AdColony configuration error - make sure adcolony.jar and "
          + "unityadc.jar libraries are in your Unity project's Assets/Plugins/Android folder." );
    }
  }

  static void AndroidConfigure( string app_version, string app_id, string[] zone_ids ) {
    if(!adr_initialized) {
      AndroidInitializePlugin();
    }
    // Prepare call arguments.
    class_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

    var j_activity = class_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    var j_app_version = AndroidJNI.NewStringUTF( app_version );
    var j_app_id = AndroidJNI.NewStringUTF( app_id );
    var j_strings = AndroidJNIHelper.ConvertToJNIArray( zone_ids );

    // Call UnityADC.configure( version, app_version, app_id, ids )
    jvalue[] args = new jvalue[4];
    args[0].l = j_activity.GetRawObject();
    args[1].l = j_app_version;
    args[2].l = j_app_id;
    args[3].l = j_strings;

    AndroidJNI.CallStaticVoidMethod( class_UnityADC, method_configure, args );
    configured = true;
  }

  public static void SetCustomID( string custom_id ) {
    if(!adr_initialized){
      AndroidInitializePlugin();
    }
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( custom_id );
    AndroidJNI.CallStaticVoidMethod( class_UnityADC, method_setCustomID, args );
  }

  public static string GetCustomID() {
    jvalue[] args = new jvalue[0];
    return AndroidJNI.CallStaticStringMethod( class_UnityADC, method_getCustomID, args );
  }

  public static bool IsVideoAvailable( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    return AndroidJNI.CallStaticBooleanMethod( class_UnityADC, method_isVideoAvailable, args );

  }

  public static bool IsV4VCAvailable( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    return AndroidJNI.CallStaticBooleanMethod( class_UnityADC, method_isV4VCAvailable, args );
  }

  public static string GetDeviceID() {
    jvalue[] args = new jvalue[0];
    return AndroidJNI.CallStaticStringMethod( class_UnityADC, method_getDeviceID, args );
  }

  public static string AndroidGetOpenUDID() {
    return "undefined";
  }

  public static int GetV4VCAmount( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    return AndroidJNI.CallStaticIntMethod( class_UnityADC, method_getV4VCAmount, args );
  }

  public static string GetV4VCName( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    string name = AndroidJNI.CallStaticStringMethod( class_UnityADC, method_getV4VCName, args );
    return name;
  }

  public static bool ShowVideoAd( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    AndroidJNI.CallStaticBooleanMethod( class_UnityADC, method_showVideo, args );
    return true;
  }

  public static bool ShowV4VC( bool popup_result, string zone_id ) {
    jvalue[] args = new jvalue[2];
    args[0].z = popup_result;
    args[1].l = AndroidJNI.NewStringUTF( zone_id );
    AndroidJNI.CallStaticBooleanMethod( class_UnityADC, method_showV4VC, args );
    return true;
  }

  public static void OfferV4VC( bool popup_result, string zone_id ) {
    jvalue[] args = new jvalue[2];
    args[0].z = popup_result;
    args[1].l = AndroidJNI.NewStringUTF( zone_id );
    AndroidJNI.CallStaticVoidMethod( class_UnityADC, method_offerV4VC, args );
  }

  public static string StatusForZone( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    return AndroidJNI.CallStaticStringMethod( class_UnityADC, method_statusForZone, args );
  }

  public static int GetAvailableViews( string zone_id ) {
    jvalue[] args = new jvalue[1];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    return AndroidJNI.CallStaticIntMethod( class_UnityADC, method_getAvailableViews, args );
  }

  public static void NotifyIAPComplete(string zone_id, string trans_id, string currency_code, double price, int quantity) {
    jvalue[] args = new jvalue[4];
    args[0].l = AndroidJNI.NewStringUTF( zone_id );
    args[1].l = AndroidJNI.NewStringUTF( trans_id );
    args[2].l = AndroidJNI.NewStringUTF( currency_code );
    args[3].d = price;
    AndroidJNI.CallStaticVoidMethod( class_UnityADC, method_notifyIAPComplete, args);
  }
#endif

}
