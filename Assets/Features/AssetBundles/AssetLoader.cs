using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AssetLoader : MonoBehaviour
{
    const string LOCATION_PREFIX = "loc_";
    const string IP = "http://172.16.11.36:8080/";
    static UnityEngine.UI.Text DebugText;
    static List<string> assetBundleList = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        DebugText = GameObject.Find("Text").GetComponent<UnityEngine.UI.Text>();
        try
        {
            StartCoroutine(InitAssetBundleList(() => {
                StartCoroutine(InstantiateRandomAssetBundleGameObject("Shape"));
                StartCoroutine(InstantiateRandomAssetBundleGameObject("Ornament"));
                StartCoroutine(InstantiateRandomAssetBundleGameObject("Bottom"));
                StartCoroutine(InstantiateRandomAssetBundleGameObject("Structure"));
            }
        ));
        }
        catch(Exception e)
        {
            DebugText.text += e.StackTrace;
        }
                
    }

    void AwaitCoroutine(IEnumerator<object> coroutineFunction, Action continueWithAction)
    {
        StartCoroutine(coroutineFunction);
    }

    static IEnumerator<object> InstantiateRandomAssetBundleGameObject(string prefixLocation)
    {
        prefixLocation = LOCATION_PREFIX + prefixLocation.ToLower();
        var prefixAssetList = assetBundleList.Where(s => s.StartsWith(prefixLocation)).ToArray();
        var prefixAssetListCount = prefixAssetList.Count();
        int randomIndex = 0;
        if(prefixAssetListCount > 1)
        {
            randomIndex = UnityEngine.Random.Range(0, prefixAssetListCount);
        }

        //var loadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, prefixAssetList[randomIndex]));
        string uri = IP + prefixAssetList[randomIndex];

        DebugText.text += $"Request {uri}";
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0); 
        yield return request.SendWebRequest();
        //DebugText.text += "IsDone:" + request.downloadHandler.isDone + ": " + request.downloadHandler.text; 
        if (request.isNetworkError || request.isHttpError)
        {
            DebugText.text += "NetworkError, HttpError " + request.error;
            Debug.Log(request.error);
            yield break;
        }
        AssetBundle downloadedAssetBundle = DownloadHandlerAssetBundle.GetContent(request);

        if (downloadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            DebugText.text += "Download failed for " + prefixAssetList[randomIndex]
            + request.error + request.downloadHandler.text;
            yield break;
        }
        var names = downloadedAssetBundle.GetAllAssetNames();
        try
        {
            var prefab = downloadedAssetBundle.LoadAsset<GameObject>(names[0]);
            Instantiate(prefab, new Vector3(2, 0, 0), Quaternion.identity);
        }
        catch (Exception e)
        {
            DebugText.text += e.Message;
        }
        
    }

    IEnumerator<object> InitAssetBundleList(Action onEndCoroutineCallback)
    {
        //var assetBundleNamesCSV = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "AssetBundleNames.csv"));
        
        string uri = IP + "AssetBundleNames.csv";
        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();        
        //AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

        if (string.IsNullOrEmpty(request.downloadHandler.text))
        {
            Debug.LogError("AssetBundleNames.csv can't be found on StreamingAssets folder");
        }

        foreach (var name in request.downloadHandler.text.Split(','))
        {
            assetBundleList.Add(name);
        }
        onEndCoroutineCallback?.Invoke();
    }
}
