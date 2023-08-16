using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class ParticleSound : MonoBehaviour {

	public AudioClip OnBirthSound;
	public ParticleSystem emitter;

	private int particleNum;
	//private AudioSource audio;

	// Use this for initialization
	void Start () {
		//AudioSource audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

		if (particleNum < emitter.particleCount){
			GetComponent<AudioSource>().clip = OnBirthSound;
			GetComponent<AudioSource>().Play();
		}

		particleNum = emitter.particleCount;
	}
}
