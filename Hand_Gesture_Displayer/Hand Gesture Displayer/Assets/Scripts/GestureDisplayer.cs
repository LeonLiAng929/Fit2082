/*
 Leon (Ang) Li, Bachelor of Computer Science(Advanced).
    Monash University Australia
    Wellington Rd, Clayton VIC 3800, Australia
    alii0017@student.monash.edu
 Developed for FIT2082 project 'AR Hand Gesture Capture for Interactive Data Analytics' 
 Supervised by:
 Barrett Ens  
    Monash University Australia
    Wellington Rd, Clayton VIC 3800, Australia
    barrett.ens@monash.edu
 Max Cordeil
    Monash University Australia
    Wellington Rd, Clayton VIC 3800, Australia
    max.cordeil@monash.edu
 
 This class handles all instances of Gesture, which correspond to .csv files recorded by the web app 
 and playbacks the gesture data during runtime.
 User can manage these gesture data with different categories.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestureDisplayer : MonoBehaviour
{
    [System.Serializable]
    public class GesturePool
    {
        /*
         * A gesture pool is used to categorize gestures elicited.
         * User will need to populate the .csv files recorded by the webapp before running the unity app.
         */ 
        public string tag;
        public GameObject handModel;
        public TextAsset[] CsvData;  // an array of csv files recorded from people for one particular gesutrue.
        public Vector3[] PositionFactor;
    }

    public Transform handModelsContainer;
    public Transform gestureTagsContainer;
    public Text userInputTag;
    public GameObject gestureTagTemplate;
    private string previousInput = null;
   
    #region Singleton
    public static GestureDisplayer instance;
    #endregion
    private void Awake()
    {
        instance = this;
        poolDic = new Dictionary<string, List<Gesture>>();

        foreach (GesturePool pool in pools)
        {
            if (pool.PositionFactor.Length != pool.CsvData.Length)
            {
                Debug.LogWarning("The number of position factors must be the same as the number of .csv files!");
                return;
            }

            List<Gesture> geturePool = new List<Gesture>();
            for (int i = 0; i < pool.CsvData.Length; i++)
            {
                GameObject hand = Instantiate(pool.handModel, handModelsContainer);
                GameObject gestureTag = Instantiate(gestureTagTemplate, gestureTagsContainer);
                gestureTag.SetActive(false);
                hand.SetActive(false);
                gestureTag.GetComponent<TMP_Text>().text = pool.tag + i.ToString();
                Gesture gesture = new Gesture();
                gesture.SetHand(hand);
                gesture.SetTag(gestureTag);
                gesture.SetCSV(pool.CsvData[i]);
                gesture.PositionUpdate(pool.PositionFactor[i]);
                geturePool.Add(gesture);
            }
            poolDic.Add(pool.tag, geturePool);
        }

        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {
                gesture.prepare();
            }
        }
    }
    
    public List<GesturePool> pools;
    public Dictionary<string, List<Gesture>> poolDic;

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value){
                gesture.ConditionCheck();
            }
        }
    }
    
    void FixedUpdate()
    {
        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {
                gesture.Animate();
            }
        }
    }

    /// <summary>
    /// Draws the bounding box for a gesture
    /// </summary>
    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {
                Gizmos.color = Color.red;
                if (gesture.GetHand().activeSelf)
                {
                    Gizmos.DrawWireCube(gesture.GetCentroid() + gesture.GetPositionFactor(), gesture.GetBoundingBoxSize());
                }
            }
        }
    }

    /// <summary>
    /// Displays a category of gestures and their tag.
    /// </summary>
    public void DisplayGesture()
    {
        previousInput = userInputTag.text;
        foreach (Gesture gesture in poolDic[userInputTag.text])
        {
            gesture.GetHand().SetActive(true);
            gesture.getTag().SetActive(true);
        }
    }

    /// <summary>
    /// Hides a category of gestures and their tag.
    /// </summary>
    public void HideGesture()
    {
        if(previousInput != null) { 
            foreach (Gesture gesture in poolDic[previousInput])
            {
                gesture.GetHand().SetActive(false);
                gesture.getTag().SetActive(false);
            }
        }
    }
}
