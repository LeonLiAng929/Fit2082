using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureDisplayer : MonoBehaviour
{
    [System.Serializable]
    public class GesturePool
    {
        public string tag;
        public GameObject handModel;
        public TextAsset[] CsvData;  // an array of csv files recorded from people for one particular gesutrue.
    }

    #region Singleton
    public static GestureDisplayer instance;
    private void Awake()
    {
        instance = this;
        poolDic = new Dictionary<string, List<Gesture>>();

        foreach (GesturePool pool in pools)
        {
            List<Gesture> geturePool = new List<Gesture>();
            for (int i = 0; i < pool.CsvData.Length; i++)
            {
                GameObject hand = Instantiate(pool.handModel);
                hand.SetActive(true);
                Gesture gesture = new Gesture();
                gesture.SetHand(hand);
                gesture.SetCSV(pool.CsvData[i]);
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
    #endregion
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
}
