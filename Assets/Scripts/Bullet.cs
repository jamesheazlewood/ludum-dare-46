using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
  // Defaults set here, change in UI
  public int damage = 1;
  public float speed = 1;
  public bool alive = true;
  public float lifeTime = 2.0f;
  public string hitSoundName = "";

  private Animator animator = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;

  // Start is called before the first frame update
  void Start()
  {
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    animator = GetComponent<Animator>();
  }

  // Update is called once per frame
  void Update()
  {
    lifeTime -= Time.deltaTime;
    if(lifeTime < 0) {
      Destroy(gameObject);
    }

    if(alive) {
      transform.position += transform.up * speed * Time.deltaTime;
    }
  }

  public void Explode() {
    alive = false;
    Debug.Log("Explo");
    transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.back);
    animator.SetBool("Dead", true);
    soundManager.PlaySound(hitSoundName);
    lifeTime = 1.0f;
  }
}
