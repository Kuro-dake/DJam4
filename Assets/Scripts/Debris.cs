using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    Vector3 direction;
    // Start is called before the first frame update
    public float orig_until_explosion;
    public float until_explosion;
    public static List<Debris> all_debris = new List<Debris>();
    public static void StartCountdown()
    {
        float extra_time = all_debris[0].until_explosion - (float)Mathf.FloorToInt(all_debris[0].until_explosion);
        all_debris.ForEach(delegate (Debris d)
        {
            if (d.cd == null || d.countdown)
            {
                return;
            }

            d.cd.SetNumber(Mathf.CeilToInt(d.until_explosion));
            d.until_explosion = (float)Mathf.FloorToInt(d.until_explosion) + extra_time;
            d.countdown = true;
            d.orig_until_explosion = (float)Mathf.FloorToInt(d.until_explosion);
        });
    }
    public void Fly()
    {
        Debug.Log(impact_point);
        Debug.Log(direction);


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
        Vector3 npos = transform.position;
        npos.x = Random.Range(0f, 4f) * (direction.x > 0f ? -1 : 1);
        transform.position = npos;
        do
        {
            pass = true;
            InitTrajectory();
            foreach (Debris d in all_debris)
            {
                if (d != this && Mathf.Abs(d.impact_point.x - impact_point.x) < 1f || Mathf.Abs(impact_point.x) > 8f)
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
        Debug.Log(impact_point);
        Debug.Log(direction);
        initialized = true;
        all_debris.Add(this);
        total++;
    }
    void Start()
    {

        

    }

    void InitTrajectory()
    {
        direction = Vector3.down;
        direction.x = Random.Range(.3f, 1.1f) * (Random.Range(0, 2) == 1 ? 1 : -1);
        RaycastHit2D[] rh2d = Physics2D.RaycastAll(transform.position, direction, Mathf.Infinity);
        foreach (RaycastHit2D hit in rh2d)
        {
            if (hit.collider.gameObject.tag == "ground")
            {
                impact_point = hit.point;
                break;
            }
        }
    }
    Vector3 impact_point;
    LineRenderer lr { get { return GetComponent<LineRenderer>(); } }
    TrailRenderer tr { get { return GetComponent<TrailRenderer>(); } }
    ParticleSystem explosion { get { return transform.Find("explosion").GetComponent<ParticleSystem>(); } }
    IEnumerator ShowImpactAndFly(Vector3 from, Vector3 to)
    {
        Debug.Log(from);
        Debug.Log(to);
        lr.SetPositions(new Vector3[] { to, from });
        while ((lr.startWidth -= Time.deltaTime * .5f) > 0f)
        {
            yield return null;
        }
        direction = (to - from).normalized;
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
                StartCoroutine(Explode());
                Destroy(cd.gameObject);
                GM.Explode();
                countdown = false;
            }
            until_explosion -= Time.deltaTime;
            return;
        }
        if (!moving)
        {
            return;
        }
        transform.position += direction * Time.deltaTime * speed;
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
    IEnumerator Explode() {

        explosion.startColor = GM.explode_debris_color;
        alive = false;
        explosion.Play();
        yield return new WaitForSeconds(.1f);
        sr.enabled = false;
        yield return new WaitForSeconds(1f);
        DestroyDebris();
    }
    void DestroyDebris()
    {
        all_debris.Remove(this);
        Destroy(gameObject);
        
    }
    [SerializeField]
    float speed = 4f;
    bool moving = false;
    public bool countdown = false;
    [SerializeField]
    float kill_radius = .7f;
    Countdown cd { get { return GetComponentInChildren<Countdown>(); } }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {
            transform.position = impact_point;
            moving = false;
            explosion.Play();
            GM.ShakeScreen(.5f);
            Debug.DrawLine(transform.position + Vector3.left * kill_radius, transform.position + Vector3.right * kill_radius, Color.red, .5f);
            GM.Explode();
            if (Mathf.Abs(GM.player.transform.position.x - transform.position.x) < kill_radius)
            {
               /* Destroy(GM.player.gameObject);
                GM.GameOver();*/
            }
            
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
        stasis = this;
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
            sr.color -= Color.black * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public static int total = 0;
    public static int salvaged = 0;

}
