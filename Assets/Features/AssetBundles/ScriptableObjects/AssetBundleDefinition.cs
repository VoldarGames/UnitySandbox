using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetBundleDefinition", menuName = "Definitions/AssetBundleDefinition", order = 1)]
public class AssetBundleDefinition : ScriptableObject
{
    public uint ID;
    public string Location;
    public string DesignName;
    public Material[] MaterialCatalog;
}
