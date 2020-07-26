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
        all_debris.ForEach(delegate (Debris d)
        {
            d.cd.SetNumber(Mathf.CeilToInt(d.until_explosion));
            d.countdown = true;
            d.orig_until_explosion = d.until_explosion;
        });
    }
    void Start()
    {
        bool pass = true;
        int i = 0;
        do
        {
            pass = true;
            InitTrajectory();
            foreach (Debris d in all_debris)
            {
                if (Mathf.Abs(d.impact_point.x - impact_point.x) < 1f)
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
        all_debris.Add(this);
        
    }
    
    void InitTrajectory()
    {
        tr.enabled = false;
        direction = Vector2.down;
        direction.x = Random.Range(.3f, 1.1f) * (Random.Range(0, 2) == 1 ? 1 : -1);
        Vector3 npos = transform.position;
        npos.x = Random.Range(0f, 4f) * (direction.x > 0f ? -1f : 1f);
        transform.position = npos;
        RaycastHit2D[] rh2d = Physics2D.RaycastAll(transform.position, direction, Mathf.Infinity);
        foreach (RaycastHit2D hit in rh2d)
        {
            if (hit.collider.gameObject.tag == "ground")
            {
                impact_point = hit.point;
                StartCoroutine(ShowImpactAndFly(transform.position, hit.point.Vector3() + (hit.point.Vector3() - transform.position).normalized * 2f));
                break;
            }
        }
    }
    Vector3 impact_point;
    LineRenderer lr { get { return GetComponent<LineRenderer>(); } }
    TrailRenderer tr { get { return GetComponent<TrailRenderer>(); } }
    ParticleSystem ps { get { return GetComponent<ParticleSystem>(); } }
    IEnumerator ShowImpactAndFly(Vector3 from, Vector3 to)
    {
        lr.SetPositions(new Vector3[] { to, from });
        while((lr.startWidth -= Time.deltaTime * .5f) > 0f)
        {
            yield return null;
        }
        moving = true;
        tr.enabled = true;
        lr.enabled = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        hilight = false;
        hilight_sprite.transform.Rotate(0f, 0f, 2f * 360f * Time.deltaTime);
        if (!alive)
        {
            return;
        }
        if (countdown)
        {
            
            if((until_explosion -= Time.deltaTime) <= 0f)
            {
                StartCoroutine(Explode());
                Destroy(cd.gameObject);
                GM.Explode();
                countdown = false;
            }
            if(cd.current > 1 && Mathf.CeilToInt(until_explosion) < cd.current)
            {
                if(GM.player.nearby_active_debris == this && GM.player.interacting)
                {
                    until_explosion += 1f;
                }
                else
                {
                    cd.Substract();
                }
                
                GM.Tick();
            }

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

        ps.startColor = GM.explode_debris_color;
        alive = false;
        ps.Play();
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
            ps.Play();
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

    private void OnMouseDown()
    {
        if(cd != null && cd.current < cd.max && GM.player.recharges > 0)
        {
            GM.player.recharges--;
            cd.ModifyNumber(cd.max);
            while(until_explosion + 1f < orig_until_explosion)
            {
                until_explosion += 1f;
            }
            GM.audio.PlaySound("recharge", .5f, new FloatRange(.8f, 1.2f));
            GM.effects.PlayEffect("recharge", transform.position);
        }
    }

    public void Disarm()
    {
        alive = false;
        Destroy(cd.gameObject);
        StartCoroutine(DisarmStep());
        GM.audio.PlaySound("salvage", .5f, new FloatRange(.8f, 1.2f));
        all_debris.Remove(this);
    }

    IEnumerator DisarmStep()
    {
        GM.effects.PlayEffect("salvage", transform.position, true);
        while(sr.color.a > 0f)
        {
            sr.color -= Color.black * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    

}
