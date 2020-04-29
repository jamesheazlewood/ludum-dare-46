using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
  // Setup
  public GameObject bulletObject = null;
  public GunStats stats;
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
      timer = stats.delay;
      soundManager.PlaySound(stats.shootSoundName);
      var bullet = Instantiate(bulletObject, transform.position + transform.up * 0.6f, transform.rotation);
      var bulletScript = bullet.GetComponent<Bullet>();
      bulletScript.travelAnimationName = stats.travelAnimationName;
      bulletScript.explosionAnimationName = stats.explosionAnimationName;
      bulletScript.speed = stats.speed;
      bulletScript.damage = stats.damage;
      animator.Play(stats.muzzleFlashAnimationName, -1, 0f);
    }
  }
}
