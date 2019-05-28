using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundlesBuilderEditor : MonoBehaviour
{

    [MenuItem("Assets/Build AssetBundles/Android")]
    static void BuildAllAssetBundlesStrictAndroid()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolderAndroid);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolderAndroid, BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolderAndroid + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    [MenuItem("Assets/Build AssetBundles/iOS")]
    static void BuildAllAssetBundlesStrictIOS()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolderIOS);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolderIOS, BuildAssetBundleOptions.StrictMode, BuildTarget.iOS);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolderIOS + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    [MenuItem("Assets/Build AssetBundles/Standalone")]
    static void BuildAllAssetBundlesStrictStandalone()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolderStandalone);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolderStandalone, BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolderStandalone + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    [MenuItem("Assets/Build AssetBundles/All")]
    static void BuildAllAssetBundlesStrictAll()
    {
        BuildAllAssetBundlesStrictAndroid();
        BuildAllAssetBundlesStrictIOS();
        BuildAllAssetBundlesStrictStandalone();
    }

    [MenuItem("Assets/Generate AssetBundle Names")]
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
                if(importablePrefab != null)
                {
                    var assetBundleDefinition = importablePrefab.AssetBundleDefinition;
                    AssetImporter.GetAtPath(assetPath)
                    .SetAssetBundleNameAndVariant(assetBundleDefinition.GetAssetBundleName(), string.Empty);
                }
            }
        }
    }
}
