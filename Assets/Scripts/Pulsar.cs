using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Pulsar : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Pulsate());
    }
    IEnumerator Pulsate()
    {
        yield return null;
        Vector3 localscale = transform.localScale;
        Color[] c = new Color[2];
        for (int i = 0; i < 2; i++)
        {
            Image im = transform.GetChild(i).GetComponent<Image>();
            c[i] = im.color;

        }
        while (true)
        {
            
            for (int i = 0; i < 2; i++)
            {
                Image im = transform.GetChild(i).GetComponent<Image>();
                c[i].a = (.4f + (Mathf.Sin(Time.realtimeSinceStartup * 8) + 2f) / 14f);
                im.color = c[i];
            }
            transform.localScale = localscale * (1f + (Mathf.Sin(Time.realtimeSinceStartup * 8) + 2f) / 14f);
            yield return null;
        }
    }

}
