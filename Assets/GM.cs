using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    static GM inst;
    private void Start()
    {
        inst = this;
        Initialize();
        
    }
    void Initialize()
    {
        
    }
    public static AudioManager audio { get { return inst.GetComponentInChildren<AudioManager>(); } }
}
