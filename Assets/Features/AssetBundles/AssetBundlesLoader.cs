using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundlesLoader : MonoBehaviour
{
    public Vector3 SpawnVector3 = new Vector3(2, 0, 0);
    public static Transform[] CaptureParents;
    public RenderTexture MyRenderTexture;
    public Texture2D MyTexture2D;

    static UnityEngine.UI.Text DebugText;
    static List<string> assetBundleNamesList = new List<string>();
    static Transform CurrentParent;
    static Dictionary<string, string> currentAssetBundlesSelection = new Dictionary<string, string>();
    static List<AssetBundle> cachedAssetBundles;

    public void Start()
    {
        DebugText = GameObject.Find("Text").GetComponent<UnityEngine.UI.Text>();
        CurrentParent = transform;
        try
        {
            StartCoroutine(InitAssetBundleNamesList(() =>
            {
                LoadSavedShield();
            }
        ));
        }        
        catch (Exception e)
        {
            Debug.LogError(e.Message + e.StackTrace);
            DebugText.text += e.StackTrace;
        }       
    }

    public void LoadSavedShield()
    {
        if(PlayerPrefs.GetInt(Constants.PlayerPrefsKeys.SavedShield, 0) == 1)
        {
            try
            {
                Utils.InstantiateAssetBundle(Utils.LoadAssetBundleFromFile(PlayerPrefs.GetString(Constants.PlayerPrefsKeys.SavedLocationShape)),
                SpawnVector3, CurrentParent);
                Utils.InstantiateAssetBundle(Utils.LoadAssetBundleFromFile(PlayerPrefs.GetString(Constants.PlayerPrefsKeys.SavedLocationOrnament)),
                    SpawnVector3, CurrentParent);
                Utils.InstantiateAssetBundle(Utils.LoadAssetBundleFromFile(PlayerPrefs.GetString(Constants.PlayerPrefsKeys.SavedLocationBottom)),
                    SpawnVector3, CurrentParent);
                Utils.InstantiateAssetBundle(Utils.LoadAssetBundleFromFile(PlayerPrefs.GetString(Constants.PlayerPrefsKeys.SavedLocationStructure)),
                    SpawnVector3, CurrentParent);
            }
            catch(Exception)
            {
                Debug.Log("Unsaved shield");
                Debug.Log("Generating new shield...");
                GenerateRandomShield();
                SaveCurrentSelectionToFile();
            }
        }
        else
        {
            Debug.Log("Unsaved shield");
            Debug.Log("Generating new shield...");
            GenerateRandomShield();
            SaveCurrentSelectionToFile();
        }

    }

    public void GenerateRandomShield(Vector3 position = default(Vector3), int parentIndex = -1)
    {
        if(parentIndex < 0)
        {
            DeleteCurrentSelection();
        }
        else
        {
            DeleteCaptureSelection(parentIndex);
        }        

        try
        {
            StartCoroutine(InstantiateRandomShieldLocation(ShieldLocation.Shape, position, parentIndex));
            StartCoroutine(InstantiateRandomShieldLocation(ShieldLocation.Ornament, position, parentIndex));
            StartCoroutine(InstantiateRandomShieldLocation(ShieldLocation.Bottom, position, parentIndex));
            StartCoroutine(InstantiateRandomShieldLocation(ShieldLocation.Structure, position, parentIndex));
        }
        catch (Exception e)
        {
            var errorMsg = $"Failed to connect to asset bundles server: {e.Message}";
            DebugText.text = errorMsg;
            Debug.LogError(errorMsg);
        }
        
    }

    public void DeleteCaptureSelection(int parentIndex)
    {
        for (int i = 0; i < CaptureParents[parentIndex].childCount; i++)
        {
            Destroy(CaptureParents[parentIndex].GetChild(i).gameObject);
        }
    }

    public void DeleteCurrentSelection()
    {
        currentAssetBundlesSelection.Clear();

        for (int i = 0; i < CurrentParent.childCount; i++)
        {         
            Destroy(CurrentParent.GetChild(i).gameObject);
        }
        
    }

    public void SaveCurrentSelectionToFile()
    {
        StartCoroutine(SaveCurrentSelectionToFileInternal());
    }

    IEnumerator<object> SaveCurrentSelectionToFileInternal()
    {
        foreach (var assetBundleName in currentAssetBundlesSelection.Values)
        {
            UnityWebRequest www = UnityWebRequest.Get(Constants.Routes.BackendIp + assetBundleName);
            yield return www.SendWebRequest();
            File.WriteAllBytes(Application.persistentDataPath + "/" + assetBundleName, www.downloadHandler.data);
            Debug.Log($"Saved in {Application.persistentDataPath + "/" + assetBundleName}");
            if(assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Shape.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationShape, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Ornament.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationOrnament, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Bottom.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationBottom, assetBundleName);
            }
            else if (assetBundleName.StartsWith(Constants.Prefixes.AssetBundleLocation + ShieldLocation.Structure.Value))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.SavedLocationStructure, assetBundleName);
            }
            PlayerPrefs.SetInt(Constants.PlayerPrefsKeys.SavedShield, 1);
        }
    }

    static IEnumerator<object> InstantiateRandomShieldLocation(ShieldLocation location, Vector3 position = default(Vector3), int parentIndex = -1)
    {
        var prefixLocation = Constants.Prefixes.AssetBundleLocation + location.Value;
        var prefixAssetList = assetBundleNamesList.Where(s => s.StartsWith(prefixLocation)).ToArray();
        var prefixAssetListCount = prefixAssetList.Count();
        int randomIndex = 0;
        if(prefixAssetListCount > 1)
        {
            randomIndex = UnityEngine.Random.Range(0, prefixAssetListCount);
        }

        AssetBundle downloadedAssetBundle = null;
        UnityWebRequest request = null;

        cachedAssetBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
        AssetBundle cachedAssetBundle = cachedAssetBundles.FirstOrDefault(a => a.name == prefixAssetList[randomIndex]);

        if (cachedAssetBundle == null)
        {
            string uri = Constants.Routes.BackendIp + prefixAssetList[randomIndex];

            Debug.Log($"Request {uri}");

            request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                DebugText.text += "NetworkError, HttpError " + request.error;
                Debug.LogError("NetworkError, HttpError : " + request.error);
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
            Debug.LogError("Failed to load AssetBundle! Download failed for " + prefixAssetList[randomIndex]
            + request.error + request.downloadHandler.text);
            yield break;
        }

        currentAssetBundlesSelection[location.Value] = prefixAssetList[randomIndex];

        try
        {
            var go = Utils.InstantiateAssetBundle(downloadedAssetBundle, position, parentIndex < 0 ? CurrentParent : CaptureParents[parentIndex]);

            var importablePrefab = go.GetComponent<ImportablePrefab>();
            var assetBundleDef = importablePrefab.AssetBundleDefinition;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + e.StackTrace);
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
            Debug.LogError(request.error);
            DebugText.text = request.error;
            yield break;
        }

        if (string.IsNullOrEmpty(request.downloadHandler.text))
        {
            var msgError = Constants.Paths.AssetBundlesNames + "couldn't be retrieved";
            Debug.LogError(msgError);
            DebugText.text = msgError;
            yield break;
        }

        foreach (var name in request.downloadHandler.text.Split(','))
        {
            assetBundleNamesList.Add(name);
        }
        onEndCoroutineCallback?.Invoke();
    }
}
