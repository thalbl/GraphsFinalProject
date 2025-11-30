using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EdgeData
{
    public float costHealth;
    public float costSanity;
    public float costTime;
    public string description;

    public EdgeData(float health, float sanity, float time, string desc = "")
    {
        costHealth = health;
        costSanity = sanity;
        costTime = time;
        description = desc;
    }
}
