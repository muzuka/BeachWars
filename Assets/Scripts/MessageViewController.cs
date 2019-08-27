using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageViewController : MonoBehaviour {

    public float timeToDisappear;

    public Text textRef;

    Queue<Text> messages;

    bool isVisible;
    float timeConsumed;

    // Use this for initialization
    void Start () {
        messages = new Queue<Text>();
        timeConsumed = 0f;
        gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (isVisible)
        {
            timeConsumed += Time.deltaTime;
            if (timeConsumed > timeToDisappear)
            {
                isVisible = false;
                gameObject.SetActive(false);
                timeConsumed = 0f;
                foreach(Text message in messages)
                {
                    Destroy(message.gameObject);
                }
                messages.Clear();
            }
        }
	}

    public void addNewMessage(string message)
    {
       
        Text text = Instantiate(textRef, transform);
        text.transform.Translate(0.0f, -text.GetComponent<RectTransform>().rect.height * messages.Count, 0.0f);
        text.text = message;
        messages.Enqueue(text);

        isVisible = true;
        gameObject.SetActive(true);
        gameObject.SetActive(true);
        timeConsumed = 0f;
    }
}
