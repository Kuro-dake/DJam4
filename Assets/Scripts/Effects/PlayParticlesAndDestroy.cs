using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class PlayParticlesAndDestroy : Effect
{
    ParticleSystem ps { get { return GetComponent<ParticleSystem>(); } }
    protected override IEnumerator PlayStep()
    {
        yield return null;
        while (ps.isPlaying || ps.particleCount > 0)
        {
            yield return null;
        }
        Destroy(gameObject);

    }
}
