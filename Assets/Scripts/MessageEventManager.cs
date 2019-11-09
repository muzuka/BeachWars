using UnityEngine;

public class MessageEventManager : MonoBehaviour {

    public MessageViewController Controller;

    private static MessageEventManager _eventManager;

    public static MessageEventManager Instance
    {
        get
        {
            if (!_eventManager)
            {
                _eventManager = FindObjectOfType(typeof(MessageEventManager)) as MessageEventManager;

                if (!_eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
            }

            return _eventManager;
        }
    }

    public static void AddNewMessage(string message)
    {
        Instance.Controller.AddNewMessage(message);
    }
}
