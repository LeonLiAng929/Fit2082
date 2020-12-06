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

 This Analyser class implements a dissimilarity function (euclidian distance)
 that computes a real and positive number to determine the extent of difference
 between two elicited gestures in performing a function(e.g. zoom, pan...). 
 Note that to get a valid analysis, there must be at least two elicited gestures under one category(function).
 In future after Dynamic time warping algorithm is complete, it will eventually be 
 able to conduct analysis based on the Dissimilarity-Consensus Approach 
 proposed by Radu-Daniel Vatavu.

 The academic publication for it is:
    Radu-Daniel Vatavu. (2019).
    The Dissimilarity-Consensus Approach to Agreement Analysis in Gesture Elicitation Studies.
    Proceedings of CHI '19, the 37th ACM Conference on Human Factors in Computing Systems (Glasgow, Scottland, UK). 
    New York, NY, USA: ACM Press, Paper 224
    DOI: https://doi.org/10.1145/3290605.3300454
 */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Analyser : MonoBehaviour
{
    public GestureDisplayer gestureDisplayer;
    public int resample_size;
    public GameObject table;
    public Text input;
    public Text tolerance;
    private string gesturePoolTag;

    /// <summary>
    /// Computes the Euclidean distance between two points
    /// since the data received about z-axis is fixed all the time, the computation for it has been excluded.
    /// If the issue with z-axis is resolved in the future, modify this function by adding 
    /// calculation regarding z-axis.
    /// </summary>
    public double EuclideanDistancePointwise(Vector3 point1, Vector3 point2)
    {
        return Math.Sqrt((point1.x - point2.x) * (point1.x - point2.x) + (point1.y - point2.y) * (point1.y - point2.y));
    }

    /// <summary>
    /// Computes the Euclidean distance between two sets of joints
    /// Since the data received about z-axis is fixed all the time, the computation for it has been excluded .
    /// </summary>
    public double EuclideanDistanceJointswise(Vector3[] jointsCoordinate1, Vector3[] jointsCoordinate2)
    {
        double sum = 0;

        for (int i = 0; i < jointsCoordinate1.Length; i++)
        {
            sum += EuclideanDistancePointwise(jointsCoordinate1[i], jointsCoordinate2[i]);
            
        }
        return sum / 21; // divided by 21 to make the result invariant to the number of points (21) the web app tracks
    }

    /// <summary>
    /// Computes the Euclidean distance between two getures 
    /// as the sum of the Euclidean distances between their corresponding joints.
    /// Since the data received about z-axis is fixed all the time, the computation for it has been excluded .
    /// </summary>
    public double EuclideanDistanceGesturewise(Gesture gesture1, Gesture gesture2)
    {
        List<Data> g1;
        List<Data> g2;
        double sum = 0;
        g1 = gesture1.Resample(resample_size);
        
        g2 = gesture2.Resample(resample_size);
        

        for (int i = 0; i < resample_size ; i++)
        {
            sum += EuclideanDistanceJointswise(g1[i].jointsCoordinate, g2[i].jointsCoordinate);
        }
        return sum;
    }

    /// <summary>
    /// Computes the sum of Euclidean distance among all combination of pairs of getures in a gesture pool. 
    /// as the sum of the Euclidean distances between their corresponding joints.
    /// since the data received about z-axis is fixed all the time, the computation for it has been excluded .
    /// </summary>
    public double EuclideanDistancePoolwise(string tag)
    {
  
        List<Gesture> gestures;
        List<TableEntry> entries = new List<TableEntry>();
        /*if (gestureDisplayer.poolDic.TryGetValue(tag, out gestures))
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
                double distance = EuclideanDistanceGesturewise(gestures[i], gestures[j]);
                sum += distance;

                // prepare results to be shown in the analysis table.
                TableEntry entry = new TableEntry();
                entry.euclideanDistance = distance;
                entry.gesturePair = (tag + i.ToString() + " | " + tag+j.ToString());
                entries.Add(entry);
            }
        }
        table.GetComponent<AnalysisTable>().UpdateResult(entries);
        return sum;
    }

    /// <summary>
    /// Get the Pool Tag specified by user from the input field in the scene. 
    /// </summary>
    public void GetTag() {
        gesturePoolTag = input.text;
    }

    /// <summary>
    /// Computes the DTW distance between two gestures.
    /// </summary>
    public double DTW_Gesturewise(Gesture gesture1, Gesture gesture2)
    {
        Data[] g1 = gesture1.GetProcessedData();
        Data[] g2 = gesture2.GetProcessedData();
        int n = g1.Length;
        int m = g2.Length;
        if (n == 0 || m == 0) return 0;

        double[,] cost = new double[n, m];
       
        cost[0, 0] = EuclideanDistanceJointswise(g1[0].jointsCoordinate, g2[0].jointsCoordinate);
        for (int j = 1; j < m; j++)
            cost[0, j] = cost[0, j - 1] + EuclideanDistanceJointswise(g1[0].jointsCoordinate, g2[j].jointsCoordinate);
        for (int i = 1; i < n; i++)
            cost[i, 0] = cost[i - 1, 0] + EuclideanDistanceJointswise(g1[i].jointsCoordinate,g2[0].jointsCoordinate);

        for (int i = 1; i < n; i++)
            for (int j = 1; j < m; j++)
            {
                double min = Math.Min(cost[i - 1, j - 1], Math.Min(cost[i - 1, j], cost[i, j - 1]));
                cost[i, j] = min + EuclideanDistanceJointswise(g1[i].jointsCoordinate, g2[j].jointsCoordinate);
            }
        return cost[n - 1, m - 1];
    }

    /// <summary>
    /// Computes the normalized DTW distance between two gestures.
    /// </summary>
    public double NormalizedDTW_Gesturewise(Gesture gesture1, Gesture gesture2)
    {
        Data[] g1 = gesture1.GetProcessedData();
        Data[] g2 = gesture2.GetProcessedData();
        int n = g1.Length;
        int m = g2.Length;
        if (n == 0 || m == 0) return 0;

        double[,] cost = new double[n, m];
        int[,] length = new int[n, m];

        cost[0, 0] = EuclideanDistanceJointswise(g1[0].jointsCoordinate, g2[0].jointsCoordinate);
        length[0, 0] = 1;
        for (int j = 1; j < m; j++)
        {
            cost[0, j] = cost[0, j - 1] + EuclideanDistanceJointswise(g1[0].jointsCoordinate, g2[j].jointsCoordinate);
            length[0, j] = length[0, j - 1] + 1;
        }
        for (int i = 1; i < n; i++)
        {
            cost[i, 0] = cost[i - 1, 0] + EuclideanDistanceJointswise(g1[i].jointsCoordinate, g2[0].jointsCoordinate);
            length[i, 0] = length[i - 1, 0] + 1;
        }

        for (int i = 1; i < n; i++)
            for (int j = 1; j < m; j++)
            {
                double min = cost[i - 1, j - 1];
                int l = length[i - 1, j - 1];

                if (min > cost[i - 1, j])
                {
                    min = cost[i - 1, j];
                    l = length[i - 1, j];
                }

                if (min > cost[i, j - 1])
                {
                    min = cost[i, j - 1];
                    l = length[i, j - 1];
                }

                cost[i, j] = min + EuclideanDistanceJointswise(g1[i].jointsCoordinate, g2[j].jointsCoordinate);
                length[i, j] = l + 1;
            }
        //Debug.Log("DWT" + cost[n - 1, m - 1]);
        //Debug.Log("Length" + length[n - 1, m - 1]);
        return cost[n - 1, m - 1] / length[n - 1, m - 1];
    }

    public double DTW_Poolwise(string tag, double tolerance)
    {
        List<Gesture> gestures;
        List<TableEntry> entries = new List<TableEntry>();

        gestures = gestureDisplayer.poolDic[tag];

        int count = 0;
        for (int i = 0; i < gestures.Count - 1; i++)
        {
            for (int j = i + 1; j < gestures.Count; j++)
            {
                double DTWdistance = NormalizedDTW_Gesturewise(gestures[i], gestures[j]);
                Debug.Log("Normalized DTW distance: " + DTWdistance.ToString());   // have not come up a way to display the analysis in a meaningful manner, so get it printed in the console for now.
                if (DTWdistance <= tolerance)
                {
                    count += 1;
                }
            }
        }
     
       
        Double result = (gestures.Count * (gestures.Count - 1))/2;
        result = count / result;
        result = result * 100;
        return result;
    }

    /// <summary>
    /// Invokes methods from AnalysisTable to perform an analysis on a pool of gesture.
    /// Computes the sum of Euclidean distance among all combination of pairs of getures,
    /// sorts them in descending order and displayes the results on the analysis table in the scene.
    /// </summary>
    public void Analyse()
    {
        double sum = EuclideanDistancePoolwise(gesturePoolTag);
        double similarity = DTW_Poolwise(gesturePoolTag, float.Parse(tolerance.text));
        table.GetComponent<AnalysisTable>().Sort();
        table.GetComponent<AnalysisTable>().GenerateReport(sum, similarity);
        table.SetActive(true);
    }
}
