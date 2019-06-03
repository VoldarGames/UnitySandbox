using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundlesLoader : MonoBehaviour
{
    public Vector3 SpawnVector3 = new Vector3(0, 0, 0);
    public static Transform[] CaptureParents;
    public RenderTexture MyRenderTexture;
    public Texture2D MyTexture2D;

    static UnityEngine.UI.Text DebugText;
    static List<string> assetBundleNamesList = new List<string>();
    static Dictionary<ShieldLocation, List<string>> locationAssetBundlesDictionary = new Dictionary<ShieldLocation, List<string>>();
    static Transform CurrentParent;
    static Shield currentShieldSelection;
    static List<AssetBundle> cachedAssetBundles;

    public void Start()
    {
        DebugText = GameObject.Find("Text").GetComponent<UnityEngine.UI.Text>();
        CurrentParent = transform;
        try
        {
            StartCoroutine(InitAssetBundleNamesList(() =>
            {
                //LoadSavedShield();
            }
        ));
        }        
        catch (Exception e)
        {
            Logger.Log(e.Message + e.StackTrace, Logger.LogLevel.Error);
            DebugText.text += e.StackTrace;
        }       
    }

    public void LoadSavedShield()
    {
        if(PlayerPrefs.GetInt(Constants.PlayerPrefsKeys.SavedShield, 0) == 1)
        {
            try
            {
                Logger.Log("Loading previous saved shield");

                foreach (var key in Constants.PlayerPrefsKeys.AllKeys)
                {
                    var path = PlayerPrefs.GetString(key);
                    Logger.Log($"Shape: {path}");
                    Utils.InstantiateAssetBundle(Utils.LoadAssetBundleFromFile(path), SpawnVector3, CurrentParent);
                }
            }
            catch(Exception)
            {
                Logger.Log("Unsaved shield, generating a new one...");
                GenerateRandomShield();
                SaveCurrentSelectionToFile();
            }
        }
        else
        {
            Logger.Log("Unsaved shield, generating a new one...");
            GenerateRandomShield();
            SaveCurrentSelectionToFile();
        }

    }

    public void GenerateRandomShield()
    {
        GenerateRandomShield(Vector3.zero);
    }

    public Shield GenerateRandomShield(Vector3 position, int parentIndex = -1)
    {
        Logger.Log($"Generating random shield at {position}");
        if (parentIndex < 0)
        {
            DeleteCurrentSelection();
        }
        else
        {
            DeleteCaptureSelection(parentIndex);
        }        

        try
        {
            currentShieldSelection = GetRandomShield();
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.BannerAssetBundleName, ShieldLocation.Banner, position, parentIndex));            
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.FrameAssetBundleName, ShieldLocation.Frame, position, parentIndex));
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.FrameSecondAssetBundleName, ShieldLocation.FrameSecond, position, parentIndex));
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.FrameThirdAssetBundleName, ShieldLocation.FrameThird, position, parentIndex));            
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.ShapeAssetBundleName, ShieldLocation.Shape, position, parentIndex));            
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.SymbolAssetBundleName, ShieldLocation.Symbol, position, parentIndex));            
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.TopAssetBundleName, ShieldLocation.Top, position, parentIndex));            
            StartCoroutine(InstantiateShieldLocation(currentShieldSelection.WingsAssetBundleName, ShieldLocation.Wings, position, parentIndex));
            return currentShieldSelection;
        }
        catch (Exception e)
        {
            var errorMsg = $"Failed to connect to asset bundles server: {e.Message}";
            DebugText.text = errorMsg;
            Logger.Log(errorMsg, Logger.LogLevel.Error);
            return null;
        }        
    }

    public void DeleteCaptureSelection(int parentIndex)
    {
        Logger.Log($"Deleting childs of slot {parentIndex}", Logger.LogLevel.Debug);
        for (int i = 0; i < CaptureParents[parentIndex].childCount; i++)
        {
            Destroy(CaptureParents[parentIndex].GetChild(i).gameObject);
        }
    }

    public void DeleteCurrentSelection()
    {
        Logger.Log($"Deleting childs of main selection", Logger.LogLevel.Debug);
        currentShieldSelection = null;

        for (int i = 0; i < CurrentParent.childCount; i++)
        {         
            Destroy(CurrentParent.GetChild(i).gameObject);
        }        
    }

    public void SaveCurrentSelectionToFile()
    {
        StartCoroutine(SaveCurrentSelectionToFileInternal());
    }

    Shield GetRandomShield()
    {
        var resultShield = new Shield();

        var bannerIndex = GetRandomShieldLocationIndex(ShieldLocation.Banner);
        resultShield.BannerId = GetIdFromIndex(ShieldLocation.Banner, bannerIndex);
        resultShield.BannerAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Banner, bannerIndex);

        var frameIndex = GetRandomShieldLocationIndex(ShieldLocation.Frame);
        resultShield.FrameId = GetIdFromIndex(ShieldLocation.Frame, frameIndex);
        resultShield.FrameAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Frame, frameIndex);
        var frameSecondIndex = GetRandomShieldLocationIndex(ShieldLocation.Frame, true, frameIndex);
        resultShield.FrameSecondId = GetIdFromIndex(ShieldLocation.Frame, frameSecondIndex);
        resultShield.FrameSecondAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Frame, frameSecondIndex);
        var frameThirdIndex = GetRandomShieldLocationIndex(ShieldLocation.Frame, true, frameIndex, frameSecondIndex);
        resultShield.FrameThirdId = GetIdFromIndex(ShieldLocation.Frame, frameThirdIndex);
        resultShield.FrameThirdAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Frame, frameThirdIndex);

        var shapeIndex = GetRandomShieldLocationIndex(ShieldLocation.Shape);
        resultShield.ShapeId = GetIdFromIndex(ShieldLocation.Shape, shapeIndex);
        resultShield.ShapeAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Shape, shapeIndex);

        var symbolIndex = GetRandomShieldLocationIndex(ShieldLocation.Symbol, acceptsNone: true);
        resultShield.SymbolId = GetIdFromIndex(ShieldLocation.Symbol, symbolIndex);
        resultShield.SymbolAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Symbol, symbolIndex);

        var topIndex = GetRandomShieldLocationIndex(ShieldLocation.Top, acceptsNone: true);
        resultShield.TopId = GetIdFromIndex(ShieldLocation.Top, topIndex);
        resultShield.TopAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Top, topIndex);

        var wingsIndex = GetRandomShieldLocationIndex(ShieldLocation.Wings, acceptsNone: true);
        resultShield.WingsId = GetIdFromIndex(ShieldLocation.Wings, wingsIndex);
        resultShield.WingsAssetBundleName = GetAssetBundleNameByIndex(ShieldLocation.Wings, wingsIndex);

        return resultShield;
    }

    string GetAssetBundleNameByIndex(ShieldLocation location, int index)
    {
        if (index == -1) return string.Empty;
        return locationAssetBundlesDictionary[location][index];
    }

    string GetAssetBundleNameById(ShieldLocation location, int id)
    {
        if (id < 0) return string.Empty;
        var locationAssetBundles = locationAssetBundlesDictionary[location];

        foreach (var assetBundleName in locationAssetBundles)
        {
            if (GetIdFromAssetBundleName(location, assetBundleName) == id)
            {
                return assetBundleName;
            }
        }
        return string.Empty;
    }

    int GetIdFromAssetBundleName(ShieldLocation location, string assetBundleName)
    {
        var result = assetBundleName.Replace(Constants.Prefixes.AssetBundleLocation + location.Value + "_", string.Empty);
        var indexOf_ = result.IndexOf("_");
        result = result.Replace(result.Substring(indexOf_, result.Length - indexOf_), string.Empty);
        try
        {
            return int.Parse(result);
        }
        catch
        {
            Logger.Log($"Parse error for Asset Bundle Name {assetBundleName}", Logger.LogLevel.Error);
            return -1;
        }
    }

    int GetIdFromIndex(ShieldLocation location, int index)
    {
        if (index == -1) return -1;

        return GetIdFromAssetBundleName(location, locationAssetBundlesDictionary[location][index]);
    }

    IEnumerator<object> SaveCurrentSelectionToFileInternal()
    {
        foreach (var assetBundleName in currentShieldSelection.GetAllAssetBundleNames())
        {
            UnityWebRequest www = UnityWebRequest.Get(Constants.Routes.BackendIp + assetBundleName);
            yield return www.SendWebRequest();
            File.WriteAllBytes(Application.persistentDataPath + "/" + assetBundleName, www.downloadHandler.data);
            Logger.Log($"Saved in {Application.persistentDataPath + "/" + assetBundleName}", Logger.LogLevel.Info);           
            if(assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Shape.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationShape, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Frame.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationFrame, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Banner.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationBanner, assetBundleName);
            }                        
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Symbol.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationSymbol, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.FrameSecond.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationSecondFrame, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Top.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationTop, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Wings.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationWings, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.FrameThird.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationThirdFrame, assetBundleName);
            }
            PlayerPrefs.SetInt(Constants.PlayerPrefsKeys.SavedShield, 1);
        }
    }

    static int GetRandomShieldLocationIndex(ShieldLocation location, bool acceptsNone = false, params int[] bannedIndexes)
    {
        var shieldLocationBundlesCount = locationAssetBundlesDictionary[location].Count();
        int randomIndex = 0;
        if(shieldLocationBundlesCount > 1)
        {
            randomIndex = Utils.Random(acceptsNone ? -1 : 0, shieldLocationBundlesCount, bannedIndexes);
        }
        return randomIndex;
    }

    static IEnumerator<object> InstantiateShieldLocation(string assetBundleName, ShieldLocation keySelection, Vector3 position = default, int parentIndex = -1)
    {
        AssetBundle downloadedAssetBundle = null;
        UnityWebRequest request = null;

        cachedAssetBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
        AssetBundle cachedAssetBundle = cachedAssetBundles.FirstOrDefault(a => a.name == assetBundleName);

        if (cachedAssetBundle == null)
        {
            string uri = Constants.Routes.BackendIp + assetBundleName;

            Logger.Log($"Request {uri}", Logger.LogLevel.Info);

            request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                DebugText.text += "NetworkError, HttpError " + request.error;
                Logger.Log($"NetworkError, HttpError : " + request.error, Logger.LogLevel.Error);
                yield break;
            }
            downloadedAssetBundle = DownloadHandlerAssetBundle.GetContent(request);
        }
        else
        {
            downloadedAssetBundle = cachedAssetBundle;
        }

        if (downloadedAssetBundle == null)
        {
            Logger.Log("Failed to load AssetBundle! Download failed for " + assetBundleName
            + request.error + request.downloadHandler.text, Logger.LogLevel.Error);
            yield break;
        }      

        try
        {
            var go = Utils.InstantiateAssetBundle(downloadedAssetBundle, position, parentIndex < 0 ? CurrentParent : CaptureParents[parentIndex]);

            var importablePrefab = go.GetComponent<ImportablePrefab>();
            var assetBundleDef = importablePrefab.AssetBundleDefinition;
        }
        catch (Exception e)
        {
            Logger.Log(e.Message + e.StackTrace, Logger.LogLevel.Error);
            DebugText.text += e.Message;
        }
    }

    IEnumerator<object> InitAssetBundleNamesList(Action onEndCoroutineCallback)
    {
        string uri = Constants.Routes.AssetBundlesNames;
        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();

        if(request.isHttpError || request.isNetworkError)
        {            
            Logger.Log(request.error, Logger.LogLevel.Error);
            DebugText.text = request.error;
            yield break;
        }

        if (string.IsNullOrEmpty(request.downloadHandler.text))
        {
            var msgError = Constants.Paths.AssetBundlesNames + "couldn't be retrieved";

            Logger.Log(msgError, Logger.LogLevel.Error);
            DebugText.text = msgError;
            yield break;
        }

        foreach (var name in request.downloadHandler.text.Split(','))
        {
            assetBundleNamesList.Add(name);
        }

        InitLocationAssetBundlesDictionary();

        onEndCoroutineCallback?.Invoke();
    }

    void InitLocationAssetBundlesDictionary()
    {
        foreach (var location in ShieldLocation.AllShieldLocations)
        {
            locationAssetBundlesDictionary.Add(location, assetBundleNamesList.Where(a => a.StartsWith(Constants.Prefixes.AssetBundleLocation + location.Value)).ToList());
        }
    }
}
