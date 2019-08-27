using UnityEngine;

public class MessageEventManager : MonoBehaviour {

    public MessageViewController controller;

    private static MessageEventManager eventManager;

    public static MessageEventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(MessageEventManager)) as MessageEventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
            }

            return eventManager;
        }
    }

    public static void addNewMessage(string message)
    {
        instance.controller.addNewMessage(message);
    }
}
