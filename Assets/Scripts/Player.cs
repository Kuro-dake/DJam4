using System.Collections;
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
        {KeyCode.A, action.left },
        {KeyCode.D, action.right },
        {KeyCode.W, action.interact },
        {KeyCode.LeftArrow, action.left },
        {KeyCode.RightArrow, action.right },
        {KeyCode.UpArrow, action.interact }

    };
    float x_boundary = 8f;
    void Act(action d)
    {
        if (direction_movement.ContainsKey(d))
        {
            Vector3 npos = transform.position + direction_movement[d] * Time.deltaTime * speed;
            float nval = Mathf.Clamp(npos.x, -x_boundary, x_boundary);
            walking = nval == npos.x;
            npos.x = nval;
            transform.position = npos;

            csr.flipX = d == action.left;
        }
        else if(d == action.interact)
        {
            Interact();
        }
        GM.paused = false;
        pause_in = 1f;
        
    }
    bool walking = false;
    [SerializeField]
    private float speed = 2f;
    SpriteRenderer sr { get { return GetComponent<SpriteRenderer>(); } }
    SpriteRenderer csr { get { return GetComponentInChildren<SpriteRenderer>(); } }
    Animator anim { get { return GetComponent<Animator>(); } }
    // Update is called once per frame
    float pause_in = 1f;
    void Update()
    {
        interacting = false;
        walking = false;
        foreach(KeyValuePair<KeyCode, action> kv in direction_keys)
        {
            if (Input.GetKey(kv.Key))
            {
                Act(kv.Value);
            }
        }
        if(DebrisGenerator.playing && !GM.paused && (pause_in-= Time.deltaTime) <= 0f)
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
    void Interact()
    {
        Debris d = nearby_active_debris;
        if(d == null)
        {
            return;
        }
        interacting = true;
        if((current_interact_duration -= Time.deltaTime) <= 0f)
        {
            d.Salvage();
            current_interact_duration = interact_duration; 
        }
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

    int _recharges = 3;
    public int recharges
    {
        get { return _recharges; }
        set
        {
            _recharges = value;
        }
    }
    [SerializeField]
    bool reset_on_pass = false;
    public bool hover_stasis = false;
}

enum action
{
    left,
    right,
    interact,
    none
}