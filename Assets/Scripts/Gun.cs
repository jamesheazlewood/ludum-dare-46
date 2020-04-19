using System.Collections;
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
  }

  // Update is called once per frame
  void Update()
  {
    timer -= Time.deltaTime;

    if(timer < 0 && (
      (playerControlled && gameBehaviour.touchDown)
      || (!playerControlled && gameBehaviour.alphaFireDown)
    )) {
      timer = delay;
      soundManager.PlaySound(shootSoundName);
      Instantiate(bulletObject, transform.position + transform.up * 0.2f, transform.rotation);
      animator.Play(animationName, -1, 0f);
    }
  }
}
