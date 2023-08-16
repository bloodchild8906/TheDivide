using UnityEngine;
using System.Collections;
namespace Invector
{
    public class vDestroyOnSleep : MonoBehaviour
    {
        IEnumerator Start()
        {
            var rigdb = GetComponent<Rigidbody>();
            var collider = GetComponent<Collider>();

            yield return transform.parent.gameObject.activeSelf;

            while (!rigdb.IsSleeping())
                yield return new WaitForSeconds(2f);

            Destroy(rigdb);
            if (collider)
                Destroy(collider);
        }
    }
}