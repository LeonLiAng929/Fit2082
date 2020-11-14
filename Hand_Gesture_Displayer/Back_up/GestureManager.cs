using System;
using UnityEngine;

public class GestureManager : MonoBehaviour
{

    public static GestureManager instance;


    public Csv_Animator[] gestures;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Csv_Animator s in gestures)
        {
        }
    }

   /* public void Play(string gesture)
    {
        Gesture s = Array.Find(gestures, item => item.name == gesture);
        if (s == null)
        {
            Debug.LogWarning("Gesture: " + name + " not found!");
            return;
        }
    }*/

}
