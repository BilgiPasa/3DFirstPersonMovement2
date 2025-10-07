using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class GranadeManager : MonoBehaviour
{
    //* Attach this script to explodable (granade) objects.

    int delaySeconds = 1, explosionRadius = 15, explosionForce = 2000;
    bool removePin, pimRemoved;
    Transform granadeTransform;
    Rigidbody granadeRigidbody, objectRigidbodyThatEffectedFromExplosion;
    PlayerInteractionManager playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion;
    [SerializeField] GameObject explosionEffectPrefabObject;

    public bool RemovePin
    {
        get => removePin;
        set { removePin = value; }
    }

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

                if (objectRigidbodyThatEffectedFromExplosion) // Hareket edebilen ama Collider'ın bulunduğu yerde değil de Parent'ında Rigidbody'si bulunan objeler için
                {
                    objectRigidbodyThatEffectedFromExplosion.AddExplosionForce(explosionForce, granadeTransform.position, explosionRadius);

                    if (objectRigidbodyThatEffectedFromExplosion.gameObject.tag.Equals("Player")) // Oyuncu için
                    {
                        playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion = objectRigidbodyThatEffectedFromExplosion.GetComponent<PlayerInteractionManager>();

                        if (granadeRigidbody.Equals(playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion.GrabbedObjectRigidbody)) // Oyuncunun elinde tutmuş olduğu bu patlayan objeyi bırakması için
                        {
                            playerInteractionManagerScriptFromPlayerThatEffectedFromExplosion.ReleaseObject();
                        }

                        objectRigidbodyThatEffectedFromExplosion.GetComponent<PlayerMovementManager>().PlayerHealthDecrease += explosionForce / ((int)(objectRigidbodyThatEffectedFromExplosion.position - granadeTransform.position).magnitude * 100);
                    }
                }
            }
            else // Hareket edebilen ve Collider'ın bulunduğu yerde Rigidbody'si bulunan objeler için
            {
                objectRigidbodyThatEffectedFromExplosion.AddExplosionForce(explosionForce, granadeTransform.position, explosionRadius);
            }

            objectRigidbodyThatEffectedFromExplosion = null;
        }

        Destroy(gameObject);
    }
}
