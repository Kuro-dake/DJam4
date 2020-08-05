using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thunderstorm : MonoBehaviour
{
    public Sprite[] sprites;
    int current_sprite = 0;
    private void Start()
    {
        sr.color = Color.clear;
        StartCoroutine(Storm());
    }
    public void Lightning()
    {
        //current_sprite = 1;
        sr.sprite = sprites[current_sprite];
        current_sprite++;
        if (current_sprite >= sprites.Length)
        {
            current_sprite = 0;
        }
        sr.color = Color.white;
        Vector3 npos = new Vector3((Random.Range(0f, 4.5f) + 2f) * (Random.Range(0, 2) == 1 ? 1 : -1), Random.Range(2.5f, 2f));
        transform.position = npos;
        sr.flipX = Random.Range(0, 2) == 1;
        float rotation_range = 20f;
        transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-rotation_range, rotation_range));
    }
    SpriteRenderer sr
    {
        get { return GetComponent<SpriteRenderer>(); }
    }

    private void Update()
    {
        sr.color -= Color.black * Time.fixedDeltaTime * .5f;
    }

    [SerializeField]
    Light env_light;
    [SerializeField]
    float lightning_intensity = 3f;
    
    IEnumerator Storm()
    {
        List<int> numbers = new List<int> { 1, 2, 3 };
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 8f));
            Lightning();
            Color prev_color = env_light.color;
            float prev_intensity = env_light.intensity;
            //env_light.color = Color.white;
            env_light.intensity = lightning_intensity;
            float speed = Random.Range(1f, 1.5f);
            Color bgcolor = new Color(.3f, .15f, .15f);
            Camera.main.backgroundColor = bgcolor;
            int cnumber = numbers[Random.Range(0, numbers.Count)];
            numbers.Remove(cnumber);
            if (numbers.Count == 0)
            {
                numbers = new List<int> { 1, 2, 3,4 };
            }
            GM.audio.PlaySound("thunderbolt" + cnumber.ToString(), .6f, new FloatRange(.6f, .8f));
            while ((env_light.intensity -= Time.fixedDeltaTime * speed) > prev_intensity)
            {
                Camera.main.backgroundColor -= bgcolor * Time.fixedDeltaTime * (1f / speed);
                yield return null;
            }
            Camera.main.backgroundColor = Color.black;
            env_light.color = prev_color;
            env_light.intensity = prev_intensity;
        }
    }
}
