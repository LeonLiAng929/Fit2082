using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagDisplayer_Aux : MonoBehaviour
{
    //When the mouse hovers over the GameObject, it turns to this color (red)
    Color m_MouseOverColor = Color.red;

    //This stores the GameObject’s original color
    Color m_OriginalColor;

    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;
    TagDisplayer parent;

    void Start()
    {
        parent = GetComponentInParent<TagDisplayer>();
    }

    void OnMouseOver()
    {
        // Change the color of the GameObject to red when the mouse is over GameObject
        parent.Highlight();
        parent.DisplayTag();
    }

    void OnMouseUp()
    {
        if (parent.IfSelected())
        {
            parent.Unselect();
            parent.UnHightlight();
        }
        else
        {
            parent.Select();
            parent.Highlight();
        }
    }

    void OnMouseExit()
    {
        // Reset the color of the GameObject back to normal
        if (parent.IfSelected())
        {
            parent.HideTag();
        }
        else
        {
            parent.UnHightlight();
            parent.HideTag();
        }
    }
}