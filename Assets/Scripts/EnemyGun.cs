using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : MonoBehaviour
{
  // Setup
  public GameObject bulletObject = null;
  public float delay = 2.0f;
  public bool autoTracking = false;
  public string shootSoundName = "";
  public string animationName = "";

  // Hide
  private Animator animator = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;
  private Enemy enemy = null;
  private float timer = 0;

  // Start is called before the first frame update
  void Start()
  {
    timer = 0;
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    animator = transform.Find("Muzzle Flash").GetComponent<Animator>();
    enemy = transform.parent.GetComponent<Enemy>();
    animator.StopPlayback();
  }

  // Update is called once per frame
  void Update()
  {
    timer -= Time.deltaTime;

    if(timer < 0) {
      timer = delay;
      soundManager.PlaySound(shootSoundName);
      var ob = Instantiate(bulletObject, transform.position + transform.up * 0.2f, transform.rotation);
      ob.GetComponent<Bullet>().tag = "Bug Bullet";
      animator.Play(animationName, -1, 0f);
    }
  }
}
