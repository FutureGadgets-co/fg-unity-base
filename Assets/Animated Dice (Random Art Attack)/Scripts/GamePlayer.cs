using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

public class GamePlayer : MonoBehaviour
{
    public GameObject user;
    public GameObject another;
    public RollJump rollJump;
    public GameObject textView;
    
    private int userValue = 0;
    private int anotherValue = 0;
    private Text textComponent;
    
    [DllImport("__Internal")]
    private static extern void unityToFlutter(string handlerName, string message);
    
    // Start is called before the first frame update
    void Start()
    {
        user = GameObject.Find("User");
        another = GameObject.Find("Another");
        rollJump = gameObject.GetComponent<RollJump>();
        textView = GameObject.Find("ShowText");
        
        if (user != null)
        {
            Debug.Log("拿到了user");
        }
        else
        {
            Debug.Log("无法获取user");
        }
        if (textView != null)
        {
            textComponent = textView.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = "You hold the black dice.\nPress anywhere to roll!";
            }
            else
            {
                Debug.LogError("Text component not found on ShowText.");
            }
        }
        else
        {
            Debug.LogError("GameObject ShowText not found in the scene.");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rollJump.onceFinished)
        {
            userValue = user.GetComponent<DiceStats>().side;
            anotherValue = another.GetComponent<DiceStats>().side;
            
            if (userValue > anotherValue)
            {
                textComponent.text = "You win!";
                StartCoroutine(TimerCoroutine(1));
            }
            else if (userValue < anotherValue)
            {  
                textComponent.text = "You lose!";
                StartCoroutine(TimerCoroutine(0));
            }
            else
            {
                textComponent.text = "Try Again!";
                //textView.text = "Try Again!";
            }
            rollJump.onceFinished = false;
        }
    }
    
    private IEnumerator TimerCoroutine(int message)
    {
        unityToFlutter("onUnityMessage", $"{{\"gameResult\":\"{message}\"}}");
        yield return new WaitForSeconds(1);
    }
}

