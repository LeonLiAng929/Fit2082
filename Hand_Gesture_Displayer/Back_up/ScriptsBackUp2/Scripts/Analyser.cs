using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Analyser : MonoBehaviour
{
    public GestureDisplayer gestureDisplayer;

    // Start is called before the first frame update
    void Start()
    {
       // Debug.Log(EuclideanDistancePoolwise("Zoom"));
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Computes the Euclidean distance between two points
    /// since the data received about z-axis is fixed all the time, the computation for it has been excluded .
    /// </summary>
    public double EuclideanDistancePointwise(Vector3 point1, Vector3 point2)
    {
        return Math.Sqrt((point1.x - point2.x) * (point1.x - point2.x) + (point1.y - point2.y) * (point1.y - point2.y));
    }


    /// <summary>
    /// Computes the Euclidean distance between two getures 
    /// as the sum of the Euclidean distances between their corresponding joints.
    /// since the data received about z-axis is fixed all the time, the computation for it has been excluded .
    /// </summary>
    public double EuclideanDistanceGesturewise(Gesture gesture1, Gesture gesture2)
    {
        Vector3[] g1;
        Vector3[] g2;
        double sum = 0;
        for (int i = 1; i <= 2; i++) // 30: number of frames we extract from data.
        {
            g1 = gesture1.RawDataToCoordinate(gesture1.GetCurrentRow(i));
       
            g2 = gesture2.RawDataToCoordinate(gesture2.GetCurrentRow(i));

            for (int j = 0; j < g1.Length; j++)
            {
                sum += EuclideanDistancePointwise(g1[i], g2[i]);
            }
        }
        return sum;
    }

    /// <summary>
    /// Computes the sum of Euclidean distance among all combination of two getures in a gesture pool. 
    /// as the sum of the Euclidean distances between their corresponding joints.
    /// since the data received about z-axis is fixed all the time, the computation for it has been excluded .
    /// </summary>
    public double EuclideanDistancePoolwise(string tag)
    {
  
        List<Gesture> gestures;
       /* if (GestureDisplayer.instance.poolDic.TryGetValue(tag, out gestures))
        {
            Debug.LogWarning(tag + " does not exist!");
            return -1;
        }*/
        double sum = 0;
        gestures = gestureDisplayer.poolDic[tag]; 
        for (int i = 0; i < gestures.Count - 1; i++)
        {
            for (int j = i + 1; j < gestures.Count; j++)
            {
                sum += EuclideanDistanceGesturewise(gestures[i], gestures[j]);
            }
        }
        return sum;
    }

}
