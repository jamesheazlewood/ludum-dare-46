using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovePattern {
  DriftDown,
  DownThenIn,
  DownThenStay,
}

public class Enemy : MonoBehaviour
{
  public bool agro = false;
  public int health = 1;
  public MovePattern movePattern = MovePattern.DriftDown;
  public float speed = 1.0f;
  public int stage = 0;

  // Start is called before the first frame update
  void Start()
  {
    
  }

  // Update is called once per frame
  void Update()
  {
    if(agro) {
      switch(movePattern) {
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
          var newY = transform.position.y - speed * Time.deltaTime;

          transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
          );
          break;
        }
      }
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
        }
      } else if(col.gameObject.tag == "Human") {
        Bullet b = col.GetComponent<Bullet>();
        if(b.alive) {
          b.Explode();
        }
      } else if(col.gameObject.tag == "Enemy Exit Trigger") {
        Destroy(gameObject);
      }
    } else if(col.gameObject.tag == "Screen Trigger") {
      agro = true;
    }

    Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
  }
}
