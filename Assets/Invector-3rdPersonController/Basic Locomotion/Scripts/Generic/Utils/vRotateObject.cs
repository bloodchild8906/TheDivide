using UnityEngine;
using System.Collections;
namespace Invector
{
    public class vRotateObject : MonoBehaviour
    {
        public Vector3 rotationSpeed;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}