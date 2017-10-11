using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtils : MonoBehaviour
{
    public TreeSystem m_TreeSystem;

    private float m_Distance = 300;

	void OnGUI()
    {
        float dist = GUI.HorizontalSlider(new Rect(20, 20, 100, 20), m_Distance, 0f, 2000f);
        GUI.Label(new Rect(20, 40, 200, 20), "Distance: " + m_Distance);

        if(dist != m_Distance)
        {
            m_Distance = Mathf.Floor(dist);
            m_TreeSystem.SetTreeDistance(m_Distance);
        }
    }
}
