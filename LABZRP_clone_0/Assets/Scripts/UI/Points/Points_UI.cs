using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Points_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI texto;

    private int points;
    
    public void setPoints(int points)
    {
        this.points = points;
        updateText();
    }
    
    public void updateText()
    {
        texto.text = "$|" + points;
    }
}
