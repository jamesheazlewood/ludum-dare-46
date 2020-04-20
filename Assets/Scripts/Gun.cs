﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
  // Setup
  public GameObject bulletObject = null;
  public float delay = 0.5f;
  public string shootSoundName = "";
  public string animationName = "";
  public bool playerControlled = true;
  public bool active = false;

  // Hide
  private Animator animator = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;
  private float timer = 0;

  // Start is called before the first frame update
  void Start()
  {
    timer = 0;
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    animator = transform.Find("Muzzle Flash").GetComponent<Animator>();
    animator.StopPlayback();
  }

  // Update is called once per frame
  void Update()
  {
    timer -= Time.deltaTime;
    var shooting = (playerControlled && gameBehaviour.touchDown)
        || (!playerControlled && gameBehaviour.alphaFireDown);

    if(timer < 0 && active && shooting) {
      timer = delay;
      soundManager.PlaySound(shootSoundName);
      Instantiate(bulletObject, transform.position + transform.up * 0.2f, transform.rotation);
      animator.Play(animationName, -1, 0f);
    }
  }
}
