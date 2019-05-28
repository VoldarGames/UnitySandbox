public struct Constants
{
    public struct Prefixes
    {
        public const string AssetBundleLocation = "loc_";
        public const string AssetBundleMaterial = "mat_";
    }

    public struct Paths
    {
        public const string ImportablePrefabs = "Assets/Features/AssetBundles";
        public const string AssetBundlesBuildFolderAndroid = "Assets/Features/AssetBundles/BuildFolder/Android";
        public const string AssetBundlesBuildFolderIOS = "Assets/Features/AssetBundles/BuildFolder/iOS";
        public const string AssetBundlesBuildFolderStandalone = "Assets/Features/AssetBundles/BuildFolder/Standalone";
        public const string AssetBundlesNames = "AssetBundleNames.csv";
        public const string Pngs = "pngs";
    }

    public struct PlayerPrefsKeys
    {
        public const string SavedShield = "SavedShield";
        public const string SavedLocationBanner = "SavedLocationBanner";
        public const string SavedLocationFrame = "SavedLocationFrame";
        public const string SavedLocationSecondFrame = "SavedLocationSecondFrame";
        public const string SavedLocationThirdFrame = "SavedLocationThirdFrame";
        public const string SavedLocationShape = "SavedLocationShape";
        public const string SavedLocationSymbol = "SavedLocationSymbol";
        public const string SavedLocationTop = "SavedLocationTop";
        public const string SavedLocationWings = "SavedLocationWings";
    }

    public struct Routes
    {
        public const string BackendIp = "http://127.0.0.1:8080/"; //office "http://172.16.11.36:8080/"
        public const string AssetBundlesNames = BackendIp + Paths.AssetBundlesNames;

        public const string RequestRandomShieldGuid = "/RequestRandomShieldGuid";
        public const string GetGifByGuid = "/GetGifByGuid";
    }

    public struct SceneManagers
    {
        public const string AssetBundlesLoader = "AssetBundlesLoader";
        public const string GifCaptureManager = "GifCaptureManager";
        public const string CaptureJobsManager = "CaptureJobsManager";
    }
}
