using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Csv_Animator : MonoBehaviour
{
    public GameObject hand;
    public TextAsset motion_data;
    public float x, y, z;
    private string[] data_read; // contains an empty row in the end somehow, so the index of the last valid row would be data_read[length -2]
    private float timer = 0.0f; // the time since start() in seconds.
    private float csv_time;
    private string[] csv_row;
    private Transform[] transforms;
    private Transform[] init_transforms;
    private float init_timestamp; // the very first timestamp registered in the .csv file, in miliseconds, will be treated as the start time.
    private Vector3[] csv_coordinates;
    private Vector3[] init_coordinates; // the very first row of coordinates, which will be treaded as the origin for the correspoding joint.
    private int row_count = 1; // cooreponding to the rows in the original .csv file, in each update function, data at data_read[row_count] is used for lerp. 
    private float last_timestamp;
    private float pre_timestamp;
    private int init = 1;

    void Start()
    {
        data_read = motion_data.text.Split('\n'); // read in the data and save it to data_read
        //List<Transform> coordinates = new List<Transform> (hand.GetComponentsInChildren<Transform>());
        transforms = hand.GetComponentsInChildren<Transform>(); // {hand, joint1, joint2...}
        init_transforms = transforms;
        string[] row1 = GetCurrentRow(row_count);

        
        init_coordinates = RawDataToCoordinate(row1);
        for (int i = 0; i < init_coordinates.Length; i++){
            init_coordinates[i].x += x;
            init_coordinates[i].y += y;
            init_coordinates[i].z += z;
        }


        init_timestamp = float.Parse(row1[0]);
        last_timestamp = float.Parse(GetCurrentRow(data_read.Length - 2)[0]) - init_timestamp;
        timer = 0f;
        csv_time = GetTimeStamp(row1);
        Update_row();

        transforms[0].position = new Vector3(0, 0, 0);


       /* for (int i = 0; i < init_coordinates.Length; i++)
        {
            Debug.Log(i.ToString() + init_coordinates[i]);
        }*/
        for (int i = 1; i < transforms.Length; i++)
        {
            transforms[i].position = init_coordinates[i - 1];
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(timer);
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
            csv_row = GetCurrentRow(row_count);
            csv_time = GetTimeStamp(csv_row);
            csv_coordinates = RawDataToCoordinate(csv_row);
        }
    }
    void FixedUpdate()
     {
       if(init == 1)
        {
            init = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
      
       for (int i =1; i < transforms.Length; i++)
        {
            Vector3 start_pos = transforms[i].position;
            Vector3 end_pos = csv_coordinates[i - 1];
            float ratio = (SecondsToMs(timer) - pre_timestamp) / (csv_time - pre_timestamp);
            transforms[i].position = Vector3.Lerp(start_pos, end_pos, ratio);
        }
     }


    string[] GetCurrentRow(int row_index)
    {
        return data_read[row_index].Split(',');
    }

    float GetTimeStamp(string[] curr_row) // get the relative timestamp when init_timestamp is treated relatively as 0.
    {
        return float.Parse(curr_row[0]) - init_timestamp;
    }

    float SecondsToMs(float time)
    {
        return time * 1000;
    }

    Vector3[] RawDataToCoordinate(string[] raw_data) // raw_data conatains { timestamp, joint1_x, joint1_y, joint1_z, joint2_x.....}, which represents a row read from the data.
    {
        int num_of_xyz = (raw_data.Length - 1) / 3;
        Vector3[] coordinates = new Vector3[num_of_xyz];
        int array_ptr = 0;
        for (int i = 1; i < raw_data.Length; i += 3)
        {
            coordinates[array_ptr] = new Vector3(float.Parse(raw_data[i]) + x, float.Parse(raw_data[i + 1]) + y, float.Parse(raw_data[i + 2]) + z);
            array_ptr += 1;
        }
        return coordinates;
    }


    Vector3[] GetRelativeCoordinates(Vector3[] coordinates)
    {
        for (int i = 0; i < coordinates.Length; i++)
        {
            coordinates[i].x -= init_coordinates[i].x;
            coordinates[i].y -= init_coordinates[i].y;
            coordinates[i].z -= init_coordinates[i].z;

        }
        return coordinates;
    }


    void Update_row()
    {
        row_count += 1;
    }

    void readCSV()
    {
        string[] data = motion_data.text.Split('\n');
        for (int i = 0; i < data.Length; i++)
        {
            string[] fields = data[i].Split(',');
            //GameObject[] joints = hand.GetComponentsInChildren<GameObject>();

            //Vector3 cooridinates;

            //Vector3.Lerp()
            data_read[i].Split();

            //hand.transform.localPosition = new Vector3(float.Parse(fields[1]), float.Parse(fields[2]), float.Parse(fields[3]));
        }
    }
}
