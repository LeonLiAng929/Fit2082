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
 
 Each instance of this class corresponds a .csv file recorded from the webapp.

 The implementation of Resample, Rescale, TranslateToOrigin under Gesture intends to realize 
 the 3 preposessing steps proposed by Radu-Daniel Vatavu in his Dissimilarity-Consensus Approach
 to Agreement Analysis in Gesture Elicitation Studies.

 The academic publication for it is:
    Radu-Daniel Vatavu. (2019).
    The Dissimilarity-Consensus Approach to Agreement Analysis in Gesture Elicitation Studies.
    Proceedings of CHI '19, the 37th ACM Conference on Human Factors in Computing Systems (Glasgow, Scottland, UK). 
    New York, NY, USA: ACM Press, Paper 224
    DOI: https://doi.org/10.1145/3290605.3300454
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Data
{
    public Vector3[] jointsCoordinate;
    public float timestamp; 
}
public class BoundingBox
{
    public float maxX = float.NegativeInfinity;
    public float minX = float.PositiveInfinity;
    public float maxY = float.NegativeInfinity;
    public float minY = float.PositiveInfinity;
    public float maxZ = float.NegativeInfinity;
    public float minZ = float.PositiveInfinity;
}
public class Gesture
{
    private string[] raw_data_read; // contains an empty row in the end somehow, so the index of the last valid row would be data_read[length -2]
    private int num_of_rows;
    private int row_count = 0; // cooreponding to the rows in the original .csv file, in each update function, data at data_read[row_count] is used for lerp. 
    private float timer = 0.0f; // the time since start() in seconds.
    private float init_timestamp; // the very first timestamp registered in the .csv file, in miliseconds, will be treated as the start time.
    private float csv_time; // time in miliseconds, used to compare with the system time during runtime
    private float last_timestamp; // the very last time registered in the .csv file, same as the duration of the gesture.
    private float pre_timestamp; // previous timestamp recorded in the .csv file.
    private int init = 1;
    private GameObject gestureTag;
    private GameObject hand;
    private TextAsset motion_data;
    private Vector3 position_factor = new Vector3(0,0,0);// will be used to adjust the postion of the hand on the screen.
    private Data[] processed_data; // data after processed.
    private Transform[] transforms; // coordinates of the hand model
    private Transform[] init_transforms; // the very first coordinates of the hand model. 
    private Vector3[] csv_coordinates; 
    private Vector3[] init_coordinates; // the very first row of coordinates, which will be treaded as the origin for the correspoding joint.
    private BoundingBox boundingBox;
    private Vector3 centroid;
    private Vector3 rescaleReference = new Vector3(300, 300, 35); // the data will have a bounding box of size (300,300,35) after re-scaling

    /// <summary>
    /// Assign a hand model that will animate this gesture in the scene.
    /// </summary>
    public void SetHand(GameObject hand)
    {
        this.hand = hand;
    }

    /// <summary>
    /// Return the hand model assigned to this gesture.
    /// </summary>
    public GameObject GetHand()
    {
        return hand;
    }

    /// <summary>
    /// Set a tag in the scene that will appear below the hand model. Format of this tag can be "Export1","Zoom0"...etc.
    /// </summary>
    public void SetTag(GameObject tag)
    {
        gestureTag = tag;
    }

    /// <summary>
    /// Get the tag that is assigned to this gesture.
    /// </summary>
    public GameObject getTag()
    {
        return gestureTag;
    }

    /// <summary>
    /// set the .csv file which will be used to animate the hand model during runtime.
    /// </summary>
    public void SetCSV(TextAsset text)
    {
        this.motion_data = text;
    }

    /// <summary>
    /// This function makes the .csv file ready to play in Unity
    /// It first converts the raw data in .csv file to a more usable customized type 'Data'.
    /// It then preprocesses the data according to the 3 steps specified in the Dissimilarity-Concensus method:
    ///     1.Height normalization (rescaling the gesture so that the hand size of all participants will be the same)
    ///         -Makes the dissimilarity values independent of participants’ hand sizes
    ///     2.Translation to origin(for each gesture, subtract the centroid from each joint so that the new centroid becomes(0,0,0))
    ///         -Makes the dissimilarity values independent of where the gesture is produced in space.
    ///     Note that the third step - resampling will not be performed in this function. Data will be resampled only when
    ///     an analysis is to perform on it.
    /// After that, sets the initial position for each joint of the hand model in the scene. 
    /// </summary>
    public void prepare()
    {
        raw_data_read = motion_data.text.Split('\n'); // read in the data and save it to data_read
        num_of_rows = raw_data_read.Length - 2;
        processed_data = new Data[num_of_rows];
        transforms = hand.GetComponentsInChildren<Transform>(); // {parent_object, joint1, joint2...}
        init_transforms = transforms;

        for (int i = 1; i < raw_data_read.Length - 1; i++)
        {
            Data temp = new Data();
            temp.timestamp = float.Parse(GetCurrentRow(i)[0]);
            temp.jointsCoordinate = RawDataToCoordinate(GetCurrentRow(i));
            processed_data[i - 1] = temp;
        }

        SetBoundingBox();
        SetCentroid();
        TranslateToOrigin();
        Rescale();

        gestureTag.transform.localPosition = centroid - position_factor - new Vector3(0, rescaleReference.y/2,0);

        init_coordinates = processed_data[0].jointsCoordinate;

        init_timestamp = processed_data[0].timestamp;
        last_timestamp = float.Parse(GetCurrentRow(raw_data_read.Length - 2)[0]) - init_timestamp;
        timer = 0f;
        csv_time = GetRelativeTimeStamp(processed_data[0].timestamp);
        Update_row();

        transforms[0].localPosition = new Vector3(0, 0, 0);
        for (int i = 1; i < transforms.Length; i++)
        {
            transforms[i].localPosition = init_coordinates[i - 1] + position_factor;
        }
    }

    /// <summary>
    /// Called in Update(), update position of the hand model by regularly tracking the system timer with the current csv_time. 
    /// Enables the animation to loop.
    /// </summary>
    public void ConditionCheck()
    {
        if (SecondsToMs(timer) > last_timestamp)
        {
            transforms = init_transforms;
            timer = 0f;
            csv_time = init_timestamp;
            row_count = 1;
            init = 1;
            Update_row();
        }
        if (SecondsToMs(timer) > csv_time)
        {
            pre_timestamp = csv_time;
            Update_row();
            csv_time = GetRelativeTimeStamp(processed_data[row_count].timestamp);
            csv_coordinates = processed_data[row_count].jointsCoordinate;
        }
    }

    /// <summary>
    /// Animates the gesture in the scene based on given .csv file.
    /// </summary>
    public void Animate()
    {
        if (init == 1)
        {
            init = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }

        for (int i = 1; i < transforms.Length; i++)
        {
            Vector3 start_pos = transforms[i].localPosition;
            Vector3 end_pos = csv_coordinates[i - 1] + position_factor;
       
            float ratio = (SecondsToMs(timer) - pre_timestamp) / (csv_time - pre_timestamp);
            transforms[i].localPosition = Vector3.Lerp(start_pos, end_pos, ratio);
        }
    }

    /// <summary>
    /// Gets a row in the .csv file based on the index.
    /// format of a .csv file can be:
    /// row 0: Legends
    /// row 1: timestamp, joint_0_x, joint_0_y, joint_0_z...etc
    /// ...
    /// </summary>
    public string[] GetCurrentRow(int row_index)
    {
        return raw_data_read[row_index].Split(',');
    }

    /// <summary>
    /// get the timestamp relative to init_timestamp
    /// </summary>
    float GetRelativeTimeStamp(float raw_time) 
    {
        return raw_time - init_timestamp;
    }

    /// <summary>
    /// Converts time in seconds to miliseconds
    /// </summary>
    float SecondsToMs(float time)
    {
        return time * 1000;
    }

    /// <summary>
    /// Convert a row of raw data in .csv to Vector3 coordinates
    /// </summary>
    public Vector3[] RawDataToCoordinate(string[] raw_data) // raw_data conatains { timestamp, joint1_x, joint1_y, joint1_z, joint2_x.....}, which represents a row read from the data.
    {
        int num_of_xyz = (raw_data.Length - 1) / 3;
        Vector3[] coordinates = new Vector3[num_of_xyz];
        int array_ptr = 0;
        for (int i = 1; i < raw_data.Length; i += 3)
        {
            coordinates[array_ptr] = new Vector3(float.Parse(raw_data[i]), float.Parse(raw_data[i + 1]), float.Parse(raw_data[i + 2]));
            array_ptr += 1;
        }
        return coordinates;
    }

    /// <summary>
    /// increments row_count by 1
    /// </summary>
    void Update_row()
    {
        row_count += 1;
    }

    /// <summary>
    /// Resamples a gesture into a fixed number of n Data uniformly spaced in time.
    /// </summary>
    public List<Data> Resample(int n)
    {
        List<Data> set = new List<Data>();
        float I = last_timestamp / (n - 1);
       
        set.Add(processed_data[0]);
        for (int i = 1; i < processed_data.Length; i++)
        {
            float timeDiff = processed_data[i].timestamp - set[set.Count - 1].timestamp;
            while (timeDiff >= I)
            {
                // interpolate two rows of data
                float t = I / timeDiff;
                Data temp = new Data();
                temp.jointsCoordinate = new Vector3[processed_data[i].jointsCoordinate.Length];
                //BodyPose posture = new BodyPose();
                for (int j = 0; j < processed_data[i].jointsCoordinate.Length; j++)
                {
                    Vector3 interpolatedCoordinates;
                    interpolatedCoordinates.x = (1 - t) * set[set.Count - 1].jointsCoordinate[j].x + t * processed_data[i].jointsCoordinate[j].x;
                    interpolatedCoordinates.y = (1 - t) * set[set.Count - 1].jointsCoordinate[j].y + t * processed_data[i].jointsCoordinate[j].y;
                    interpolatedCoordinates.z = (1 - t) * set[set.Count - 1].jointsCoordinate[j].z + t * processed_data[i].jointsCoordinate[j].z;
                    temp.jointsCoordinate[j] = interpolatedCoordinates;
                }
                    
                temp.timestamp = (1 - t) * set[set.Count - 1].timestamp + t * processed_data[i].timestamp;
                set.Add(temp);
                timeDiff -= I;
            }
        }
        if (set.Count == n - 1)
            set.Add(processed_data[processed_data.Length - 1]);
        return set;
    }

    /// <summary>
    /// Updates the position factor, which is used to adjust the postion of hand model in the scene
    /// </summary>
    public void PositionUpdate(Vector3 factor)
    {
        position_factor = factor;
    }

    /// <summary>
    /// Gets the position factor.
    /// </summary>
    public Vector3 GetPositionFactor()
    {
        return position_factor;
    }

    /// <summary>
    /// Calculates the bounding box based on data in .csv file.
    /// </summary>
    public void SetBoundingBox()
    {
        boundingBox = new BoundingBox();
        foreach (Data d in processed_data)
        {
            foreach (Vector3 coordinate in d.jointsCoordinate)
            {
                if (coordinate.x > boundingBox.maxX)
                {
                    boundingBox.maxX = coordinate.x;
                }
                if (coordinate.x < boundingBox.minX)
                {
                    boundingBox.minX = coordinate.x;
                }
                if (coordinate.y > boundingBox.maxY)
                {
                    boundingBox.maxY = coordinate.y;
                }
                if (coordinate.y < boundingBox.minY)
                {
                    boundingBox.minY = coordinate.y;
                }
                if (coordinate.z > boundingBox.maxZ)
                {
                    boundingBox.maxZ = coordinate.z;
                }
                if (coordinate.z < boundingBox.minZ)
                {
                    boundingBox.minZ = coordinate.z;
                }
            }
        }
    }

    /// <summary>
    /// Returns the calculated bounding box 
    /// </summary>
    public BoundingBox GetBoundingBox()
    {
        return boundingBox;
    }

    /// <summary>
    /// Calculates and returns the size of the bounding box.
    /// </summary>
    public Vector3 GetBoundingBoxSize()
    {
        return new Vector3(boundingBox.maxX - boundingBox.minX, boundingBox.maxY - boundingBox.minY, boundingBox.maxZ - boundingBox.minZ); 
    }

    /// <summary>
    /// Returns centroid of this recorded gesture
    /// </summary>
    public Vector3 GetCentroid()
    {
        return centroid;
    }

    /// <summary>
    /// Calculates the centroid for the recorded gesture based on its bounding box
    /// </summary>
    public void SetCentroid() {
        Vector3 size = GetBoundingBoxSize();
        centroid = new Vector3(size.x / 2 + boundingBox.minX, size.y / 2 + boundingBox.minY, size.z / 2 + boundingBox.minZ);
    }

    /// <summary>
    /// Translates the gesture so that its centroid becomes (0, 0, 0).
    /// </summary>
    public void TranslateToOrigin()
    {
        Vector3 centroid = GetCentroid();
        foreach (Data data in processed_data) { 
            for (int i =0; i < data.jointsCoordinate.Length; i++)
            {
                data.jointsCoordinate[i].x -= centroid.x;
                data.jointsCoordinate[i].y -= centroid.y;
                data.jointsCoordinate[i].z -= centroid.z;
            }
        }
        SetBoundingBox();
        SetCentroid();
    }

    /// <summary>
    /// Rescales the recorded gesture based on rescaleReference.
    /// Z coordinate is commented out because the data recorded does not reflect the acutal movement of hand along z-axis.
    /// uncomment rescale for z coordinate if this is fixed in the future.
    /// </summary>
    public void Rescale()
    {
        Vector3 size = GetBoundingBoxSize();
        float xScale = rescaleReference.x / size.x;
        float yScale = rescaleReference.y / size.y;
        //float zScale = size.z / rescaleReference.z;

        foreach (Data data in processed_data)
        {
            for (int i = 0; i < data.jointsCoordinate.Length; i++)
            {
                data.jointsCoordinate[i].x = data.jointsCoordinate[i].x * xScale;
                data.jointsCoordinate[i].y = data.jointsCoordinate[i].y * yScale;
                //data.jointsCoordinate[i].z = data.jointsCoordinate[i].z * zScale;
            }
        }
        SetBoundingBox();
        SetCentroid();
    }

    /// <summary>
    /// Gets coordinates relative to the first row of coordinates recorded.
    /// This was used when re-scale and tranlate to origin were not implemented
    /// I'd leave this function here just in case it might be useful in the future. 
    /// </summary>
    public void GetRelativeCoordinates(List<Data> coordinates)
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            for (int j = 0; j < coordinates[i].jointsCoordinate.Length; j++)
            {
                coordinates[i].jointsCoordinate[j].x -= init_coordinates[i].x;
                coordinates[i].jointsCoordinate[j].y -= init_coordinates[i].y;
                coordinates[i].jointsCoordinate[j].z -= init_coordinates[i].z;
            }

        }
    }
}

