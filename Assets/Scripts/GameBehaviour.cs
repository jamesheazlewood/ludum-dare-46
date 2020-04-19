using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState {
  TitleScreen,
  Dialogue,
  Playing,
  Shopping,
}

public class GameBehaviour : MonoBehaviour
{
  public GameState currentState = GameState.TitleScreen;
  public GameObject touchObject = null;
  public Text moneyText = null;
  public GameObject player = null;
  public Camera mainCamera = null;
  public bool touchDown = false;
  public bool touchDownLastFrame = false;
  public bool alphaFireDown = false;
  public SoundManager soundManager = null;
  public int money = 2000;

  private Transform playerPivot;
  private Vector3 offsetTargetPosition;
  private Vector3 shipTargetPosition;
  
  public float stateTimer = 0;

  // Start is called before the first frame update
  void Start()
  {
    stateTimer = 0;
    shipTargetPosition = player.transform.position;
    offsetTargetPosition = player.transform.position;
    playerPivot = player.transform.Find("Pivot");
  }

  // Update is called once per frame
  void Update()
  {
    stateTimer += Time.deltaTime;

    switch(currentState) {
      case GameState.TitleScreen: {
        UpdateTitleScreen();
        break;
      }
      case GameState.Dialogue: {
        UpdateTitleScreen();
        break;
      }
      case GameState.Playing: {
        UpdateGame();
        break;
      }
      case GameState.Shopping: {
        UpdateShop();
        break;
      }
      default: {
        UnusedState();
        break;
      }
    }
  }

  void UpdateShop() {
    Debug.Log("UpdateShop");
  }

  void MovePlayer(Vector3 targetPosition) {
    var moveSpeedCap = 0.1f;
    var playerPos = player.transform.position;

    // limit against screen
    if(targetPosition.x > 2.8f) {
      targetPosition.x = 2.8f;
    }
    if(targetPosition.x < -2.8f) {
      targetPosition.x = -2.8f;
    }

    // It's shit to be a square
    // TODO: deltaTime
    var xSpeed = (targetPosition.x - playerPos.x) / 16;
    if(xSpeed > moveSpeedCap) {
      xSpeed = moveSpeedCap;
    }
    if(xSpeed < -moveSpeedCap) {
      xSpeed = -moveSpeedCap;
    }

    var ySpeed = (targetPosition.y - playerPos.y) / 16;
    if(ySpeed > moveSpeedCap) {
      ySpeed = moveSpeedCap;
    }
    if(ySpeed < -moveSpeedCap) {
      ySpeed = -moveSpeedCap;
    }

    playerPos.x += xSpeed;
    playerPos.y += ySpeed;

		player.transform.position = new Vector3(playerPos.x, playerPos.y, player.transform.position.z);
    // roll player ship graphics
    playerPivot.rotation = Quaternion.AngleAxis(-xSpeed * 300.0f, Vector3.up);
  }


  void UpdateGame() {
    // check if we are touching the screen
    var speed = 3.0f; // TODO: Forgot why this is here, used for Phone
    // Touch position is offscreen when not touching
    var touchPosition = new Vector3(-999, -999, 0);
    touchDown = false;
    if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
    {
      
      var touchDeltaPosition = Input.GetTouch(0).deltaPosition;
      touchPosition = new Vector3(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
      touchDown = true;
    }

    if(Input.GetButton("Fire1")) {
      touchPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
      touchDown = true;
    }

    // debug touch object
    var touchWorldPosition = mainCamera.ScreenToWorldPoint(touchPosition);
    touchWorldPosition.z = 0;
    touchObject.transform.position = touchWorldPosition;

    if(touchDown) {
      // Offset the target position from where user started tapping on the screen so you can control
      // the ship by touching and dragging anywhere
      if(!touchDownLastFrame) {
        offsetTargetPosition = shipTargetPosition - touchWorldPosition;
      }
      shipTargetPosition = offsetTargetPosition + touchWorldPosition;
      // Debug.Log("Touching");
    }

    MovePlayer(shipTargetPosition);

    touchDownLastFrame = touchDown;
  }

  void UnusedState() {
    // reset to title screen oif state goes weird
    ChangeState(GameState.TitleScreen);
  }

  void UpdateTitleScreen() {
    
  }

  void ChangeState(GameState newScene) {
    stateTimer = 0;
    currentState = newScene;
  }
}
