using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundlesBuilderEditor : MonoBehaviour
{
    [MenuItem("Assets/Build AssetBundles/Normal")]
    static void BuildAllAssetBundles()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolder);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolder, BuildAssetBundleOptions.None, BuildTarget.Android);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolder + "/AssetBundleNames.csv", bundleNamesCSV);
    }

    [MenuItem("Assets/Build AssetBundles/Strict")]
    static void BuildAllAssetBundlesStrict()
    {
        Utils.CreateDirectory(Constants.Paths.AssetBundlesBuildFolder);
        var manifest = BuildPipeline.BuildAssetBundles(Constants.Paths.AssetBundlesBuildFolder, BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
        var bundleNamesCSV = Utils.StringArrayToCSV(manifest.GetAllAssetBundles());
        File.WriteAllText(Constants.Paths.AssetBundlesBuildFolder + "/AssetBundleNames.csv", bundleNamesCSV);
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
                    .SetAssetBundleNameAndVariant(
                    Constants.Prefixes.AssetBundleLocation + assetBundleDefinition.Location +
                    "_" + assetBundleDefinition.ID + "_" + assetBundleDefinition.DesignName,
                    string.Empty);
                }
            }
        }
    }
}
