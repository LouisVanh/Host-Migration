using System;
using UnityEngine;

[Serializable]
public class BoosterCardVisualData
{
    public string Name;
    public string Description;
    public Color Color;

    public BoosterCardVisualData(string name, string description, Color color)
    {
        Name = name;
        Description = description;
        Color = color;
    }
}
