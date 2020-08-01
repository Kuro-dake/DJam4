using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    [SerializeField]
    Vector3 direction;
    // Start is called before the first frame update
    public float orig_until_explosion;
    float _until_explosion = 0f;
    public float until_explosion
    {
        set {
            //Debug.Log("ue set to " + value);
            _until_explosion = value; }
        get
        {
            return _until_explosion;
        }
    }
    public static List<Debris> all_debris = new List<Debris>();
    public static List<Debris> trash = new List<Debris>();
    public static void ExplodeAllDebris()
    {
        all_debris.ForEach(delegate (Debris d)
        {
            d.Explode();
        });
        trash.ForEach(delegate (Debris d)
        {
            if(trash != null)
            {
                d.Explode();
            }
            
        });

    }
    public static void StartCountdown()
    {
        float extra_time = all_debris[0].until_explosion - (float)Mathf.FloorToInt(all_debris[0].until_explosion);
        all_debris.ForEach(delegate (Debris d)
        {
            if (d.cd == null || d.countdown || !d.persistent)
            {
                return;
            }
            
            d.cd.SetNumber(Mathf.CeilToInt(d.until_explosion));

            Debug.Log((float)Mathf.FloorToInt(18f));

            d.until_explosion = (float)Mathf.FloorToInt(d.until_explosion) + extra_time;
            d.countdown = true;
            d.orig_until_explosion = (float)Mathf.FloorToInt(d.until_explosion);
        });
    }
    bool released = false;
    public void Fly()
    {
        if (!alive) {
            return;
        }
        if (!initialized)
        {
            throw new UnityException("Uninitialized debris can not fly");
        }
        StartCoroutine(ShowImpactAndFly(transform.position, impact_point + (impact_point - transform.position).normalized * 2f));
    }
    public bool initialized = false;
    public void Initialize()
    {
        bool pass;
        int i = 0;
        tr.enabled = false;
        
        do
        {
            pass = true;
            InitTrajectory();
            if((Mathf.Abs(impact_point.x) > (persistent ? 7f : 10f)))
            {
                pass = false;
                continue;
            }
            foreach (Debris other_debris in all_debris)
            {
                
                if (other_debris != this && (persistent && other_debris.persistent) && Mathf.Abs(other_debris.impact_point.x - impact_point.x) < 1f)
                {
                    pass = false;
                    break;
                }
            }
            if (i++ > 100)
            {
                
                throw new UnityException("Too many attempts at finding impact point");
                
            }
        }
        while (!pass);
        
        initialized = true;
        all_debris.Add(this);
        
       
        total++;
    }
   

    void InitTrajectory()
    {
        bool ghit = false;
        int i = 0;
        do
        {
            Vector3 npos = transform.position;
            npos.x = Random.Range(0f, 8f) * (direction.x > 0f ? -1 : 1);
            transform.position = npos;
            direction = Vector3.down;
            direction.x = Random.Range(.3f, 1.1f) * (Random.Range(0, 2) == 1 ? 1 : -1);
            RaycastHit2D[] rh2d = Physics2D.RaycastAll(transform.position, direction, Mathf.Infinity);
            foreach (RaycastHit2D hit in rh2d)
            {
                if (hit.collider.gameObject.tag == "ground")
                {

                    impact_point = hit.point;
                    ghit = true;
                    break;
                }
            }
            if(i++ > 100)
            {
                throw new UnityException("Cannot find ground");
            }
        }
        while (!ghit);
        
    }
    [SerializeField]
    Vector3 impact_point;
    LineRenderer lr { get { return GetComponent<LineRenderer>(); } }
    TrailRenderer tr { get { return GetComponent<TrailRenderer>(); } }
    ParticleSystem explosion { get { return transform.Find("explosion").GetComponent<ParticleSystem>(); } }
    public static bool any_flying
    {
        
            get{
                foreach(Debris d in all_debris)
                {
                    if (d.flying)
                    {
                        return true;
                    }

                }
                return false;
            }
        
    }
    bool flying = false;
    IEnumerator ShowImpactAndFly(Vector3 from, Vector3 to)
    {
        flying = true;
        released = true;
        lr.SetPositions(new Vector3[] { to, from });
        while ((lr.startWidth -= Time.deltaTime * .5f) > 0f)
        {
            yield return null;
        }
        //direction = (to - from).normalized;
        moving = true;
        tr.enabled = true;
        lr.enabled = false;

        
    }
    ParticleSystem stasis_system
    {
        get { return transform.Find("stasis").GetComponent<ParticleSystem>(); }
    }
    static Debris _stasis = null;
    static Debris stasis
    {
        get { return _stasis; }
        set
        {
            if (!GM.player.hover_stasis)
            {
                _stasis = null;
                return;
            }
            if(stasis != null)
            {
                stasis.stasis_system.Stop();
            }
            _stasis = value;
            stasis.stasis_system.Play();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!should_hilight)
        {
            hilight = false;
        }
        should_hilight = false;
        hilight_sprite.transform.Rotate(0f, 0f, 2f * 360f * Time.fixedDeltaTime);
        if (!alive)
        {
            return;
        }
        if (countdown)
        {
            if (Mathf.CeilToInt(until_explosion) < cd.current && (GM.player.nearby_active_debris == this && GM.player.interacting || stasis == this)) {
                until_explosion += 1f;
            }
            if(cd.current > 1 && Mathf.CeilToInt(until_explosion) < cd.current)
            {
                cd.Substract();
                GM.Tick();
            }
            if (until_explosion <= 0f)
            {
                GM.GameOver();
            }
            until_explosion -= Time.deltaTime;
            return;
        }
        if (!moving)
        {
            return;
        }
        /*float radius = 1.5f;
        float aspeed = speed;
        Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 invertpos = mpos;
        invertpos.z = 0f;
        //invert.transform.position = invertpos;
        Debug.DrawLine(mpos.Vector2(), mpos.Vector2() + Vector2.up * radius, Color.green);
        Debug.DrawLine(mpos.Vector2(), mpos.Vector2() + Vector2.left * radius, Color.green);
        Debug.DrawLine(mpos.Vector2(), mpos.Vector2() + Vector2.down * radius, Color.green);
        Debug.DrawLine(mpos.Vector2(), mpos.Vector2() + Vector2.right * radius, Color.green);
        if (Vector2.Distance(transform.position, mpos) < radius){
            aspeed /= (9f*(Vector2.Distance(transform.position, mpos) * 20)) * - 1;
        }*/

        transform.position += direction * Time.deltaTime * speed;
    }
    public void Explode()
    {
        StartCoroutine(ExplodeStep());
        Destroy(cd.gameObject);

        if (released && alive)
        {
            GM.Explode();
        }

        alive = false;
        countdown = false;

        
        countdown = false;
    }
    public bool alive = true;
    [SerializeField]
    GameObject hilight_sprite;
    public bool hilight
    {
        set
        {
            hilight_sprite.GetComponent<SpriteRenderer>().enabled = value;
        }
    }
    SpriteRenderer sr { get { return GetComponent<SpriteRenderer>(); } }
    IEnumerator ExplodeStep() {

        explosion.startColor = GM.explode_debris_color;
        yield return null;
        all_debris.Remove(this);
        trash.Remove(this);
        explosion.Play();
        yield return new WaitForSeconds(.1f);
        sr.enabled = false;
        yield return new WaitForSeconds(1f);
        
        DestroyDebris();
    }
    void DestroyDebris()
    {
        all_debris.Remove(this);
        trash.Remove(this);
        Destroy(gameObject);
        
    }
    [SerializeField]
    float speed = 4f;
    bool moving = false;
    public bool countdown = false;
    [SerializeField]
    float kill_radius = .7f;
    Countdown cd { get { return GetComponentInChildren<Countdown>(); } }
    public bool persistent = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {
            transform.position = impact_point;
            moving = false;
            if (!persistent)
            {
                Explode();
            }
            else
            {
                explosion.Play();
            }
            
            GM.ShakeScreen();
            Debug.DrawLine(transform.position + Vector3.left * kill_radius, transform.position + Vector3.right * kill_radius, Color.red, kill_radius);
            GM.Explode();
            
            if (Mathf.Abs(GM.player.transform.position.x - transform.position.x) < kill_radius)
            {
                //Destroy(GM.player.gameObject);
                GM.GameOver();
            }
            flying = false;

        }
    }
    bool should_hilight = false;
    void OnMouseOver()
    {
        hilight = true;
        should_hilight = true;
    }

    private void OnMouseEnter()
    {
        if(alive && persistent && countdown)
        {
            stasis = this;
        }
        
    }

    private void LateUpdate()
    {
        
        if (should_hilight)
        {
            hilight = true;
        }
    }

    private void OnMouseDown()
    {
        if(cd != null && cd.current < cd.max && GM.player.recharges > 0)
        {
            GM.player.recharges--;
            ResetCountdown();
        }
    }

    public void ResetCountdown()
    {
        if (cd != null && cd.current < cd.max)
        {
            cd.ModifyNumber(cd.max);
            while (until_explosion + 1f < orig_until_explosion)
            {
                until_explosion += 1f;
            }
            GM.audio.PlaySound("recharge", .5f, new FloatRange(.8f, 1.2f));
            GM.effects.PlayEffect("recharge", transform.position);
        }
    }

    public void Salvage()
    {
        alive = false;
        Destroy(cd.gameObject);
        StartCoroutine(SalvageStep());
        GM.audio.PlaySound("salvage", .5f, new FloatRange(.8f, 1.2f));
        all_debris.Remove(this);

        salvaged++;
    }

    IEnumerator SalvageStep()
    {
        GM.effects.PlayEffect("salvage", transform.position, true);
        while(sr.color.a > 0f)
        {
            sr.color -= Color.black * Time.fixedDeltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public static int total = 0;
    public static int salvaged = 0;

}
