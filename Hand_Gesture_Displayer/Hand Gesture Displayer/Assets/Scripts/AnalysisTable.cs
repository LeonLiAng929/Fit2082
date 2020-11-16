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

 This class displays results of an euclidean distance analysis in the scene.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TableEntry
{
    public double euclideanDistance;
    public string gesturePair;
}

public class AnalysisTable : MonoBehaviour
{
    private List<TableEntry> tableEntries;
    private List<Transform> entryTransforms = new List<Transform>();
    public Transform entries;
    public Transform entryTemplate;
    public TMP_Text sum;
    public TMP_Text avg;

    private void Awake()
    {
        entryTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sorts euclidean distances among pairs of gestures under one category in decending order.
    /// </summary>
    public void Sort()
    {
        // Sort entry list by Euclidean distance
        for (int i = 0; i < tableEntries.Count; i++)
        {
            for (int j = i + 1; j < tableEntries.Count; j++)
            {
                if (tableEntries[j].euclideanDistance > tableEntries[i].euclideanDistance)
                {
                    // Swap
                    TableEntry tmp = tableEntries[i];
                    tableEntries[i] = tableEntries[j];
                    tableEntries[j] = tmp;
                }
            }
        }
    }

    /// <summary>
    /// Sets tableEntries to the given results.
    /// </summary>
    public void UpdateResult(List<TableEntry> results)
    {
        tableEntries = results;
    }

    /// <summary>
    /// Displays result of an analysis in the analysis table in the scene.
    /// </summary>
    public void GenerateReport(double euclideanSum)
    {
        sum.text = "Sum: " + euclideanSum.ToString();
        avg.text = "Avg: " + (euclideanSum / tableEntries.Count).ToString();

        if (entryTransforms.Count != 0)
        {
            foreach (Transform entrytransform in entryTransforms)
            {
                Destroy(entrytransform.gameObject);
            }
            entryTransforms = new List<Transform>();
        }
        foreach (TableEntry entry in tableEntries)
        {
            CreateEntryTransform(entry, entries, entryTransforms);
        }
    }

    /// <summary>
    /// Spawns a entry in the analysis table in the scene and put it to a proper place in the table.
    /// </summary>
    private void CreateEntryTransform(TableEntry entry, Transform container, List<Transform> transformList)
    {
        float templateHeight = 31f;
        Transform newEntry = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = newEntry.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        newEntry.gameObject.SetActive(true);

        double distance = entry.euclideanDistance;

        newEntry.Find("EuclideanDistanceText").GetComponent<TMP_Text>().text = distance.ToString();

        string gesturePair = entry.gesturePair;
        newEntry.Find("PairText").GetComponent<TMP_Text>().text = gesturePair;

        transformList.Add(newEntry);
    }
}
