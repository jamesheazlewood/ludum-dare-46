using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Character {
  Pixie,
  Alpha,
}

public enum Guns {
  CometBlaster,
  FryLaser,
  MissileRack,
  PulseCannon,
  Blueberry,
  MicroMissiles,
  StarDestroyer,
  Inactive,
}

[Serializable]
public class GunStats {
  public string name = "Comet";
  public string shootSoundName = "Shoot 1";
  public string travelAnimationName = "Bullet Yellow";
  public string explosionAnimationName = "Bullet Yellow Explode";
  public string muzzleFlashAnimationName = "Yellow Muzzle Flash";
  public float speed = 8.0f;
  public int damage = 1;
  public float delay = 0.5f;
}

public class Human : MonoBehaviour
{
  public int maxHealth = 5;
  public int currentHealth = 5;
  public bool active = true;
  public Character character = Character.Pixie;

  public GameObject deathExplosion = null;
  private SoundManager soundManager = null;
  private GameBehaviour gameBehaviour = null;

  // Gun spots
  [SerializeField]
  public GunStats[] guns;

  public Guns[] gunSlots = null;
  public string[] gunNames = null;

  // Start is called before the first frame update
  void Start()
  {
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
    soundManager = gameBehaviour.soundManager;
    currentHealth = maxHealth;

    if(character == Character.Pixie) {
      gameBehaviour.pixieHudHealth.transform.Find("Slider").GetComponent<Slider>().maxValue = maxHealth;
      gameBehaviour.UpdatePixieHealth(currentHealth, maxHealth);
    } else {
      gameBehaviour.alphaHudHealth.transform.Find("Slider").GetComponent<Slider>().maxValue = maxHealth;
      gameBehaviour.UpdateAlphaHealth(currentHealth, maxHealth);
    }

    if(character == Character.Pixie) {
      AllocateGuns();
    }
  }

  Gun GetGun(string name) {
    return transform.Find("Pivot").transform.Find(name).GetComponent<Gun>();
  }

  // Sets all guns to inactive apart from passives - called from the shop
  public void ResetAllGuns() {
    // loop through gunNames as index i
    for(var s = 0; s < gunSlots.Length; s++) {
      if(s != 1 && s != 6 && s!= 7) {
        gunSlots[s] = Guns.Inactive;
      }
    }
  }

  // Update is called once per frame
  public void AllocateGuns() {
    // loop through gunNames as index i
    for(var i = 0; i < gunNames.Length; i++) {
      Gun gunStat = GetGun(gunNames[i]);
      Guns gunType = gunSlots[i];
      gunStat.active = false;
      if(gunType != Guns.Inactive) {
        gunStat.active = true;
        gunStat.stats = guns[(int)gunType];
      }
    }
  }

  public void Activate() {
    active = true;
    transform.Find("Coccoon").gameObject.SetActive(false);
  }

  public void Heal(int amount) {
    currentHealth += amount;
    if(currentHealth > maxHealth) {
      currentHealth = maxHealth;
    }
    if(character == Character.Pixie) {
      gameBehaviour.UpdatePixieHealth(currentHealth, maxHealth);
    } else {
      gameBehaviour.UpdateAlphaHealth(currentHealth, maxHealth);
    }
  }

  void Hurt(int damage) {
    currentHealth -= damage;
    soundManager.PlaySound("Hurt");

    if(character == Character.Pixie) {
      gameBehaviour.UpdatePixieHealth(currentHealth, maxHealth);
    } else {
      gameBehaviour.UpdateAlphaHealth(currentHealth, maxHealth);
    }

    if(currentHealth <= 0 && gameBehaviour.currentState == GameState.Playing) {
      Instantiate(deathExplosion, transform.position, transform.rotation);
      if(character == Character.Pixie) {
        soundManager.PlaySound("Pixie Die");
        gameBehaviour.pixieDieGameOver.SetActive(true);
      } else {
        soundManager.PlaySound("Alpha Die");
        gameBehaviour.alphaDieGameOver.SetActive(true);
      }
      gameBehaviour.ChangeState(GameState.Dialogue);
      gameObject.SetActive(false);
      gameBehaviour.levelSpeed = 0;
    }
  }

  // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
  void OnTriggerStay2D(Collider2D col)
  {
    if(active) {
      // Handle what happens whena bullet hits us
      if(col.gameObject.tag == "Bug Bullet") {
        Bullet b = col.GetComponent<Bullet>();
        if(b.alive) {
          b.Explode();
          Hurt(b.damage);
        }
      } else if(col.gameObject.tag == "Bug") {
        Enemy e = col.GetComponent<Enemy>();
        e.Hurt(1);
        Hurt(1);
      }
    }
  }
}
