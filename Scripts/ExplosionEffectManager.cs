using System.Collections;
using UnityEngine;

public class ExplosionEffectManager : MonoBehaviour
{
    //* Attach this script to the explosion effect prefab that you are using for explodable (granade) objects.

    void Start()
    {
        StartCoroutine(destroyAfterExplosionEnds());
    }

    IEnumerator destroyAfterExplosionEnds()
    {
        yield return new WaitForSeconds(gameObject.GetComponent<ParticleSystem>().main.duration);
        Destroy(gameObject);
    }
}
