﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Dictionary<action, Vector3> direction_movement = new Dictionary<action, Vector3>() {
        {action.left, Vector3.left },
        {action.right, Vector3.right },
        {action.none, Vector3.zero}
    };
    Dictionary<KeyCode, action> direction_keys = new Dictionary<KeyCode, action>() {
        {KeyCode.W, action.interact },
        {KeyCode.UpArrow, action.interact },

        {KeyCode.A, action.left },
        {KeyCode.D, action.right },
        
        {KeyCode.LeftArrow, action.left },
        {KeyCode.RightArrow, action.right },
        

    };
    float x_boundary = 8f;
    void Act(action d)
    {
        
        if (direction_movement.ContainsKey(d))
        {
            Vector3 npos = transform.position + direction_movement[d] * (warudo ? Time.fixedDeltaTime : Time.deltaTime) * speed;
            float nval = Mathf.Clamp(npos.x, -x_boundary, x_boundary);
            walking = nval == npos.x;
            npos.x = nval;
            transform.position = npos;

            csr.flipX = d == action.left;
            
            pause_in = .3f;
            
            
            interacting = false;
        }
        if (!warudo)
        {
            GM.paused = false;
        }
        
        
    }
    bool walking = false;
    [SerializeField]
    private float speed = 2f;
    SpriteRenderer sr { get { return GetComponent<SpriteRenderer>(); } }
    SpriteRenderer csr { get { return GetComponentInChildren<SpriteRenderer>(); } }
    Animator anim { get { return GetComponent<Animator>(); } }
    // Update is called once per frame
    float pause_in = .3f;
    public bool time_stop { get { return GM.debris.run_level >= 3; } }
    Coroutine warudo_routine = null;
    void Warudo()
    {
        if(warudo_routine == null)
        {
            warudo_routine = StartCoroutine(WarudoStep());
        }
        
    }
    public void StopWarudo()
    {
        if(warudo_routine != null)
        {
            StopCoroutine(warudo_routine);
            StartCoroutine(ReduceWarudoStep());
            warudo_routine = null;
        }

    }

    IEnumerator ReduceWarudoStep() {
        Transform invert = GameObject.Find("invert").transform;
        GM.audio.FadeOutSource(warudo_source, .3f);
        while ((current -= Time.fixedDeltaTime * .7f) > 0f)
        {
            Debug.Log(current);
            invert.localScale = Vector3.one * Mathf.Sin(current * Mathf.PI) * 40f;
            yield return null;
        }
        current = 0f;
        invert.localScale = Vector2.zero;
        Time.timeScale = 1f;
    }
    AudioSource warudo_source;
    public bool warudo { get { return warudo_routine != null; } }
    float current = 0f;
    IEnumerator WarudoStep()
    {
        ParticleSystemTimeScale.scaled = true;
        Transform invert = GameObject.Find("invert").transform;
        current = 0f;
        warudo_source = GM.audio.PlaySound("warudo", .9f);
        yield return new WaitForSeconds(1.55f);
        while ((Time.timeScale -= Time.fixedDeltaTime * 4f) > .05f)
        {
            yield return null;
        }
        Time.timeScale = 0f;
        
        
        while ((current += Time.fixedDeltaTime * .7f) < 1f)
        {   
            invert.localScale = Vector3.one * Mathf.Sin(current * Mathf.PI) * 40f;
            yield return null;
        }
        /*while ((current -= Time.deltaTime) > 0f)
        {
            invert.localScale = Vector3.one * Mathf.Lerp(0f, target, current);
            yield return null;
        }*/
        current = 0f;
        invert.localScale = Vector2.zero;
        while (warudo_source != null && warudo_source.isPlaying)
        {
            yield return null;
        }
        for(int i = 0; i < 3; i++)
        {
            GM.Tick();
            yield return new WaitForSecondsRealtime(1f);
        }
        while ((Time.timeScale += Time.fixedDeltaTime * 4f) < 1f)
        {

            yield return null;
        }
        Time.timeScale = 1;
        warudo_routine = null;
    }
    void Update()
    {
        anim.updateMode = warudo ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
        walking = false;
        foreach(KeyValuePair<KeyCode, action> kv in direction_keys)
        {
            if (Input.GetKey(kv.Key))
            {
                Act(kv.Value);
                break;
            }
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) 
        {
            if (GM.debris_falling && GM.debris.run_level > 2)
            {
                Warudo();
            }
            else
            {
                interacting = true;
            }
            
        }
        
        if (interacting)
        {
            interacting = Interact();
        }
        if(DebrisGenerator.playing && time_stop && !GM.paused && (pause_in-= Time.deltaTime) <= 0f)
        {
            GM.paused = true;
        }
        anim.SetBool("walking", walking);
        
        if(nearby_active_debris != null && reset_on_pass)
        {
            nearby_active_debris.ResetCountdown();
        }
        if (!interacting)
        {
            current_interact_duration = interact_duration;
            cd.ModifyNumber(0);
            work_sound = false;
        }
        else
        {
            if (!was_interacting)
            {
                cd.SetNumber(cd_segments);
                cd.transform.position = nearby_active_debris.transform.position;
                work_sound = true;
            }
            else
            {
                
                float to_set = ((interact_duration - current_interact_duration) / interact_duration) * (float)cd_segments;
                
                cd.ModifyNumber(cd_segments - Mathf.CeilToInt(to_set));
            }
        }
        was_interacting = interacting;
    }
    AudioSource work_sound_source;
    bool work_sound
    {
        set
        {
            if (work_sound_source != null)
            {
                GM.audio.FadeOutSource(work_sound_source, 1.5f);
            }
            if (value)
            {
                work_sound_source = GM.audio.PlaySound("work", .5f);
            }

        }
    }
    [SerializeField]
    int cd_segments = 10;
    private void LateUpdate()
    {
        Debris d = nearby_active_debris;
        if (d != null)
        {
            d.hilight = true;
        }
    }
    Countdown cd { get { return GetComponentInChildren<Countdown>(); } }
    [SerializeField]
    float interact_duration = 1f;
    float current_interact_duration = 0f;
    public bool interacting = false;
    bool was_interacting = false;
    bool Interact()
    {
        Debris d = nearby_active_debris;
        if(d == null)
        {
            return false;
        }
        pause_in = .3f;
        if((current_interact_duration -= Time.deltaTime) <= 0f)
        {
            d.Salvage();
            current_interact_duration = interact_duration;
            return false;
        }
        return true;
    }
    public Debris nearby_active_debris
    {
        get
        {
            Debris id = null;
            foreach (Debris d in Debris.all_debris)
            {
                if (d.countdown && d.alive && Mathf.Abs(transform.position.x - d.transform.position.x) < .4f)
                {
                    id = d;
                    break;
                }
            }
            return id;
        }
    }

    int _recharges = 0;
    public int recharges
    {
        get { return _recharges; }
        set
        {
            _recharges = value;
        }
    }
    
    bool reset_on_pass { get { return GM.debris.run_level >= 4; } }
    public bool hover_stasis { get { return GM.debris.run_level >= 2; } }
}

enum action
{
    left,
    right,
    interact,
    none
}