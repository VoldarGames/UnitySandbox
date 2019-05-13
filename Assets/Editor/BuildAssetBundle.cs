using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle : MonoBehaviour
{
    const string LOCATION_PREFIX = "Loc";
    const string IMPORTABLE_PREFABS_PATH = "Assets/Features/AssetBundles";

    [MenuItem("Assets/Build AssetBundles/Normal")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = CreateAssetBundleDirectory();
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.Android);
        var bundleNamesCSV = StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(assetBundleDirectory + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    

    [MenuItem("Assets/Build AssetBundles/Strict")]
    static void BuildAllAssetBundlesStrict()
    {
        string assetBundleDirectory = CreateAssetBundleDirectory();
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
        var bundleNamesCSV = StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(assetBundleDirectory + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    [MenuItem("Assets/Generate AssetBundle Names")]
    static void FindImportablePrefabs()
    {
        var assetGUIDS = AssetDatabase.FindAssets("t:prefab", new[] { IMPORTABLE_PREFABS_PATH });

        foreach (var assetGUID in assetGUIDS)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset != null)
            {
                var importablePrefab = asset.GetComponent<ImportablePrefab>();
                if(importablePrefab != null)
                {
                    var assetBundleDefinition = importablePrefab.AssetBundleDefinition;
                    AssetImporter.GetAtPath(assetPath)
                    .SetAssetBundleNameAndVariant(
                    LOCATION_PREFIX + "_" + assetBundleDefinition.Location +
                    "_" + assetBundleDefinition.ID + "_" + assetBundleDefinition.DesignName,
                    string.Empty);
                }
            }
        }
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

    static string CreateAssetBundleDirectory()
    {
        string assetBundleDirectory = "Assets/Features/AssetBundles/BuildFolder";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        return assetBundleDirectory;
    }
}
