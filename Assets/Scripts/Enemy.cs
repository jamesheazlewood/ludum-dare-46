using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MovePattern {
  DriftDown,
  DownThenIn,
  DownThenStay,
  Grounded,
  DownThenUp,
}

public class Enemy : MonoBehaviour
{
  public bool agro = false;
  public bool isBoss = false; // triggers health bar
  public bool isSmall = false; // Smaller explosion
  public int health = 1;
  public MovePattern movePattern = MovePattern.DriftDown;
  public float speed = 1.0f;
  public float xSpeed = 0.0f;
  public int points = 100;
  public int directionX = -1;
  public bool alive = true;

  private int currentHealth = 1;
  private int stage = 0;
  private GameObject explosion = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;
  private float stageTimer = 0;
  private string explosionSound;
  private Slider bossSlider = null;

  // Start is called before the first frame update
  void Start()
  {
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    currentHealth = health;
    bossSlider = gameBehaviour.bossHudHealth.transform.Find("Slider").GetComponent<Slider>();
    stageTimer = 0;
    alive = true;

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
      var newX = transform.position.x;
      var newY = transform.position.y;

      switch(movePattern) {
        case MovePattern.DriftDown: {
          newY -= speed * Time.deltaTime;
          break;
        }
        case MovePattern.DownThenStay: {
          if(stage == 0) {
            newY -= speed * Time.deltaTime;

            if(stageTimer > 3) {
              directionX = 1;
              if(isBoss) {
                gameBehaviour.levelSpeed = 0;
              }
              NextStage();
            }
          }

          if(stage == 1) {
            newY -= gameBehaviour.levelSpeed * Time.deltaTime;
            newX += directionX * Time.deltaTime;

            if(transform.position.x > 2.0f) {
              directionX = -1;
            }
            if(transform.position.x < -2.0f) {
              directionX = 1;
            }
          }

          break;
        }
        case MovePattern.DownThenIn: {
          newY -= speed * Time.deltaTime;
          if(stageTimer > 3) {
            if(newX > 0) {
              directionX = -1;
            } else {
              directionX = 1;
            }
            NextStage();
          }
          if(stage == 1) {
            newX += speed * directionX * Time.deltaTime;
          }

          break;
        }
        case MovePattern.DownThenUp: {
          speed -= stageTimer * 2.7f * Time.deltaTime;
          if(speed < -6.0f) {
            speed = -6.0f;
          }
          newY -= speed * Time.deltaTime;

          break;
        }
        default: {
          // Grounded
          break;
        }
      }

      transform.position = new Vector3(newX, newY, transform.position.z);
    }
  }

  void NextStage() {
    stageTimer = 0;
    stage += 1;
  }

  public void Hurt(int damage) {
    if(alive) {
      currentHealth -= damage;

      if(currentHealth <= 0) {
        Instantiate(explosion, transform.position, transform.rotation);
        soundManager.PlaySound(explosionSound);
        gameBehaviour.AddPoints(points);
        alive = false;

        if(isBoss) {
          RemoveBossHealth();
          gameBehaviour.ExplodeAllBugBullets();
          gameBehaviour.DamageAllAgroBugs(999999);
          gameBehaviour.StartEndLevelSequence();
        }

        Destroy(gameObject);
      }
    }
  }

  void InitBossHealth() {
    gameBehaviour.bossHudHealth.SetActive(true);
    bossSlider.maxValue = currentHealth;
    bossSlider.value = currentHealth;
  }

  void RemoveBossHealth() {
    gameBehaviour.bossHudHealth.SetActive(false);
  }

  // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
  void OnTriggerStay2D(Collider2D col)
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
            bossSlider.value = currentHealth;
          }
        }
      } else if(col.gameObject.tag == "Human") {
        Hurt(1);
      } else if(col.gameObject.tag == "Enemy Exit Trigger") {
        if(isBoss) {
          RemoveBossHealth();
        }
        Destroy(gameObject);
      }
    } else if(col.gameObject.tag == "Screen Trigger") {
      agro = true;
      if(isBoss) {
        InitBossHealth();
      }
    }

    // Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
  }
}
