using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadAssetBundles : MonoBehaviour
{
    public string onlinePathBase;
    public string[] memePackages = new string[] { "base", "ragememes" };
    public bool AllPackagesLoaded = false;
    public int PackagesLoadedCount = 0;
    private MemeLoaderScript myMemeLoaderScript;
    AssetBundle[] myLoadedAssetbundles;
    private float[] assetBundleDownloadProgress;
    private bool[] downloadingFromInternet;

    //Script is responsible for loading memes from assetpackages server, and updating UI in initial panel accordingly
    public void GetAllAssetBundles()
    {
        myMemeLoaderScript = this.GetComponent<MemeLoaderScript>();
        myLoadedAssetbundles = new AssetBundle[memePackages.Length];
        assetBundleDownloadProgress = new float[memePackages.Length];
        downloadingFromInternet = new bool[memePackages.Length];
        for (int i = 0; i < memePackages.Length; i++)
        {
            downloadingFromInternet[i] = false;
            assetBundleDownloadProgress[i] = 0f;
            StartCoroutine(LoadAssetBundleFromCacheOrWeb(onlinePathBase, i, memePackages[i]));
        }
    }

    //Coroutine is called per meme package, and loads it from server
    IEnumerator LoadAssetBundleFromCacheOrWeb(string bundleUrlBase, int bundleIndex, string bundleName)
    {

        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrlBase + bundleName + "/" + bundleName, 2, 0))
        {
            downloadingFromInternet[bundleIndex] = !Caching.IsVersionCached(uwr.url, 2);//Check if asset is being downloaded from server or taken from cache
            var operation = uwr.SendWebRequest();

            while (!operation.isDone)// As object is downloading, update vars accordingly
            {
                assetBundleDownloadProgress[bundleIndex] = uwr.downloadProgress;
                myMemeLoaderScript.UpdateLoadingText();
                yield return null;
            }

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Debug.Log("Got bundle from cache or internet");
                // Get downloaded asset bundle
                myLoadedAssetbundles[bundleIndex] = DownloadHandlerAssetBundle.GetContent(uwr);
                yield return new WaitForSeconds(Time.deltaTime);
                AssignSpritesToMemes(bundleIndex, bundleName);
            }
        }
    }

    //After memes are downloaded from assetbundle, time to assign them to our object as Sprites.
    void AssignSpritesToMemes(int index, string packageName)
    {
        List<Sprite> myMemes = new List<Sprite>();
        List<string> myPackage = new List<string>();
        foreach (Texture2D myTex in myLoadedAssetbundles[index].LoadAllAssets<Texture2D>())
        {
            myPackage.Add(packageName);
            myMemes.Add(Sprite.Create(myTex, new Rect(0.0f, 0.0f, myTex.width, myTex.height), new Vector2(0.5f, 0.5f), 100.0f));
        }
        myMemeLoaderScript.memes.AddRange(myMemes);
        myMemeLoaderScript.memePackage.AddRange(myPackage);
        Debug.Log("Set sprites from assetbundle");
        PackagesLoadedCount++;
        if (PackagesLoadedCount == memePackages.Length)//In case this is the final package, allow the loading panel to begin exiting
            AllPackagesLoaded = true;
        myMemeLoaderScript.UpdateLoadingText();
        //myMemeLoaderScript.CalcDisplay();
    }

    public float GetDownloadProgress(int index = -1)//Allows to get individual progress, by default returns progress of all packages
    {
        if (index > -1)
            return assetBundleDownloadProgress[index];
        else
        {
            float progressTotal = 0f;
            foreach (float val in assetBundleDownloadProgress)
                progressTotal += val;
            return (progressTotal / (float)assetBundleDownloadProgress.Length);
        }

    }

    public bool IsDownloadingPackages(int index = -1)//Allows to get individual info, by default returns true if at least one is downloading
    {
        if (index > -1)
            return downloadingFromInternet[index];
        else
        {
            foreach (bool val in downloadingFromInternet)
                if (val)
                    return true;
            return false;
        }

    }
}
