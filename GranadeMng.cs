using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class GranadeMng : MonoBehaviour
{
    //* Attach this script to explodable (granade) objects.

    int delaySeconds = 1, explosionRadius = 15, explosionForce = 2000;
    bool removePin, pimRemoved;
    Transform granadeTransform;
    Rigidbody granadeRb, effectedFromExplosionRb;
    PlayerInteractionMng effectedFromExplosionPIM;
    [SerializeField] GameObject explosionEffectPrefabObj;

    public bool RemovePin
    {
        get => removePin;
        set { removePin = value; }
    }

    void Start()
    {
        granadeTransform = transform;
        granadeRb = GetComponent<Rigidbody>();
        granadeRb.interpolation = RigidbodyInterpolation.Interpolate;
        granadeRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
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
        Instantiate(explosionEffectPrefabObj, granadeTransform.position, granadeTransform.rotation);
        Collider[] colliders = Physics.OverlapSphere(granadeTransform.position, explosionRadius);

        foreach (Collider collider in colliders)
        {
            effectedFromExplosionRb = collider.GetComponent<Rigidbody>();

            if (!effectedFromExplosionRb)
            {
                effectedFromExplosionRb = collider.GetComponentInParent<Rigidbody>();

                if (effectedFromExplosionRb) // Hareket edebilen ama Collider'ın bulunduğu yerde değil de Parent'ında Rigidbody'si bulunan objeler için
                {
                    effectedFromExplosionRb.AddExplosionForce(explosionForce, granadeTransform.position, explosionRadius);

                    if (effectedFromExplosionRb.gameObject.tag.Equals("Player")) // Oyuncu için
                    {
                        effectedFromExplosionPIM = effectedFromExplosionRb.GetComponent<PlayerInteractionMng>();

                        if (granadeRb.Equals(effectedFromExplosionPIM.GrabbedObjRb)) // Oyuncunun elinde tutmuş olduğu bu patlayan objeyi bırakması için
                        {
                            effectedFromExplosionPIM.ReleaseObj();
                        }

                        effectedFromExplosionRb.GetComponent<PlayerMovementMng>().PlayerHealthDecrease += explosionForce / ((int)(effectedFromExplosionRb.position - granadeTransform.position).magnitude * 100);
                    }
                }
            }
            else // Hareket edebilen ve Collider'ın bulunduğu yerde Rigidbody'si bulunan objeler için
            {
                effectedFromExplosionRb.AddExplosionForce(explosionForce, granadeTransform.position, explosionRadius);
            }

            effectedFromExplosionRb = null;
        }

        Destroy(gameObject);
    }
}
