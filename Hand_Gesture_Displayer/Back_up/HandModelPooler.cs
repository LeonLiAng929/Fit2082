using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandModelPooler : MonoBehaviour
{
    [System.Serializable]
    public class GesturePool
    {
        public string tag;
        public GameObject handModel;
        public TextAsset[] CsvData;  // an array of csv files recorded from people for one particular gesutrue.
    }

    #region Singleton
    public static HandModelPooler instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    public List<GesturePool> pools;
    public Dictionary<string, Queue<GameObject>> poolDic;

    // Start is called before the first frame update
    void Start()
    {
        poolDic = new Dictionary<string, Queue<GameObject>>();

        foreach (GesturePool pool in pools)
        {
            Queue<GameObject> handPool = new Queue<GameObject>();
            for (int i =0; i < pool.CsvData.Length; i++)
            {
                GameObject hand = Instantiate(pool.handModel);
                hand.SetActive(false);
                handPool.Enqueue(hand);
            }
            poolDic.Add(pool.tag, handPool);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
