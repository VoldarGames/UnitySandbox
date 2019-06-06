using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class UploadAssetBundles : MonoBehaviour
{
    static HttpClient httpclient = new HttpClient();

    static UploadAssetBundles()
    {
        httpclient.BaseAddress = new Uri("http://127.0.0.1:8080/");
    }

    [MenuItem("AssetBundles/Upload/Android")]
    async static void UploadAndroidAssetBundles()
    {
        await UploadAssetBundleFiles(Constants.Paths.AssetBundlesBuildFolderAndroid, Constants.Paths.AndroidUploadFolderName);
    }

    [MenuItem("AssetBundles/Upload/iOS")]
    async static void UploadIOSAssetBundles()
    {
        await UploadAssetBundleFiles(Constants.Paths.AssetBundlesBuildFolderIOS, Constants.Paths.IOSUploadFolderName);
    }

    [MenuItem("AssetBundles/Upload/Standalone")]
    async static void UploadStandaloneAssetBundles()
    {
        await UploadAssetBundleFiles(Constants.Paths.AssetBundlesBuildFolderStandaloneW64, Constants.Paths.StandaloneWindows64UploadFolderName);
    }

    [MenuItem("AssetBundles/Upload/All")]
    static void UploadAllAssetBundles()
    {
        UploadAndroidAssetBundles();
        UploadIOSAssetBundles();
        UploadStandaloneAssetBundles();
    }

    static async Task UploadAssetBundleFiles(string buildFolderPath, string uploadFolderName)
    {
        try
        {
            var files = Directory.EnumerateFiles(buildFolderPath);
            Debug.Log($"Uploading {files.Count()} {uploadFolderName} files...");
            for (int i = 0; i < files.Count(); i++)
            {
                var filename = new FileInfo(files.ElementAt(i)).Name;
                Debug.Log($"Uploading File [{filename}] Progress [{i + 1}/{files.Count()}]");

                var form = new MultipartFormDataContent();
                using (var streamContent = new StreamContent(File.Open(files.ElementAt(i), FileMode.Open)))
                {
                    form.Add(streamContent, "file", filename);
                    var response = await httpclient.PostAsync(uploadFolderName, form);
                }
            }
            Debug.Log($"{uploadFolderName} files uploaded to {httpclient.BaseAddress} successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"{uploadFolderName} files upload error. Reason: {e.Message}");
        }
    }
}
