using System;
using System.Collections.Generic;

[Serializable]
public class Shield
{
    public int TopId;
    public string TopAssetBundleName;
    public int FrameId;
    public string FrameAssetBundleName;
    public int FrameSecondId;
    public string FrameSecondAssetBundleName;
    public int FrameThirdId;
    public string FrameThirdAssetBundleName;
    public int ShapeId;
    public string ShapeAssetBundleName;
    public int SymbolId;
    public string SymbolAssetBundleName;
    public int BannerId;
    public string BannerAssetBundleName;
    public int WingsId;
    public string WingsAssetBundleName;

    public List<string> GetAllAssetBundleNames()
    {
        return new List<string>
        {
            TopAssetBundleName,
            FrameAssetBundleName,
            FrameSecondAssetBundleName,
            FrameThirdAssetBundleName,
            ShapeAssetBundleName,
            SymbolAssetBundleName,
            BannerAssetBundleName,
            WingsAssetBundleName
        };
    }
}
