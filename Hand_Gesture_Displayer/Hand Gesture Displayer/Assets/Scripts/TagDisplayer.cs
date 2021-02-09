using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TagDisplayer : MonoBehaviour
{
    private bool selected = false;
    public Gesture gesture;
    public TMP_Text gestureTagDisplayWindow;

    //When the mouse hovers over the GameObject, it turns to this color (red)
    Color m_MouseOverColor = Color.red;

    //This stores the GameObject’s original color
    Color m_OriginalColor;

    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer[] m_Renderer;

    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponentsInChildren<MeshRenderer>();
        Debug.Log(m_Renderer.Length);
        //Fetch the original color of the GameObject
        m_OriginalColor = m_Renderer[0].material.color;
    }

    public void Highlight()
    {

        foreach (MeshRenderer m in m_Renderer)
        {
            m.material.color = m_MouseOverColor;
        }

    }

    public void UnHightlight()
    {
        // Reset the color of the GameObject back to normal
        foreach (MeshRenderer m in m_Renderer)
        {
            m.material.color = m_OriginalColor;
        }
    }

    public void DisplayTag()
    {
        //gesture.getTag().transform.localPosition = pos;
        gestureTagDisplayWindow.text = gesture.getTag();

        //gesture.getTag().SetActive(true);
    }

    public void HideTag()
    {
        gestureTagDisplayWindow.text = "";
        //gesture.getTag().SetActive(false);
    }

    public void Select()
    {
        selected = true;
        Analyser.instance.selectedGestures.Add(gesture);
    }

    public bool IfSelected()
    {
        return selected;
    }

    public void Unselect()
    {
        //Debug.Log(GetComponentInParent<GameObject>().GetComponentInParent<Analyser>());  
        selected = false;
        Analyser.instance.selectedGestures.Remove(gesture);
    }
}