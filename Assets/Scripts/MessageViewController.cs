using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageViewController : MonoBehaviour {

    public float TimeToDisappear;

    public Text TextRef;

    Queue<Text> _messages;

    bool _isVisible;
    float _timeConsumed;

    // Use this for initialization
    void Start() {
        _messages = new Queue<Text>();
        _timeConsumed = 0f;
        gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update() {
        if (_isVisible)
        {
            _timeConsumed += Time.deltaTime;
            if (_timeConsumed > TimeToDisappear)
            {
                _isVisible = false;
                gameObject.SetActive(false);
                _timeConsumed = 0f;
                foreach(Text message in _messages)
                {
                    Destroy(message.gameObject);
                }
                _messages.Clear();
            }
        }
	}

    public void AddNewMessage(string message)
    {
       
        Text text = Instantiate(TextRef, transform);
        text.transform.Translate(0.0f, -text.GetComponent<RectTransform>().rect.height * _messages.Count, 0.0f);
        text.text = message;
        _messages.Enqueue(text);

        _isVisible = true;
        gameObject.SetActive(true);
        gameObject.SetActive(true);
        _timeConsumed = 0f;
    }
}
