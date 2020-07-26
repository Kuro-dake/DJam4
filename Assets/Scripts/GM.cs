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
        Intro();
    }
    Coroutine start_level_routine = null;
    private void Update()
    {
        ticked = false;
        ui_text = player.recharges > 0 ? player.recharges + " remote reset usages left" : "";

        if(start_level_routine != null && Input.GetKeyDown(KeyCode.Space))
        {
            StopCoroutine(start_level_routine);
            start_level_routine = null;
            level_description.Erase();
            SetCurtainVisible(false);
            debris.RunLevel(debris.run_level);
            
        }
    }
    void Intro()
    {
        StartCoroutine(IntroStep());
    }
    IEnumerator IntroStep()
    {
        yield return new WaitForSeconds(.4f);
        level_description.text = "Portal Assembly\n\nYour colony spaceship is under attack, and is descending into an atmosphere of an unknown planet.\n\n" +
            "You didn't manage to reach the on-board portal. Your only choice was to save yourself in an escape pod.\n\n" +
            "Now you need to salvage the debris of your destroyed spaceship that is raining on the planet your escape pod crashed into\n" +
            "and use it to build another portal.\n\nPress space to start";
        yield return level_description.Write(true);

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        GameOver();
    }
    void Outro()
    {
        StartCoroutine(OutroStep());
    }
    IEnumerator OutroStep()
    {
        yield return new WaitForSeconds(.4f);
        level_description.text = "You have succesfuly built the portal and escaped the planet.\n\n" +
            "Congratulations, and thank you gor playing\n\n" +
            "Made by Kuro / Dizztal for DJam4\n" +
            "26 - 27 July 2020";
        yield return level_description.Write(true);

        while (true)
        {
            yield return null;
        }
        
    }
    void StartLevel(int level)
    {
        start_level_routine = StartCoroutine(StartLevelStep(level));
    }
    string[] level_names = new string[]
    {
        "\"Crash landing\"" +
        "\n\n" +
        "Press Spacebar to skip this text\n\n"+
        "Use A and D or left and right cursor keys to move.\n\n" +
        "Avoid falling debris!\n\n" +
        "Use W or up cursor key to salvage fallen debris once the timers appear.\n\n" +
        "You have limited time to salvage each fallen part before it explodes, and destroys all the other debris.",

        "\"The ship enters the atmosphere\"\n\n" +
        "Watch the order in which the debris hit the ground.\nThe sooner debris falls the sooner it explodes.",

        "\"Stasis\"\n\n" +
        "You have gained the ability to stop the countdown of a single debris.\nHover with your mouse over the debris you wish to freeze.",

        "\"Time freezes\"\n\n" +
        "You have gained the ability to reset the debris timers.\nClick the debris of which timer you wish to reset.\n" +
        "\nYou can do this three times per wave.\n\n" +
        "You have also gained the ability to stop time. Stop moving to freeze time, \nso you can better decide what to do next.",

        "\"The sky is falling down\"\n\n" +
        "You have gained the ability to reset any debris timer as many times you want.\n\n" +
        "The debris timers will reset once you approach it."
    };
    IEnumerator StartLevelStep(int level)
    {
        yield return new WaitForSeconds(.4f);
        level_description.text = "Level " + (level + 1) + ":\n" + level_names[level];
        yield return level_description.Write(true);
        
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        level_description.Erase();
        yield return SetCurtainVisible(false);
        yield return new WaitForSeconds(1.5f);

        debris.RunLevel(level);
        start_level_routine = null;
    }
    public static AudioManager audio { get { return inst.GetComponentInChildren<AudioManager>(); } }
    public static EffectsManager effects { get { return inst.GetComponentInChildren<EffectsManager>(); } }
    public static DebrisGenerator debris { get { return inst.GetComponentInChildren<DebrisGenerator>(); } }

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
    static Coroutine game_over_routine = null;

    public static void GameOver()
    {
        if(game_over_routine == null)
        {
            game_over_routine = inst.StartCoroutine(GameOverStep());
        }
        
    }
    static IEnumerator GameOverStep()
    {
        
        Debris.ExplodeAllDebris();
        debris.StopRunLevel();
        
        yield return SetCurtainVisible(true);
        game_over_routine = null;
        yield return null;
        if(debris.run_level < 5)
        {
            inst.StartLevel(debris.run_level);
        }
        else
        {
            inst.Outro();
        }
        
        
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
    [SerializeField]
    GameObject pause_icon;
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
            if(game_over_routine == null && inst.start_level_routine == null)
            {
                Time.timeScale = value ? 0f : 1f;
                foreach (Image i in inst.pause_icon.GetComponentsInChildren<Image>())
                {
                    i.enabled = value;
                }
            }
            else
            {
                Time.timeScale = 1f;
                foreach (Image i in inst.pause_icon.GetComponentsInChildren<Image>())
                {
                    i.enabled = false;
                }
            }
            
        }
    }
    static Coroutine SetCurtainVisible(bool visible)
    {
        return inst.StartCoroutine(inst.SetCurtainVisibleStep(visible));
    }
    [SerializeField]
    TextWriter level_description;
    [SerializeField]
    float curtain_speed = 2f;
    [SerializeField]
    Image curtain;
    IEnumerator SetCurtainVisibleStep(bool visible)
    {
        float target = visible ? 1f : 0f;
        while (!Mathf.Approximately(curtain.color.a, target))
        {
            Color c = curtain.color;
            c.a = Mathf.MoveTowards(c.a, target, Time.deltaTime * curtain_speed);
            curtain.color = c;
            yield return null;
        }
    }
}
