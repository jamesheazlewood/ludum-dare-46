using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character {
  Pixie,
  Alpha,
}

public class Human : MonoBehaviour
{
  public int maxHealth = 5;
  public int currentHealth = 5;
  public Character character = Character.Pixie;

  private GameObject explosion = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;

  // Start is called before the first frame update
  void Start()
  {
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    currentHealth = maxHealth;
  }

  // Update is called once per frame
  void Update()
  {
      
  }

  void Hurt(int damage) {
    currentHealth -= damage;

    if(currentHealth <= 0) {
      Instantiate(explosion, transform.position, transform.rotation);
      if(character == Character.Pixie) {
        soundManager.PlaySound("Pixie Die");
      } else {
        soundManager.PlaySound("Alpha Die");
      }
      Destroy(gameObject);
    }
  }

  // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
  void OnTriggerStay2D(Collider2D col)
  {
    // Handle what happens whena bullet hits us
    if(col.gameObject.tag == "Bug Bullet") {
      Bullet b = col.GetComponent<Bullet>();
      if(b.alive) {
        b.Explode();
        Hurt(b.damage);
      }
    } else if(col.gameObject.tag == "Bug") {
      Hurt(1);
    }
    Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
  }
}
