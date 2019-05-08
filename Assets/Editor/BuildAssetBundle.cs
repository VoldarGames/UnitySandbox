using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle : MonoBehaviour
{
    [MenuItem("Assets/Build AssetBundles/Normal")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/Features/AssetBundles/BuildFolder";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Build AssetBundles/Strict")]
    static void BuildAllAssetBundlesStrict()
    {
        string assetBundleDirectory = "Assets/Features/AssetBundles/BuildFolder";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows);
        var bundleNamesCSV = StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(assetBundleDirectory + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    static string StringArrayToCSV(string[] stringArray)
    {
        var result = string.Empty;
        foreach (var str in stringArray)
        {
            result += string.Format("{0},", str);
        }
        return result.TrimEnd(',');
    }
}
