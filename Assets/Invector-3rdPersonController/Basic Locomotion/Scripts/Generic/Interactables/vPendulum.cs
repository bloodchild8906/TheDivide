using UnityEngine;
using System.Collections;
namespace Invector
{
    public class vPendulum : MonoBehaviour
    {
        [Range(0.0f, 360.0f)]
        public float angle = 90.0f;
        [Range(0.0f, 4.0f)]
        public float speed = 1.5f;
        public float startDelay;
        Quaternion qStart, qEnd;
        private float startTime;
        private bool work;
        private bool working;
        IEnumerator Start()
        {
            qStart = PendulumRotation(angle);
            qEnd = PendulumRotation(-angle);
            yield return new WaitForSeconds(startDelay);
            work = true;
            //		while (true)
            //		{
            //			yield return new WaitForEndOfFrame();
            //			startTime += Time.deltaTime;
            //			transform.rotation = Quaternion.Lerp (qStart, qEnd,(Mathf.Sin(startTime * speed + Mathf.PI/2) + 1.0f)/ 2.0f);
            //		}
        }

        void FixedUpdate()
        {
            if (work)
            {

                if (!working)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, qEnd, speed);
                    if (Vector3.Distance(transform.rotation.eulerAngles, qEnd.eulerAngles) < 0.1f)
                    {

                        working = true;
                    }
                }
                else
                {
                    startTime += Time.deltaTime;
                    transform.rotation = Quaternion.Lerp(qStart, qEnd, (Mathf.Sin(startTime * speed + Mathf.PI / 2) + 1.0f) / 2.0f);
                }

            }

        }

        void resetTimer()
        {
            startTime = 0.0f;
        }

        Quaternion PendulumRotation(float _angle)
        {
            var rot = transform.rotation;
            var zAngle = rot.eulerAngles.z + _angle;

            if (zAngle > 180) zAngle -= 360;
            else if (zAngle < -180) zAngle += 360;

            rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, zAngle);
            return rot;
        }
    }
}
