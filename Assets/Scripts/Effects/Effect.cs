using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    public void Play() { StartCoroutine(PlayStep()); }
    protected abstract IEnumerator PlayStep();

    
}
