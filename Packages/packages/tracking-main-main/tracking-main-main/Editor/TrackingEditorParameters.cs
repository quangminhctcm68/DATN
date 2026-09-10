namespace NabaGame.Tracking.Editor
{
    public static class TrackingEditorParameters
    {
        public static readonly string SYMBOL_FIREBASE_ANALYTIC = "BMH_FIREBASE_ANALYTIC";
        public static readonly string SYMBOL_FACEBOOK_ANALYTIC = "NB_FACEBOOK_ANALYTIC";
        public static readonly string SYMBOL_GAMEANALYTIC = "NB_GAMEANALYTIC";
        public static readonly string SYMBOL_BYTEBREW_ANALYTIC = "NB_BYTEBREW_ANALYTIC";
        public static readonly string SYMBOL_ADMOB = "NB_ADMOB";
        
        public static readonly string CLASS_FIREBASE_ANALYTIC = "Firebase.Analytics.FirebaseAnalytics,Firebase.Analytics";
        public static readonly string CLASS_FACEBOOK_ANALYTIC  = "Facebook.Unity.FB,Facebook.Unity";
        public static readonly string CLASS_GAMEANALYTIC  = "GameAnalyticsSDK.GameAnalytics,Assembly-CSharp";
        public static readonly string CLASS_BYTEBREW_ANALYTIC  = "ByteBrew.cs,Assembly-CSharp";
        
        public static readonly string FACEBOOK_ANALYTIC_NAMESPACE  = "Facebook.Unity";
        public static readonly string FIREBASE_ANALYTIC_NAMESPACE  = "Firebase.Analytics";
        public static readonly string GAMEANALYTIC_NAMESPACE  = "GameAnalyticsSDK";
    }
}