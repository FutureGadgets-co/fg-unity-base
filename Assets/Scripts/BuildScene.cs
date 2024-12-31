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

        //Assets/Animated Dice (Random Art Attack)/ExampleSceneWithRollingFunctionality.unity
        // 指定要打包的场景
        string[] scenes = { "Assets/Scenes/Level01.unity" }; // 替换为你的场景路径

        // 创建 BuildPipeline.BuildAssetBundles 选项
        AssetBundleBuild[] iosBuildMap = new AssetBundleBuild[1];
        iosBuildMap[0].assetBundleName = "Match-3-iOS.bundle"; // AssetBundle 的名称
        iosBuildMap[0].assetNames = scenes;

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, iosBuildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
        
        AssetBundleBuild[] androidBuildMap = new AssetBundleBuild[1];
        androidBuildMap[0].assetBundleName = "Match-3-Android.bundle"; // AssetBundle 的名称
        androidBuildMap[0].assetNames = scenes;

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, androidBuildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
        
        Debug.Log("Scene AssetBundle built successfully!");
    }
}
#endif