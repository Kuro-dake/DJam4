using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    static GM inst;
    private void Start()
    {
        inst = this;
        Initialize();
        
    }
    Vector3 shake_center;
    void Initialize()
    {
        shake_center = Camera.main.transform.position;
        player.recharges = player.recharges;
    }
    public static AudioManager audio { get { return inst.GetComponentInChildren<AudioManager>(); } }
    public static EffectsManager effects { get { return inst.GetComponentInChildren<EffectsManager>(); } }

    public bool shake_screen = false;

    public float shake_screen_strength = 1f;

    IEnumerator ShakeScreenRoutine()
    {
        float shakescreen_inst = shake_screen_strength;
        
        while (shakescreen_inst > .01f)
        {
            shakescreen_inst -= Time.deltaTime * 2;
            Camera.main.transform.position = shake_center + new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), 0f) * shakescreen_inst;
            
            
            yield return null;
        }
        Camera.main.transform.position = shake_center;
    }

    public static void ShakeScreen(float strength = 1f)
    {
        inst.shake_screen_strength = strength;
        inst.StartCoroutine(inst.ShakeScreenRoutine());
    }
    [SerializeField]
    Player _player;
    public static Player player { get { return inst._player; } }
    [SerializeField]
    Text dev_output;
    public static string ui_text
    {
        set
        {
            inst.dev_output.text = value;
        }
    }
    public static void GameOver()
    {
        inst.StartCoroutine(inst.GameOverStep());
    }
    IEnumerator GameOverStep()
    {
        yield return new WaitForSeconds(1f);
        Application.LoadLevel(0);
    }
    [SerializeField]
    Sprite _circle;
    [SerializeField]
    Color _explode_debris_color;
    public static Color explode_debris_color { get { return inst._explode_debris_color; } }
    [SerializeField]
    Color _marker_color;
    public static Color marker_color { get { return inst._marker_color; } }

    public static GameObject CreateCircle()
    {
        GameObject ret = new GameObject("circle", new System.Type[] { typeof(SpriteRenderer) });
        SpriteRenderer csr = ret.GetComponent<SpriteRenderer>();
        csr.sprite = inst._circle;
        csr.color = marker_color;
        return ret;
    }

    static bool ticked = false;
    static int clock = 1;
    private void Update()
    {
        ticked = false;
        ui_text = "recharges: " + player.recharges + " salvaged " + Debris.salvaged + "/" + Debris.total + (paused ? " <b>||</b>" : "");
    }
    
    public static void Tick() {
        if (ticked)
        {
            return;
        }
        audio.PlaySound("clock" + clock);
        clock = clock == 1 ? 2 : 1;
        ticked = true;
    }

    public static void Explode()
    {
        int expl = Random.Range(1, 3);
        audio.PlaySound("explode" + expl, expl == 1 ? .9f : .6f, new FloatRange(.8f, 1.2f));
    }
    static bool _paused = false;
    public static bool paused
    {
        get
        {
            return _paused;
        }
        set
        {
            _paused = value;
            Time.timeScale = value ? 0f : 1f;
        }
    }
}
