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
            for (int j = 0; j < g1[i].jointsCoordinate.Length; j++)
            {
                sum += EuclideanDistancePointwise(g1[i].jointsCoordinate[j], g2[i].jointsCoordinate[j]);
            }
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
    /// Invokes methods from AnalysisTable to perform an analysis on a pool of gesture.
    /// Computes the sum of Euclidean distance among all combination of pairs of getures,
    /// sorts them in descending order and displayes the results on the analysis table in the scene.
    /// </summary>
    public void Analyse()
    {
        double sum = EuclideanDistancePoolwise(gesturePoolTag);
        table.GetComponent<AnalysisTable>().Sort();
        table.GetComponent<AnalysisTable>().GenerateReport(sum);
        table.SetActive(true);
    }
}
