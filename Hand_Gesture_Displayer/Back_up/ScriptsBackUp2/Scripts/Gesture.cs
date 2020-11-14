using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public Vector3[] coordinates;
    public float timestamp; 
}
public class Gesture
{
    //public GameObject hand;
    //public TextAsset motion_data;
    //public float x, y, z;
    private GameObject hand;
    private TextAsset motion_data;
    private float x, y, z = 0; //temporarily made private, these three value will be used to adjust the postion of the hand on the screen.
    private string[] raw_data_read; // contains an empty row in the end somehow, so the index of the last valid row would be data_read[length -2]
    private Data[] processed_data; // data after processed. 

    private float timer = 0.0f; // the time since start() in seconds.
    private float csv_time; // time in miliseconds
    private Transform[] transforms; // coordinates of the hand model
    private Transform[] init_transforms; // the very first coordinates of the hand model. 
    private float init_timestamp; // the very first timestamp registered in the .csv file, in miliseconds, will be treated as the start time.
    private Vector3[] csv_coordinates;
    private Vector3[] init_coordinates; // the very first row of coordinates, which will be treaded as the origin for the correspoding joint.
    private int num_of_rows;
    private int row_count = 0; // cooreponding to the rows in the original .csv file, in each update function, data at data_read[row_count] is used for lerp. 
    private float last_timestamp; // the very last time registered in the .csv file, same as the duration of the gesture.
    private float pre_timestamp;
    private int init = 1;

    public void SetHand(GameObject hand)
    {
        this.hand = hand;
    }

    public void SetCSV(TextAsset text)
    {
        this.motion_data = text;
    }

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
            temp.coordinates = RawDataToCoordinate(GetCurrentRow(i));
            processed_data[i - 1] = temp;
        }

        init_coordinates = processed_data[0].coordinates;
        for (int i = 0; i < init_coordinates.Length; i++)
        {
            init_coordinates[i].x += x;
            init_coordinates[i].y += y;
            init_coordinates[i].z += z;
        }


        init_timestamp = processed_data[0].timestamp;
        last_timestamp = float.Parse(GetCurrentRow(raw_data_read.Length - 2)[0]) - init_timestamp;
        timer = 0f;
        csv_time = GetRelativeTimeStamp(processed_data[0].timestamp);
        Update_row();

        transforms[0].position = new Vector3(0, 0, 0);

        for (int i = 1; i < transforms.Length; i++)
        {
            transforms[i].localPosition = init_coordinates[i - 1];
        }
    }


    /* // Update is called once per frame
     void Update()
     {
         ConditionCheck();
     }
     */
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
            csv_coordinates = processed_data[row_count].coordinates;
        }
    }
  
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
            Vector3 end_pos = csv_coordinates[i - 1];
       
            float ratio = (SecondsToMs(timer) - pre_timestamp) / (csv_time - pre_timestamp);
            transforms[i].localPosition = Vector3.Lerp(start_pos, end_pos, ratio);
        }
    }

    public string[] GetCurrentRow(int row_index)
    {
        return raw_data_read[row_index].Split(',');
    }

    /*float GetTimeStamp(string[] curr_row) // get the relative timestamp when init_timestamp is treated relatively as 0.
    {
        return float.Parse(curr_row[0]) - init_timestamp;
    }*/

    float GetRelativeTimeStamp(float raw_time) // get the relative timestamp to init_timestamp
    {
        return raw_time - init_timestamp;
    }

    float SecondsToMs(float time)
    {
        return time * 1000;
    }

    public Vector3[] RawDataToCoordinate(string[] raw_data) // raw_data conatains { timestamp, joint1_x, joint1_y, joint1_z, joint2_x.....}, which represents a row read from the data.
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

    /// <summary>
    /// Resamples a whole-body gesture into a fixed number of n body poses uniformly spaced in time.
    /// </summary>
   /* public void Resample(int n)
    {
        List<Vector3> set = new List<Vector3>();
        double I = last_timestamp / (n - 1);
        Vector3[] pose;
        set.Add(Poses[0]);
        for (int i = 1; i < Poses.Count; i++)
        {
            double timeDiff = Poses[i].Timestamp - set[set.Count - 1].Timestamp;
            while (timeDiff >= I)
            {
                // interpolate two body postures
                double t = I / timeDiff;
                BodyPose posture = new BodyPose();
                for (int j = 0; j < Poses[i].Joints.Count; j++)
                    posture.Joints.Add(new Point3D()
                    {
                        X = (1 - t) * set[set.Count - 1].Joints[j].X + t * Poses[i].Joints[j].X,
                        Y = (1 - t) * set[set.Count - 1].Joints[j].Y + t * Poses[i].Joints[j].Y,
                        Z = (1 - t) * set[set.Count - 1].Joints[j].Z + t * Poses[i].Joints[j].Z,
                        JointType = set[set.Count - 1].Joints[j].JointType
                    });
                posture.Timestamp = (1 - t) * set[set.Count - 1].Timestamp + t * Poses[i].Timestamp;
                set.Add(posture);
                timeDiff -= I;
            }
        }
        if (set.Count == n - 1)
            set.Add(Poses[Poses.Count - 1]);

        this.Poses = set;
    }
    */
}


/* public void prepare()
  {
      data_read = motion_data.text.Split('\n'); // read in the data and save it to data_read
      transforms = hand.GetComponentsInChildren<Transform>(); // {parent_object, joint1, joint2...}
      init_transforms = transforms;
      string[] row1 = GetCurrentRow(row_count);


      init_coordinates = RawDataToCoordinate(row1);
      for (int i = 0; i < init_coordinates.Length; i++)
      {
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

      for (int i = 1; i < transforms.Length; i++)
      {
          transforms[i].localPosition = init_coordinates[i - 1];
      }
  }*/
