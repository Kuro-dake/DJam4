﻿using System.Collections;
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
    
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunLevel(run_level);
        }
    }

    void RunLevel(int level)
    {
        StartCoroutine(RunLevelStep(level));
    }
    [SerializeField]
    Collider2D ground_collider;
    IEnumerator RunLevelStep(int level)
    {
        ground_collider.enabled = true;
        yield return new WaitForSeconds(generate_delay[level]);
        
        for (int i = 0; i<level_debris_numbers[level];i++)
        {
            Debris d = Instantiate(debris_prefab);
            
            int val = Mathf.CeilToInt(Mathf.Lerp(until_explosion_ranges[level].min, until_explosion_ranges[level].max, (float)i/(float)level_debris_numbers[level]));
            Debug.Log(val);
            d.until_explosion = val;


            yield return new WaitForSeconds(generate_delay[level]);
        }
        yield return new WaitForSeconds(3f);
        ground_collider.enabled = false;
        GM.Tick();
        Debris.StartCountdown();
        StartCoroutine(WatchDebris());
    }

    IEnumerator WatchDebris()
    {
        
        while(Debris.all_debris.Count > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        run_level++;
        if(run_level < 5)
        {
            StartCoroutine(RunLevelStep(run_level));
        }
        
    }
}