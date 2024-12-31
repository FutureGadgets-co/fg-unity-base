using System;
using System.Collections;
using System.Collections.Generic;
using FlutterUnityIntegration;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
//public class MessageData
//{
//    public string methodName;
//    public string sceneName;
//}
public class FGMessageManager : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 注册场景加载完成的处理方法
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 注销场景加载完成的处理方法
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Dictionary<string, string> msg = new Dictionary<string, string>();
        msg.Add("method", "didLoadScene");
        msg.Add("name", scene.name);

        string info = JsonConvert.SerializeObject(msg);

        UnityMessageManager.Instance.SendMessageToFlutter(info);
    }

    // 处理 Flutter 消息的方法
    public void LoadScene(string message)
    {
        Debug.Log("Received message from Flutter: " + message); // 处理接收到的消息

        if (message != null)
        {
            Debug.Log("Received scene name: " + message);

            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                // 确保只卸载已加载的场景
                if (scene.isLoaded && scene.name != "BaseScene")
                {
                    StartCoroutine(UnloadSceneSafely(scene.name));
                    // SceneManager.UnloadSceneAsync(scene.name); // 异步卸载场景
                }
            }

            if (message == "BaseScene")
            {
                Debug.Log("--- unity current is BaseScene");
                // 确保BaseScene已加载
                if (!SceneManager.GetSceneByName("BaseScene").isLoaded)
                {
                    Debug.Log("--- unity has no BaseScene");
                    SceneManager.LoadScene("BaseScene", LoadSceneMode.Single);
                }
                return;
            }
            
            SceneManager.LoadScene(message, LoadSceneMode.Additive);
        }
    }
    
    IEnumerator UnloadSceneSafely(string sceneName)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        yield return asyncUnload;
        
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log("Scene and related resources unloaded successfully.");
    }
}