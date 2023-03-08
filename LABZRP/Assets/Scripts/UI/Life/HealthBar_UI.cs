using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar_UI : MonoBehaviour
{
    private Color _color;
    public GameObject Fill;
    private Slider _slider;
    private Image _fill;
    // Start is called before the first frame update
    private void Start()
    {
        _fill = Fill.GetComponent<Image>();
        _slider = GetComponent<Slider>();
    }

    public void setMaxHealth(int health)
    {
        _slider.maxValue = health;
        _slider.value = health;
    }
    public void SetHealth(int health)
    {
        _slider.value = health;
    }

    public void setColor(Color color)
    {
        _fill.color = color;
    }
    
    
    public Color getColor()
    {
        return _fill.color;
    }
}
