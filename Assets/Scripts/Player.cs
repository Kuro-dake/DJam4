using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Dictionary<direction, Vector3> direction_movement = new Dictionary<direction, Vector3>() {
        {direction.left, Vector3.left },
        {direction.right, Vector3.right },
        {direction.none, Vector3.zero}
    };
    Dictionary<KeyCode, direction> direction_keys = new Dictionary<KeyCode, direction>() {
        {KeyCode.A, direction.left },
        {KeyCode.D, direction.right },
        {KeyCode.LeftArrow, direction.left },
        {KeyCode.RightArrow, direction.right }

    };
    float x_boundary = 8f;
    void Move(direction d)
    {
        Vector3 npos = transform.position + direction_movement[d] * Time.deltaTime * speed;
        float nval = Mathf.Clamp(npos.x, -x_boundary, x_boundary);
        walking = nval == npos.x;
        npos.x = nval;
        transform.position = npos;
        
        csr.flipX = d == direction.left;
        
    }
    bool walking = false;
    [SerializeField]
    private float speed = 2f;
    SpriteRenderer sr { get { return GetComponent<SpriteRenderer>(); } }
    SpriteRenderer csr { get { return GetComponentInChildren<SpriteRenderer>(); } }
    Animator anim { get { return GetComponent<Animator>(); } }
    // Update is called once per frame
    void Update()
    {
        interacting = false;
        walking = false;
        foreach(KeyValuePair<KeyCode, direction> kv in direction_keys)
        {
            if (Input.GetKey(kv.Key))
            {
                Move(kv.Value);
            }
        }
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Interact();
        }
        anim.SetBool("walking", walking);

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
            d.Disarm();
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
            GM.ui_text = "recharges: " + recharges;
        }
    }

}

enum direction
{
    left,
    right,
    none
}