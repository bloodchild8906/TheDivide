using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vDisplayValueFade : MonoBehaviour
{
    public CanvasGroup group;
    public AnimationCurve groupAlphaCurve;
    public float upSpeed;

    public float timeToDestroy = 4f;
    float currentTime;
    Transform rotateTransform;
    void Awake()
    {
        group.alpha = 0;
    }
    public void Update()
    {
        if (rotateTransform == null)
        {
            if (Camera.current)
            {
                rotateTransform = Camera.current.transform;
                transform.forward = rotateTransform.position - transform.position;
                group.alpha = 1;
            }
            else group.alpha = 0;
            return;
        }

        transform.Translate(Vector3.up * upSpeed * Time.deltaTime);

        transform.forward = rotateTransform.position - transform.position;
        currentTime += Time.deltaTime;
        var eval = currentTime / timeToDestroy;

        if (group) group.alpha = groupAlphaCurve.Evaluate(1f - eval);

        if (currentTime >= timeToDestroy) Destroy(gameObject);
    }
}
