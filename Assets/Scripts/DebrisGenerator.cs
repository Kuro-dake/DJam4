using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisGenerator : MonoBehaviour
{
    [SerializeField]
    Debris debris_prefab;
    // Start is called before the first frame update
    
    public int run_level = 0;
    
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

       
    }
    Coroutine run_level_routine = null;
    public void RunLevel(int level)
    {
        run_level_routine = StartCoroutine(RunLevelStep(level));
    }
    public void StopRunLevel()
    {
        if(run_level_routine != null)
        {
            StopCoroutine(run_level_routine);
        }
    }
    public void RestartLevel()
    {
        RunLevel(run_level);
    }
    [SerializeField]
    Collider2D ground_collider;
    [SerializeField]
    int[] trash_debris_num;
    Coroutine watch_debris_routine;
    List<Debris> GenerateDebrisFromSeed(int level, int seed)
    {

        //seed = Random.Range(int.MinValue, int.MaxValue);
        //Debug.Log(seed);
        Random.InitState(seed);

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
    IEnumerator GenerateTrashDebris(int level)
    {

        List<Debris> trash = new List<Debris>();
        for (int i = 0; i < trash_debris_num[level]; i++)
        {
            Debris d = Instantiate(debris_prefab);
            d.persitent = false;
            
            trash.Add(d);
            
            
        }

        foreach(Debris d in trash)
        {
            d.Initialize();
            yield return new WaitForSeconds(generate_delay[level] / 2f); ;
            d.Fly();
        }

        
    }
    IEnumerator RunLevelStep(int level)
    {
        Debris.salvaged = 0;
        Debris.total = 0;
        run_level = level;
        int current_level = level;    
        foreach (string level_seed_string in level_seeds[current_level].Split(new char[] { '|' }))
        {

            int level_seed = int.Parse(level_seed_string);
            playing = false;
            ground_collider.enabled = true;
            yield return new WaitForSeconds(generate_delay[current_level]);
            List<Debris> generated_debris = GenerateDebrisFromSeed(current_level, level_seed);
            
            foreach (Debris d in generated_debris)
            {
                d.Initialize();
            }
                
            yield return null;
            foreach (Debris d in generated_debris)
            {
                d.Fly();
                yield return new WaitForSeconds(generate_delay[current_level]);
            }
            Coroutine trash_routine = StartCoroutine(GenerateTrashDebris(current_level));
            yield return trash_routine;

            yield return new WaitForSeconds(3f);
            ground_collider.enabled = false;
            GM.Tick();
            Debris.StartCountdown();
            GM.player.recharges = current_level >= 3 ? 3 : 0;
            GM.paused = true;

            playing = true;
            while (Debris.all_debris.Count > 0)
            {
                yield return null;
            }
            playing = false;

            yield return new WaitForSeconds(2f);

            
            
        }
        run_level++;
        GM.GameOver();

    }
    public static bool playing { get; protected set; }
    
}
