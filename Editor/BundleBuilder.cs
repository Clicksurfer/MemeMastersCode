using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleBuilder : Editor
{
    //Script is used to allow us to build asset bundles from editor
    [MenuItem("Assets/ Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles(@"E:\Users\Emmet\Documents\Meme Masters2019Ver4\Assets\outputFolder", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }
}
