using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [SerializeField]
    List<NamedEffect> effects = new List<NamedEffect>();
    // Start is called before the first frame update
    public Effect PlayEffect(string name, Vector3 position, bool play = true)
    {
        Effect ret = Instantiate(effects.Find(delegate (NamedEffect ne) { return ne.first == name; }).second, position, Quaternion.identity).GetComponent<Effect>();
        if (play)
        {
            ret.Play();
        }
        return ret;
    }
}
