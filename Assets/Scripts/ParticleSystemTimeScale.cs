using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemTimeScale : MonoBehaviour
{
    static ParticleSystemTimeScale inst;
    // Start is called before the first frame update
    void Start()
    {
        inst = this;
    }

    public static bool scaled
    {
        set
        {
            foreach(ParticleSystem ps in inst.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule mm = ps.main;
                mm.useUnscaledTime = !value;
            }
        }
    }

}
