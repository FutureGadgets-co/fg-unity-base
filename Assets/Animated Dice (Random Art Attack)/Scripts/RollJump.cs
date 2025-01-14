using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using UnityEngine;
using Timer = System.Timers.Timer;

public class RollJump : MonoBehaviour
{
    // This list shows what dice are going to be and can be edited via code if you want or in the inspector. 
    [SerializeField] List<GameObject> diceGroup = new List<GameObject>();
    //Set what button is pressed to make the dice jump.
    [SerializeField] float forceAmount = 400f;

    public bool onceFinished = false;
    
    
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        JumpBehavior();
    }
    void JumpBehavior()
    {
        Rigidbody rb;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        {
            onceFinished = false;
            StartCoroutine(TimerCoroutine());
            for (int i = 0; i < diceGroup.Count; i++)
            {
                diceGroup[i].transform.rotation = Random.rotation;
                rb = diceGroup[i].GetComponent<Rigidbody>();
                // all the dices can not roll out of the screen
                Vector3 position = diceGroup[i].transform.position;
                if (position.y < 0) // 检查骰子是否在屏幕下方
                {
                    position.y = 0; // 将骰子位置重置到屏幕内
                }
                if (position.x < -Screen.width / 2 + 100 || position.x > Screen.width / 2 - 100)
                {
                    position.x = 0; // 将骰子位置重置到屏幕内
                }
                rb.AddForce(Vector3.up * forceAmount);
                rb.AddTorque(new Vector3(Random.value * forceAmount, Random.value * forceAmount, Random.value * forceAmount));
            }
        }
    }
    
    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(4);
        onceFinished = true; 
        yield return new WaitForSeconds(1);
        onceFinished = false; 
    }
    
}
   
