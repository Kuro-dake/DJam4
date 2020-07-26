using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisGenerator : MonoBehaviour
{
    [SerializeField]
    Debris debris_prefab;
    // Start is called before the first frame update
    [SerializeField]
    int run_level = 0;
    void Start()
    {
        RunLevel(run_level);
    }
    
    [SerializeField]
    FloatRange[] generate_delay;

    [SerializeField]
    IntRange[] until_explosion_ranges;

    [SerializeField]
    int[] level_debris_numbers;

    [SerializeField]
    string[] level_seeds;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debris.all_debris.Clear();
            GM.paused = false;
            Application.LoadLevel(0);
        }
    }

    void RunLevel(int level)
    {
        StartCoroutine(RunLevelStep(level));
    }
    [SerializeField]
    Collider2D ground_collider;
    Coroutine watch_debris_routine;
    List<Debris> GenerateDebrisFromSeed(int level)
    {

        int seed = Random.Range(int.MinValue, int.MaxValue);
        Debug.Log(seed);
        Random.InitState(-413498176);

        List<Debris> generated_debris = new List<Debris>();
        for (int i = 0; i < level_debris_numbers[level]; i++)
        {
            Debris d = Instantiate(debris_prefab);

            int ue = Mathf.CeilToInt(Mathf.Lerp(until_explosion_ranges[level].min, until_explosion_ranges[level].max, (float)i / (float)level_debris_numbers[level]));

            d.until_explosion = ue;
            
            generated_debris.Add(d);
        }
        return generated_debris;
    }
    IEnumerator RunLevelStep(int level)
    {
        playing = false;
        ground_collider.enabled = true;
        yield return new WaitForSeconds(generate_delay[level]);
        List<Debris> generated_debris = GenerateDebrisFromSeed(level);
        
        foreach (Debris d in generated_debris)
        {
            d.Initialize();
        }
        Debug.Log("-------------");
        yield return null;
        foreach (Debris d in generated_debris)
        {
            d.Fly();
            yield return new WaitForSeconds(generate_delay[level]);
        }
        
        yield return new WaitForSeconds(3f);
        ground_collider.enabled = false;
        GM.Tick();
        Debris.StartCountdown();
        GM.player.recharges = 3;
        GM.paused = true;
        if(watch_debris_routine == null)
        {
            watch_debris_routine = StartCoroutine(WatchDebris());
        }
        
    }
    public static bool playing { get; protected set; }
    IEnumerator WatchDebris()
    {
        playing = true;
        while(Debris.all_debris.Count > 0)
        {
            yield return null;
        }
        playing = false;

        yield return new WaitForSeconds(2f);
        run_level++;
        if(run_level < 5)
        {
            StartCoroutine(RunLevelStep(run_level));
        }
        watch_debris_routine = null;
        
    }
}
