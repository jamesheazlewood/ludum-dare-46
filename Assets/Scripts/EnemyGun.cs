using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : MonoBehaviour
{
  // Setup
  public GameObject bulletObject = null;
  public float delay = 2.0f;
  public bool autoTracking = false;
  public string animationName = "";
  private float timeBeforeFirstShot = 1.33f;

  // Hide
  private Animator animator = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;
  private Enemy enemy = null;
  private float timer = 0;

  // Start is called before the first frame update
  void Start()
  {
    timer = timeBeforeFirstShot;
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    animator = transform.Find("Muzzle Flash").GetComponent<Animator>();
    enemy = gameObject.GetComponentInParent<Enemy>();
    animator.StopPlayback();
  }

  // Update is called once per frame
  void Update()
  {
    if(enemy.agro) {
      timer -= Time.deltaTime;
    }

    // dfdf
    float angle = 0;
    if(autoTracking) {
      Vector3 vectorToTarget = gameBehaviour.shipTargetPosition - transform.position;
      angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
      Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
      transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 20.0f);
    }

    if(timer < 0) {
      timer = delay;
      soundManager.PlaySound("Enemy Shoot");
      GameObject ob = null;
      if(autoTracking) {
        ob = Instantiate(bulletObject, transform.position, transform.rotation);
      } else {
        ob = Instantiate(bulletObject, transform.position, transform.rotation * Quaternion.Euler(0, 0, 180f));
      }
      ob.GetComponent<Bullet>().tag = "Bug Bullet";
      animator.Play(animationName, -1, 0f);
    }
  }
}
