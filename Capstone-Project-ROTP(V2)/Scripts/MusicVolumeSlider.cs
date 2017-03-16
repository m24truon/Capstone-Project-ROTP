using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour {

	public Slider Volume;
	public AudioSource MainMenuMusic;
	
	// Update is called once per frame
	void Update () {
		MainMenuMusic.volume = Volume.value;
	}
}
