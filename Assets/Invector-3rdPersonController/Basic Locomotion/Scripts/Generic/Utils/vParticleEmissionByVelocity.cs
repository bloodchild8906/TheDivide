using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vParticleEmissionByVelocity : MonoBehaviour
{
    public Rigidbody rigidbodyReference;
    public ParticleSystem particle;

    ParticleSystem.EmissionModule emmision;

   
    public bool normalizeVelocity = true;

    public bool inverse;
    float rate;
    private void Start()
    {
        emmision = particle.emission;
        rate = emmision.rateOverTime.constant;
    }
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        var velocity = rigidbodyReference.velocity;
        velocity.y = 0;
        float magnitude = velocity.magnitude;

        float speed = 0f;
        if (normalizeVelocity)
        {
            speed = velocity.normalized.magnitude * Mathf.Clamp(magnitude, 0, 1f);
        }
        else speed = magnitude;
        if(inverse)
            emmision.rateOverTime = rate - (rate* speed);
        else emmision.rateOverTime = rate * speed;

    }
}
