using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttraction : MonoBehaviour
{
    public Transform Target;

    private ParticleSystem system;

    private static ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];

    void Update()
    {
        if (system == null) system = GetComponent<ParticleSystem>();

        var count = system.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            var particle = particles[i];

            float distance = Vector3.Distance(Target.position, particle.position);

            if (distance > 0.01f)
            {
                particle.position = Vector3.Lerp(particle.position, Target.position, Time.deltaTime / 2.0f);

                particles[i] = particle;
            }
        }

        system.SetParticles(particles, count);
    }
}