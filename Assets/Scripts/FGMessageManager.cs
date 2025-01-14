using System.Collections;
using System.Collections.Generic;
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

    // 处理 Flutter 消息的方法
    public void HandleFlutterMessage(string message)
    {
        Debug.Log("Received message from Flutter: " + message); // 处理接收到的消息

        if (message != null)
        {
            Debug.Log("Received scene name: " + message);

            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != "BaseScene")
                {
                    SceneManager.UnloadSceneAsync(scene.name);
                }
            }

            if (message != "BaseScene")
            {
                SceneManager.LoadScene(message, LoadSceneMode.Additive);
            }
        }
        
        //MessageData messageData = JsonUtility.FromJson<MessageData>(message);
        // Debug.Log("ID: " + messageData.methodName + ", Name: " + messageData.sceneName);
        
        // if (messageData.methodName == "LoadScene")
        // {
        //     SceneManager.LoadScene(messageData.sceneName);
        // } else if (messageData.methodName == "Quit")
        // {
        //     SceneManager.LoadScene("BaseScene");
        // }
    }
    
    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
