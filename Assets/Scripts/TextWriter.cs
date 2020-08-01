using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextWriter : MonoBehaviour
{
    [SerializeField]
    float write_delay = .1f;
    [SerializeField]
    float erase_delay = .03f;

    public string text;
    int index = 0;
    Text ui_text
    {
        get
        {
            return GetComponent<Text>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //text = ui_text.text;
        ui_text.text = "";
        //gameObject.SetActive(false);
    }
    int cursor = 0;
    // Update is called once per frame
    Coroutine routine = null;
    void StopAll()
    {
        if(routine != null)
        {
            StopCoroutine(routine);
        }
    }
    public Coroutine Write(bool erase = true)
    {
        cursor = 0;
        StopAll();
        return routine = StartCoroutine(WriteStep());
    }
    IEnumerator WriteStep()
    {

        for (; cursor <= text.Length; cursor++)
        {
            
            if(cursor != text.Length && (new List<string>() { "\n", " " }).Contains(text.Substring(cursor, 1)))
            {
                continue;
            }
            ui_text.text = text.Substring(0, cursor);
            GM.audio.PlaySound("type", .2f, new FloatRange(.4f, .6f));
            yield return null;
            //yield return new WaitForSeconds(Random.Range(write_delay, write_delay + write_delay / 2f));
        }
        cursor -= 1;
    }

    public Coroutine Erase()
    {
        StopAll();
        return routine = StartCoroutine(EraseStep());
    }
    IEnumerator EraseStep()
    {

        for (; cursor >= 0; cursor -= Mathf.Clamp(erase_delay < .01f ? 8 : 1 , 0 , int.MaxValue))
        {
            ui_text.text = text.Substring(0, cursor);
            GM.audio.PlaySound("type", .2f, new FloatRange(.4f, .6f));
            yield return new WaitForSeconds(Random.Range(erase_delay / 2f, erase_delay));
        }
        ui_text.text = "";
    }
    bool _shaking = false;
    public bool shaking
    {
        get
        {
            return _shaking;
        }
        set
        {
            _shaking = value;
            if (shaking)
            {
                if (shaking_routine != null)
                {
                    return;
                }
                shaking_routine = StartCoroutine(ShakingStep());
            }
            else
            {
                StopShaking();
            }
        }
    }
    Coroutine shaking_routine = null;
    Vector2 orig_pos;
    IEnumerator ShakingStep()
    {

        orig_pos = rt.anchoredPosition;
        Debug.Log(orig_pos);
        while (true)
        {
            rt.anchoredPosition = orig_pos + Random.insideUnitCircle.normalized * 5f;
            yield return null;
        }
    }
    void StopShaking()
    {
        if (shaking_routine != null)
        {
            rt.anchoredPosition = orig_pos;
            StopCoroutine(shaking_routine);
        }
    }

    public void Blacken()
    {
        StartCoroutine(ChangeColor(false));
    }
    public void Whiten()
    {
        StartCoroutine(ChangeColor(true));
    }
    IEnumerator ChangeColor(bool to_white)
    {
        float target_r = to_white ? 1f : .235f;
        Color c = ui_text.color;
        Color white_clear = Color.white;
        white_clear.a = 0f;
        while (!Mathf.Approximately(target_r, c.r))
        {
            float r = Mathf.MoveTowards(ui_text.color.r, target_r, Time.deltaTime);
            Color nc = Color.white * r;
            nc.a = 1f;
            c = nc;
            ui_text.color = c;
            yield return null;
        }
    }
    RectTransform rt { get { return GetComponent<RectTransform>(); } }
}
