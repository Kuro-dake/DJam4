using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    public int max = 0;
    public int current { get; protected set; }
    [SerializeField]
    Color color;
    private void Update()
    {
        
    }
    IEnumerator Pulsate()
    {
        yield return null;
        Vector3 localscale = transform.localScale;
        while (true)
        {
            transform.localScale = localscale * (1f+(Mathf.Sin(Time.time * 8) + 2f) / 14f);
            yield return null;
        }
    }
    public void SetNumber(int number)
    {
        max = number;
        current = max;
        
        CreateCircles(max);
        StartCoroutine(Pulsate());
    }
    void CreateCircles(int number)
    {
        
        float angle_modifier = 0f;
        transform.DestroyChildren();
        for (int i = 0; i <number; i++)
        {

            Vector3 pos = PlaceOnCircle(transform.position, max - (float)i / (float)max, distance_from_center, angle_modifier);

            GameObject marker = GM.CreateCircle();
            marker.SetActive(true);
            marker.transform.position = pos;
            marker.GetComponent<SpriteRenderer>().color = color;

            Vector3 direction = transform.position - marker.transform.position;
            Vector2 ppos = transform.position;
            Vector2 mpos = transform.position + direction;
            Vector2 fdir = mpos - ppos;
            float target_rotation = Mathf.Atan2(fdir.y, fdir.x) * Mathf.Rad2Deg;

            marker.transform.rotation = Quaternion.Euler(0f, 0f, target_rotation + 90f);

            marker.transform.SetParent(transform);
            marker.transform.localScale = Vector2.one * segment_size;
            SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = "UI";
        }
    }
    [SerializeField]
    float segment_size = .8f,distance_from_center = .4f;
    public void ModifyNumber(int number)
    {
        current = number;
        for(int i=0; i< max; i++)
        {
            transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = i < current;
        }
    }
    public void Substract()
    {
        /*
        Transform todestroy = transform.GetChild(transform.childCount - 1);
        todestroy.GetComponent<SpriteRenderer>().enabled = false;
        todestroy.SetParent(null);
        Destroy(todestroy.gameObject);*/
        current--;
        transform.GetChild(current).GetComponent<SpriteRenderer>().enabled = false;
    }

    public static Vector3 PlaceOnCircle(Vector3 center, float circlepos, float radius, float angle_modifier = 0f)
    {

        // create random angle between 0 to 360 degrees 

        float ang = (angle_modifier + circlepos) * 360;
        Vector3 pos = new Vector3(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad),
                                   center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad),
                                   center.z);
        return pos;

    }
}
