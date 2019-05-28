using UnityEngine;

[CreateAssetMenu(fileName = "AssetBundleDefinition", menuName = "Definitions/AssetBundleDefinition", order = 1)]
public class AssetBundleDefinition : ScriptableObject
{
    public uint ID;
    public ShieldLocations Location;
    public string DesignName;
    public Material[] MaterialCatalog;

    public string GetAssetBundleName()
    {
        return Constants.Prefixes.AssetBundleLocation + Location + "_" + ID + "_" + DesignName;
    }
}
