#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AssetBundleBuilder
{
    [MenuItem("Build/Build Scene AssetBundle")]
    public static void BuildSceneAssetBundle()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!System.IO.Directory.Exists(assetBundleDirectory))
        {
            System.IO.Directory.CreateDirectory(assetBundleDirectory);
        }

        // 指定要打包的场景
        string[] scenes = { "Assets/Animated Dice (Random Art Attack)/ExampleSceneWithRollingFunctionality.unity" }; // 替换为你的场景路径

        // 创建 BuildPipeline.BuildAssetBundles 选项
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "Dice.bundle"; // AssetBundle 的名称
        buildMap[0].assetNames = scenes;

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
        Debug.Log("Scene AssetBundle built successfully!");
    }
}
#endif