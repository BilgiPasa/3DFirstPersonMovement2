using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class GranadeManager : MonoBehaviour
{
    [HideInInspector] public bool removePin;
    int delaySeconds = 1, explosionRadius = 15, explosionForce = 2000;
    bool pimRemoved;
    Transform granadeTransform;
    Rigidbody granadeRigidbody, objectRigidbodyThatEffectedFromExplosion;
    PlayerInteractionManager playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion;
    [SerializeField] GameObject explosionEffectPrefabObject;

    void Start()
    {
        granadeTransform = transform;
        granadeRigidbody = GetComponent<Rigidbody>();
        granadeRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        granadeRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        if (removePin && !pimRemoved)
        {
            pimRemoved = true;
            StartCoroutine(DelayingExplosion());
        }
    }

    IEnumerator DelayingExplosion()
    {
        yield return new WaitForSeconds(delaySeconds);
        Explode();
    }

    void Explode()
    {
        Instantiate(explosionEffectPrefabObject, granadeTransform.position, granadeTransform.rotation);
        Collider[] colliders = Physics.OverlapSphere(granadeTransform.position, explosionRadius);

        foreach (Collider collider in colliders)
        {
            objectRigidbodyThatEffectedFromExplosion = collider.GetComponent<Rigidbody>();

            if (!objectRigidbodyThatEffectedFromExplosion)
            {
                objectRigidbodyThatEffectedFromExplosion = collider.GetComponentInParent<Rigidbody>();

                if (objectRigidbodyThatEffectedFromExplosion)
                {
                    objectRigidbodyThatEffectedFromExplosion.AddExplosionForce(explosionForce, granadeTransform.position, explosionRadius);

                    if (objectRigidbodyThatEffectedFromExplosion.gameObject.tag == "Player") // For player
                    {
                        playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion = objectRigidbodyThatEffectedFromExplosion.GetComponent<PlayerInteractionManager>();

                        if (granadeRigidbody.Equals(playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion.grabbedObjectRigidbody)) // To relese the object that player is holding when the object explodes
                        {
                            playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion.ReleaseObject();
                        }

                        objectRigidbodyThatEffectedFromExplosion.GetComponent<PlayerMovementManager>().playerHealthDecrease += explosionForce / ((int)(objectRigidbodyThatEffectedFromExplosion.position - granadeTransform.position).magnitude * 100);
                    }
                }
            }
            else // For movable objects
            {
                objectRigidbodyThatEffectedFromExplosion.AddExplosionForce(explosionForce, granadeTransform.position, explosionRadius);
            }

            objectRigidbodyThatEffectedFromExplosion = null;
        }

        Destroy(gameObject);
    }
}
