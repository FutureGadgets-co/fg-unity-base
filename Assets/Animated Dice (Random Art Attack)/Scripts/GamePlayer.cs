using System.Collections;
using System.Collections.Generic;
using FlutterUnityIntegration;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    public GameObject user;
    public GameObject another;
    public RollJump rollJump;
    public GameObject textView;
    
    private int userValue = 0;
    private int anotherValue = 0;
    private Text textComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        user = GameObject.Find("User");
        another = GameObject.Find("Another");
        rollJump = gameObject.GetComponent<RollJump>();
        textView = GameObject.Find("ShowText");
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
                StartCoroutine(TimerCoroutine("Successful"));
            }
            else if (userValue < anotherValue)
            {  
                textComponent.text = "You lose!";
                StartCoroutine(TimerCoroutine("Failed"));
                UnityMessageManager.Instance.SendMessageToFlutter("Failed");
            }
            else
            {
                textComponent.text = "Try Again!";
                //textView.text = "Try Again!";
            }
            rollJump.onceFinished = false;
        }
    }
    
    private IEnumerator TimerCoroutine(string message)
    {
        yield return new WaitForSeconds(1);
        UnityMessageManager.Instance.SendMessageToFlutter("Successful");
    }
}

