using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTime : MonoBehaviour
{
    Text text
    {
        get { return GetComponent<Text>(); }
    }
    float t = 0;
    // Update is called once per frame
    void Update()
    {
        int min = Mathf.FloorToInt(Time.time / 60f);
        int sec = Mathf.FloorToInt(Time.time) - min * 60;
        text.text = min.ToString("D2") + ":" + sec.ToString("D2");
    }
}
