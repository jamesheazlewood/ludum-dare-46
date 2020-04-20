using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MovePattern {
  DriftDown,
  DownThenIn,
  DownThenStay,
  Grounded,
}

public class Enemy : MonoBehaviour
{
  public bool agro = false;
  public bool isBoss = false; // triggers health bar
  public bool isSmall = false; // Smaller explosion
  public int health = 1;
  public MovePattern movePattern = MovePattern.DriftDown;
  public float speed = 1.0f;
  public int points = 100;
  public int directionX = -1;

  private int currentHealth = 1;
  private int stage = 0;
  private GameObject explosion = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;
  private float stageTimer = 0;
  private string explosionSound;

  // Start is called before the first frame update
  void Start()
  {
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    currentHealth = health;
    stageTimer = 0;

    if(isBoss) {
      explosion = gameBehaviour.explosionBoss;
      explosionSound = "Boss Explode";
    } else {
      if(isSmall) {
        explosion = gameBehaviour.explosionSmall;
        explosionSound = "Explode Small";
      } else {
        explosion = gameBehaviour.explosion;
        explosionSound = "Explode";
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    if(agro) {
      stageTimer += Time.deltaTime;

      switch(movePattern) {
        case MovePattern.DriftDown: {
          var newY = transform.position.y - speed * Time.deltaTime;

          transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
          );
          break;
        }
        case MovePattern.DownThenStay: {
          if(stage == 0) {
            var newY = transform.position.y - speed * Time.deltaTime;

            transform.position = new Vector3(
              transform.position.x,
              newY,
              transform.position.z
            );

            if(stageTimer > 3) {
              directionX = 1;
              NextStage();
            }
          }
          if(stage == 1) {
            var newY = transform.position.y + gameBehaviour.levelSpeed * Time.deltaTime;
            var newX = transform.position.x + directionX * Time.deltaTime;

            transform.position = new Vector3(newX,newY,transform.position.z);
          }

          break;
        }
        case MovePattern.DownThenIn: {
          if(stage == 1) {
            var newY = transform.position.y - speed * Time.deltaTime;

            transform.position = new Vector3(
              transform.position.x,
              newY,
              transform.position.z
            );
          }

          break;
        }
        default: {
          // Grounded
          break;
        }
      }
    }
  }

  void NextStage() {
    stageTimer = 0;
    stage += 1;
  }

  void Hurt(int damage) {
    currentHealth -= damage;

    if(currentHealth <= 0) {
      Instantiate(explosion, transform.position, transform.rotation);
      soundManager.PlaySound(explosionSound);
      gameBehaviour.AddPoints(points);
      Destroy(gameObject);
    }
  }

  // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
  void OnTriggerEnter2D(Collider2D col)
  {
    // Only do stuff if activated
    if(agro) {
      // Handle what happens whena bullet hits us
      if(col.gameObject.tag == "Bullet") {
        Bullet b = col.GetComponent<Bullet>();
        if(b.alive) {
          b.Explode();
          Hurt(b.damage);
          
          if(isBoss) {
            gameBehaviour.bossHudHealth.transform.Find("Slider").GetComponent<Slider>().value = currentHealth;
          }
        }
      } else if(col.gameObject.tag == "Human") {
        Hurt(1);
      } else if(col.gameObject.tag == "Enemy Exit Trigger") {
        if(isBoss) {
          gameBehaviour.bossHudHealth.SetActive(false);
        }
        Destroy(gameObject);
      }
    } else if(col.gameObject.tag == "Screen Trigger") {
      agro = true;
      if(isBoss) {
        gameBehaviour.bossHudHealth.SetActive(true);
      }
    }

    Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
  }
}
