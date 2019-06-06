using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundlesBuilderEditor : MonoBehaviour
{

    [MenuItem("AssetBundles/Build/Android")]
    static void BuildAllAssetBundlesStrictAndroid()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolderAndroid);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolderAndroid, BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolderAndroid + "/AssetBundleNames.csv", bundleNamesCSV);
        Debug.Log($"Android files build success.");
    }

    [MenuItem("AssetBundles/Build/iOS")]
    static void BuildAllAssetBundlesStrictIOS()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolderIOS);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolderIOS, BuildAssetBundleOptions.StrictMode, BuildTarget.iOS);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolderIOS + "/AssetBundleNames.csv", bundleNamesCSV);
        Debug.Log($"iOS files build success.");
    }

    [MenuItem("AssetBundles/Build/Standalone")]
    static void BuildAllAssetBundlesStrictStandalone()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolderStandaloneW64);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolderStandaloneW64, BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolderStandaloneW64 + "/AssetBundleNames.csv", bundleNamesCSV);
        Debug.Log($"Standalone files build success.");
    }

    [MenuItem("AssetBundles/Build/All")]
    static void BuildAllAssetBundlesStrictAll()
    {
        BuildAllAssetBundlesStrictAndroid();
        BuildAllAssetBundlesStrictIOS();
        BuildAllAssetBundlesStrictStandalone();
    }

    [MenuItem("AssetBundles/Generate AssetBundle Names")]
    static void FindImportablePrefabs()
    {
        var assetGUIDS = AssetDatabase.FindAssets("t:prefab", new[] { Constants.Paths.ImportablePrefabs });

        foreach (var assetGUID in assetGUIDS)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset != null)
            {
                var importablePrefab = asset.GetComponent<ImportablePrefab>();
                if (importablePrefab != null)
                {
                    var assetBundleDefinition = importablePrefab.AssetBundleDefinition;
                    AssetImporter.GetAtPath(assetPath)
                    .SetAssetBundleNameAndVariant(assetBundleDefinition.GetAssetBundleName(), string.Empty);
                }
            }
        }
    }
}

