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
        public const string AssetBundlesBuildFolder = "Assets/Features/AssetBundles/BuildFolder";
        public const string AssetBundlesNames = "AssetBundleNames.csv";
        public const string Pngs = "pngs";
    }

    public struct Locations
    {
        public const string Ornament = "ornament";
        public const string Shape = "shape";
        public const string Bottom = "bottom";
        public const string Structure = "structure";
    }

    public struct PlayerPrefsKeys
    {
        public const string SavedShield = "SavedShield";
        public const string SavedLocationShape = "SavedLocationShape";
        public const string SavedLocationOrnament = "SavedLocationOrnament";
        public const string SavedLocationStructure = "SavedLocationStructure";
        public const string SavedLocationBottom = "SavedLocationbottom";
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
