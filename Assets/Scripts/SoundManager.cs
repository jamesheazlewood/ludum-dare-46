using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
  public GameObject soundLibrary;
  
  // Start is called before the first frame update
  void Start()
  {
		//
		soundLibrary = transform.Find("Sounds").gameObject;
  }

	// Play sound
	public void PlaySound(string soundName)
	{
		AudioSource a = soundLibrary.transform.Find(soundName).GetComponent<AudioSource>();

    if(a != null) {
      a.Play();
    } else {
      Debug.Log("Sound Failed: " + soundName);
    }
	}

  // Play sound
	public void StopSound(string soundName)
	{
		AudioSource a = soundLibrary.transform.Find(soundName).GetComponent<AudioSource>();

    if(a != null) {
      a.Stop();
    } else {
      Debug.Log("Sound Failed: " + soundName);
    }
	}
}
