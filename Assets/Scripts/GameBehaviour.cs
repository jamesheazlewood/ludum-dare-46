using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState {
  TitleScreen,
  Dialogue,
  Playing,
  Shopping,
}

public enum DialogueCharacter {
  Pixie,
  Alpha,
}

public struct Dialogue {
  public DialogueCharacter character;
  public string text;
  public int expressionIndex;
}

public class GameBehaviour : MonoBehaviour
{
  public GameState currentState = GameState.TitleScreen;
  public GameObject touchObject = null;
  public GameObject player = null;
  public GameObject alpha = null;
  public GameObject alphaHudPic = null;
  public GameObject pixieHudPic = null;
  public GameObject alphaHudHealth = null;
  public GameObject pixieHudHealth = null;
  public GameObject bossHudHealth = null;
  public GameObject explosion = null;
  public GameObject explosionSmall = null;
  public GameObject explosionBoss = null;
  public GameObject dialogueHud = null;
  public GameObject shopHud = null;
  public GameObject levelNumberHud = null;
  public Text dialogueHudText = null;
  public RectTransform dialogueHudPixieIcon = null;
  public Image dialogueHudPixieImage = null;
  public RectTransform dialogueHudAlphaIcon = null;
  public Image dialogueHudAlphaImage = null;
  public GameObject titleScreenHud = null;
  public GameObject fadeOut = null;
  public GameObject gameUiHud = null;
  public Camera mainCamera = null;
  public bool touchDown = false;
  public bool touchDownLastFrame = false;
  public bool alphaFireDown = false;
  public SoundManager soundManager = null;
  public int money = 2000;
  public int bombs = 1;
  public int level = 1;
  public GameObject[] levelGroups = null;
  [SerializeField]
  public LevelDifficulty[] levelDifficulties = null;
  public float levelSpeed = 1.0f;
  public float maxLevelDistance = 100.0f;
  private List<List<Dialogue>> dialogues;
  public int currentDialogueGroupIndex = 0;
  public int currentDialogueIndex = 0;
  public int currentTutorialIndex = 0;
  public GameObject[] tutorialScreens = null;
  public GameObject pixieDieGameOver = null;
  public GameObject alphaDieGameOver = null;

  public TextAsset dialogueTextFile = null;
  public Sprite[] pixieHudExpressions = null;
  public Sprite[] alphaHudExpressions = null;

  public GameObject[] shopPages = null; 
  public int shopPage = 0;

  public Text panicBombsHud = null;
  public Text moneyText = null;

  private Transform playerPivot;
  private Vector3 offsetTargetPosition;
  public Vector3 shipTargetPosition;

  private bool waitingForBossExplosionToFinish = false;
  private float sequenceTimer = 0.0f;

  public float alphaDirection = 1;

  [Serializable]
  public struct LevelDifficulty {
    public float speed;          // Base: 1.666
    public float shootMult;      // simple multiply with float
    public float scoreMult;      // round to 10 eg.!-- 613.324 = 610
    public float moveSpeedMult;  // Base: 1.00 - simple multiply with float
    public float healthMult;     // Base: 1.00 - Round to 1 eg. 4.924 = 4
    public float bossHealthMult; // Base: 1.00 - Round to 1 eg. 4.924 = 4
  }
  
  public float stateTimer = 0;

  // Start is called before the first frame update
  void Start()
  {
    Application.targetFrameRate = 60;
    stateTimer = 0;
    shipTargetPosition = player.transform.position;
    offsetTargetPosition = player.transform.position;
    playerPivot = player.transform.Find("Pivot");
    SetMoneyText();
    alphaHudPic.SetActive(false);
    alphaHudHealth.SetActive(false);
    bossHudHealth.SetActive(false);
    titleScreenHud.SetActive(true);

    // Read dialogue
    string text = dialogueTextFile.text;
    string[] lines = text.Split("\n" [0]);

    // Create list of dialogue arrays
    dialogues = new List<List<Dialogue>>();
    List<Dialogue> dialogueGroup = new List<Dialogue>();

    for(int i = 0; i < lines.Length - 1; i++) {
      var line = lines[i];
      string[] segments = line.Split(";" [0]);

      if(segments[0] == "e") {
        dialogues.Add(dialogueGroup);
        dialogueGroup = new List<Dialogue>();
      } else {
        // Not "e"
        Dialogue dialogue = new Dialogue();
        dialogue.character = segments[0] == "p" ? DialogueCharacter.Pixie : DialogueCharacter.Alpha;
        dialogue.text = segments[2];
        dialogue.expressionIndex = 2; // 3: nutral
        if(segments[1] == "h") {
          dialogue.expressionIndex = 3; // excited - Only pixie
        }
        if(segments[1] == "s") {
          dialogue.expressionIndex = 1; // sad
        }
        if(segments[1] == "b") {
          dialogue.expressionIndex = 0; //  bad
        }

        dialogueGroup.Add(dialogue);
      }
    }
    
    RefreshLevel();
    // Debug.Log(dialogues[0][0].text);
    // Debug.Log(dialogues[0][1].text);
    // Debug.Log(dialogues[1][0].text);
  } 

  void SetMoneyText() {
    moneyText.text = money.ToString();
  }

  void UpdateBombText() {
    panicBombsHud.text = "x" + bombs.ToString();
  }

  public void StartEndLevelSequence() {
    sequenceTimer = 0;
    waitingForBossExplosionToFinish = true;
  }

  public void UpdatePixieHealth(int currentHealth, int maxHealth) {
    float unitHealth = (float)currentHealth / (float)maxHealth;
    Sprite profile = pixieHudExpressions[2];
    if(unitHealth < 0.51f) {
      profile = pixieHudExpressions[1];
    }
    if(unitHealth < 0.26f) {
      profile = pixieHudExpressions[0];
    }
    pixieHudPic.transform.Find("Profile").GetComponent<Image>().sprite = profile;
    pixieHudHealth.transform.Find("Slider").GetComponent<Slider>().value = currentHealth;
  }

  public void UpdateAlphaHealth(int currentHealth, int maxHealth) {
    float unitHealth = (float)currentHealth / (float)maxHealth;
    Sprite profile = alphaHudExpressions[2];
    if(unitHealth < 0.76f) {
      profile = alphaHudExpressions[1];
    }
    if(unitHealth < 0.51f) {
      profile = alphaHudExpressions[0];
    }
    
    alphaHudPic.transform.Find("Profile").GetComponent<Image>().sprite = profile;
    alphaHudHealth.transform.Find("Slider").GetComponent<Slider>().value = currentHealth;
  }

  // Update is called once per frame
  void Update()
  {
    stateTimer += Time.deltaTime;
    touchDown = false;

    switch(currentState) {
      case GameState.TitleScreen:
      case GameState.Dialogue:
      case GameState.Shopping: {
        // Do nothing
        break;
      }
      case GameState.Playing: {
        UpdateGame();
        break;
      }
      default: {
        UnusedState();
        break;
      }
    }
  }

  // Move the payload character
  void UpdateAlphaAI() {
    if(level > 1) {
      var alphaPos = alpha.transform.position;

      // Move alpha upwards with the camera
      alphaPos.x += alphaDirection * Time.deltaTime;
      alphaPos.y += levelSpeed * Time.deltaTime;

      if(alphaPos.x > 2.5f) {
        alphaDirection = -1;
      }
      if(alphaPos.x < -2.5f) {
        alphaDirection = 1;
      }

      alpha.transform.position = new Vector3(alphaPos.x, alphaPos.y, alpha.transform.position.z);
    }
  }

  // Player movement and easing
  void MovePlayer(Vector3 targetPosition) {
    var moveSpeedCap = 0.2f;
    var playerPos = player.transform.position;

    // limit against screen
    if(targetPosition.x > 2.8f) {
      targetPosition.x = 2.8f;
    }
    if(targetPosition.x < -2.8f) {
      targetPosition.x = -2.8f;
    }
    if(targetPosition.y < mainCamera.transform.position.y - 5.0f) {
      targetPosition.y = mainCamera.transform.position.y - 5.0f;
    }
    if(targetPosition.y > mainCamera.transform.position.y + 5.0f) {
      targetPosition.y = mainCamera.transform.position.y + 5.0f;
    }

    // It's shit to be a square
    var xSpeed = (targetPosition.x - playerPos.x) * Time.deltaTime * 3.75f;
    if(xSpeed > moveSpeedCap) {
      xSpeed = moveSpeedCap;
    }
    if(xSpeed < -moveSpeedCap) {
      xSpeed = -moveSpeedCap;
    }

    var ySpeed = (targetPosition.y - playerPos.y) * Time.deltaTime * 3.75f;
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

  // Main gameplay loop
  void UpdateGame() {
    // Move camera and player up screen
    var cameraY = mainCamera.transform.position.y;
    cameraY += levelSpeed * Time.deltaTime;

    // Stop screen moving when hit end of level
    if(cameraY > maxLevelDistance) {
      cameraY = maxLevelDistance;
      levelSpeed = 0;
    }

    mainCamera.transform.position = new Vector3(
      mainCamera.transform.position.x,
      cameraY,
      mainCamera.transform.position.z
    );

    if(waitingForBossExplosionToFinish) {
      sequenceTimer += Time.deltaTime;

      if(sequenceTimer > 2.5f) {
        waitingForBossExplosionToFinish = false;
        ChangeState(GameState.Dialogue);
        if(level == 1) {
          dialogueHud.SetActive(true);
          currentDialogueGroupIndex = 1;
          currentDialogueIndex = 0;
          RefreshDialogueUi(); 
          alpha.GetComponent<Human>().Activate();
        } else {
          OpenShop();
        }
      }
    }

    UpdateAlphaAI();
    UpdatePlayerAndTouch();
  }

  // Player and touch stuff
  void UpdatePlayerAndTouch() {
    var speed = 3.0f; // TODO: Forgot why this is here, used for Phone

    // Touch position is offscreen when not touching
    var touchPosition = new Vector3(-999, -999, 0);
    if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
      var touchDeltaPosition = Input.GetTouch(0).deltaPosition;
      touchPosition = new Vector3(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
      touchDown = true;
    }

    if(Input.GetButton("Fire1")) {
      touchPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
      touchDown = true;
    }

    Time.timeScale = 1;
    if(Input.GetButton("Fire2")) {
      Time.timeScale = 5;
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

    // Move player upwards with the camera
    shipTargetPosition.y += levelSpeed * Time.deltaTime;

    MovePlayer(shipTargetPosition);

    touchDownLastFrame = touchDown;
  }

  void UnusedState() {
    // reset to title screen if state goes weird
    ChangeState(GameState.TitleScreen);
  }

  public void AddPoints(int points) {
    money += points;
    SetMoneyText();
  }

  public void ChangeState(GameState newScene) {
    stateTimer = 0;
    currentState = newScene;
  }

  public void RefreshDialogueUi() {
    var currentDialogue = dialogues[currentDialogueGroupIndex][currentDialogueIndex];

    if(currentDialogue.character == DialogueCharacter.Pixie) {
      dialogueHudPixieIcon.gameObject.SetActive(true);
      dialogueHudAlphaIcon.gameObject.SetActive(false);
      dialogueHudPixieImage.sprite = pixieHudExpressions[currentDialogue.expressionIndex];
    } else {
      dialogueHudPixieIcon.gameObject.SetActive(false);
      dialogueHudAlphaIcon.gameObject.SetActive(true);
      dialogueHudAlphaImage.sprite = alphaHudExpressions[currentDialogue.expressionIndex];
    }

    dialogueHudText.text = currentDialogue.text;
  }

  public void ButtonClick() {
    soundManager.PlaySound("Menu Click");
  }

  public void StartIntro() {
    soundManager.StopSound("Music");

    ButtonClick();
    titleScreenHud.SetActive(false);
    dialogueHud.SetActive(true);

    RefreshDialogueUi();
  }

  public void NextDialogue() {
    ButtonClick();
    if(currentDialogueIndex < dialogues[currentDialogueGroupIndex].Count - 1) {
      currentDialogueIndex += 1;
      RefreshDialogueUi();
    } else {
      EndDialogue();
    }
  }

  // closes dialogue box, checks tutorials and iterates currentDialogueGroupIndex
  public void EndDialogue() {
    dialogueHud.SetActive(false);
    if(level == 1) {
      gameUiHud.SetActive(true);
    }
    if(currentDialogueGroupIndex == 1) {
      alphaHudPic.SetActive(true);
      alphaHudHealth.SetActive(true);
    }
    CheckTutorials();
  }

  public void CloseAllTutorials() {
    // Make sure all tutorials are off
    for(var i = 0; i < tutorialScreens.Length; i += 1) {
      tutorialScreens[i].SetActive(false);
    }
  }

  public void CheckTutorials() {
    CloseAllTutorials();
    if(level == 1 && currentTutorialIndex < 3) {
      tutorialScreens[currentTutorialIndex].SetActive(true);
    } else if(alpha.GetComponent<Human>().active && currentTutorialIndex < 4) {
      tutorialScreens[currentTutorialIndex].SetActive(true);
    } else {
      StartGame();
    }
  }

  public void NextTutorial() {
    ButtonClick();
    currentTutorialIndex += 1;
    CheckTutorials();
  }

  public void SkipTutorials() {
    ButtonClick();
    CloseAllTutorials();
    dialogueHud.SetActive(false);
    currentTutorialIndex = 3;
    StartGame();
  }

  public void SkipDialogue() {
    ButtonClick();
    EndDialogue();
  }
  
  public void StartGame() {
    ChangeState(GameState.Playing);
  }

  public void ResetGame() {
    // Reloads current scene
    Scene scene = SceneManager.GetActiveScene();
    SceneManager.LoadScene(scene.name);
  }

  public void DamageAllAgroBugs(int damageAmount) {
    var enemies = GameObject.FindGameObjectsWithTag("Bug");
    foreach(GameObject e in enemies) {
      var enemyScript = e.GetComponent<Enemy>();
      if(enemyScript && enemyScript.agro && enemyScript.alive) {
        enemyScript.Hurt(damageAmount);
      }
    }
  }

  public void ExplodeAllBugBullets() {
    var bugBullet = GameObject.FindGameObjectsWithTag("Bug Bullet");
    foreach(GameObject bb in bugBullet) {
      var bullet = bb.GetComponent<Bullet>();
      if(bullet) {
        bullet.Explode();
      }
    }
  }

  public void UseBomb() {
    if(bombs > 0 && currentState == GameState.Playing) {
      bombs -= 1;
      UpdateBombText();
      soundManager.PlaySound("Bomb");
      DamageAllAgroBugs(10);
      ExplodeAllBugBullets();
    }
  }

  public void ResetLevelText() {
    levelNumberHud.SetActive(false);
  }

  public void OpenShop() {
    ChangeState(GameState.Shopping);
    ButtonClick();
    CloseAllTutorials();
    shopHud.SetActive(true);
    RefreshShopPages();
  }

  private void RefreshShopPages() {
    for(var i = 0; i < shopPages.Length; i += 1) {
      shopPages[i].SetActive(false);
    }
    shopPages[shopPage].SetActive(true);
  }

  private void RefreshLevel() {
    for(var i = 0; i < levelGroups.Length; i += 1) {
      levelGroups[i].SetActive(false);
    }
    GameObject currentLevelGroup = levelGroups[level - 1];
    if(currentLevelGroup) {
      currentLevelGroup.SetActive(true);
    } else {
      levelGroups[levelGroups.Length - 1].SetActive(true);
    }
    
  }

  public void ShopNext() {
    ButtonClick();
    shopPage += 1;
    if(shopPage >= shopPages.Length) {
      shopPage = 0;
    }
    RefreshShopPages();
  }

  public void ShopPrev() {
    ButtonClick();
    shopPage -= 1;
    if(shopPage < 0) {
      shopPage = shopPages.Length - 1;
    }
    RefreshShopPages();
  }

  public void StartNextLevel() {
    level += 1;
    ChangeState(GameState.Playing);
    levelSpeed = 0.666f;

    // Level Text thing
    levelNumberHud.SetActive(true);
    var levelText = levelNumberHud.GetComponent<Text>();
    levelText.text = "LEVEL " + level.ToString();
    levelText.GetComponent<Animator>().Play("Level Fade", -1, 0f);

    // Rotate level
    RefreshLevel();

    // Position players
    alpha.transform.position = new Vector3(-1.34f, -3.9f, alpha.transform.position.z);
    player.transform.position = new Vector3(1.34f, -3.9f, player.transform.position.z);
    shipTargetPosition = new Vector3(1.34f, -3.9f, player.transform.position.z);

    // Position camera
    mainCamera.transform.position = new Vector3(
      mainCamera.transform.position.x,
      -0.06f,
      mainCamera.transform.position.z
    );
  }

  public void ShopDone() {
    ButtonClick();
    shopHud.SetActive(false);
    StartNextLevel();
  }

  public bool CanAfford(int cost) {
    if(money >= cost) {
      money -= cost;
      soundManager.PlaySound("Buy");
      SetMoneyText();
      return true;
    }
    return false;
  }

  public void ShopBuy(string bundle) {
    ButtonClick();
    //             0: Middle
    //             1: Under
    //     2: Left Forward
    //              3: Right Forward
    // 4: Left Diagonal
    //                  5: Right Diagonal
    // 6: Wing Left
    //                  7: Wing Right
    // 8: Spread Left
    //                  9: Spread Right
    Human pixieH = player.GetComponent<Human>();
    Human alphaH = alpha.GetComponent<Human>();
    string shopPageName = shopPages[shopPage].gameObject.name;

    Debug.Log("Buying " + shopPageName + ", " + bundle + ". Cost: " + 3000 + "/" + money);

    // Do shop shiz - this is awful - need a text file to sort this
    if(shopPageName == "Shield") {
      if(bundle == "Bundle 1") {
        if(CanAfford(800)) {
          pixieH.Heal(1);
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(2800)) {
          pixieH.Heal(10);
        }
      }
    } else if(shopPageName == "Chocolates") {
      if(bundle == "Bundle 1") {
        if(CanAfford(1000)) {
          alphaH.Heal(3);
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(5000)) {
          alphaH.Heal(10);
        }
      } else if(bundle == "Bundle 3") {
        if(CanAfford(9000)) {
          alphaH.Heal(40);
        }
      }
    } else if(shopPageName == "Bomb") {
      if(bundle == "Bundle 1") {
        if(CanAfford(1000)) {
          bombs += 1;
          UpdateBombText();
        }
      }
    } else if(shopPageName == "Flowers") {
      if(bundle == "Bundle 1") {
        if(CanAfford(23000)) {
          pixieH.maxHealth = 10;
          alphaH.maxHealth = 40;
        }
      }
    } else if(shopPageName == "Comet Blaster") {
      if(bundle == "Bundle 1") {
        if(CanAfford(3000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.CometBlaster;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(5000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[2] = Guns.CometBlaster;
          pixieH.gunSlots[3] = Guns.CometBlaster;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 3") {
        if(CanAfford(7500)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.CometBlaster;
          pixieH.gunSlots[2] = Guns.CometBlaster;
          pixieH.gunSlots[3] = Guns.CometBlaster;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 4") {
        if(CanAfford(10000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.CometBlaster;
          pixieH.gunSlots[2] = Guns.CometBlaster;
          pixieH.gunSlots[3] = Guns.CometBlaster;
          pixieH.gunSlots[4] = Guns.CometBlaster;
          pixieH.gunSlots[5] = Guns.CometBlaster;
          pixieH.AllocateGuns();
        }
      }
    } else if(shopPageName == "Fry Laser") {
      if(bundle == "Bundle 1") {
        if(CanAfford(4000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.FryLaser;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(6500)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[2] = Guns.FryLaser;
          pixieH.gunSlots[3] = Guns.FryLaser;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 3") {
        if(CanAfford(9000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.FryLaser;
          pixieH.gunSlots[2] = Guns.FryLaser;
          pixieH.gunSlots[3] = Guns.FryLaser;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 4") {
        if(CanAfford(15000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.FryLaser;
          pixieH.gunSlots[2] = Guns.FryLaser;
          pixieH.gunSlots[3] = Guns.FryLaser;
          pixieH.gunSlots[4] = Guns.FryLaser;
          pixieH.gunSlots[5] = Guns.FryLaser;
          pixieH.AllocateGuns();
        }
      }
    } else if(shopPageName == "Missile Rack") {
      if(bundle == "Bundle 1") {
        if(CanAfford(8000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.MissileRack;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(12000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[2] = Guns.MissileRack;
          pixieH.gunSlots[3] = Guns.MissileRack;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 3") {
        if(CanAfford(18000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.MissileRack;
          pixieH.gunSlots[2] = Guns.MissileRack;
          pixieH.gunSlots[3] = Guns.MissileRack;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 4") {
        if(CanAfford(30000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.MissileRack;
          pixieH.gunSlots[2] = Guns.MissileRack;
          pixieH.gunSlots[3] = Guns.MissileRack;
          pixieH.gunSlots[4] = Guns.MissileRack;
          pixieH.gunSlots[5] = Guns.MissileRack;
          pixieH.AllocateGuns();
        }
      }
    } else if(shopPageName == "Star Destroyer") {
      if(bundle == "Bundle 1") {
        if(CanAfford(19000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.StarDestroyer;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(30000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[2] = Guns.StarDestroyer;
          pixieH.gunSlots[3] = Guns.StarDestroyer;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 3") {
        if(CanAfford(50000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.StarDestroyer;
          pixieH.gunSlots[2] = Guns.StarDestroyer;
          pixieH.gunSlots[3] = Guns.StarDestroyer;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 4") {
        if(CanAfford(90000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.StarDestroyer;
          pixieH.gunSlots[2] = Guns.StarDestroyer;
          pixieH.gunSlots[3] = Guns.StarDestroyer;
          pixieH.gunSlots[4] = Guns.StarDestroyer;
          pixieH.gunSlots[5] = Guns.StarDestroyer;
          pixieH.AllocateGuns();
        }
      }
    } else if(shopPageName == "Pulse Cannon") {
      if(bundle == "Bundle 1") {
        if(CanAfford(7000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.PulseCannon;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 2") {
        if(CanAfford(10000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[8] = Guns.PulseCannon;
          pixieH.gunSlots[9] = Guns.PulseCannon;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 3") {
        if(CanAfford(14000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.PulseCannon;
          pixieH.gunSlots[8] = Guns.PulseCannon;
          pixieH.gunSlots[9] = Guns.PulseCannon;
          pixieH.AllocateGuns();
        }
      } else if(bundle == "Bundle 4") {
        if(CanAfford(25000)) {
          pixieH.ResetAllGuns();
          pixieH.gunSlots[0] = Guns.PulseCannon;
          pixieH.gunSlots[8] = Guns.PulseCannon;
          pixieH.gunSlots[9] = Guns.PulseCannon;
          pixieH.gunSlots[4] = Guns.PulseCannon;
          pixieH.gunSlots[5] = Guns.PulseCannon;
          pixieH.AllocateGuns();
        }
      }
    } else if(shopPageName == "Blueberry") {
      if(bundle == "Bundle 1") {
        if(CanAfford(11000)) {
          pixieH.gunSlots[1] = Guns.Blueberry;
          pixieH.AllocateGuns();
        }
      }
    } else if(shopPageName == "Micro Missiles") {
      if(bundle == "Bundle 1") {
        if(CanAfford(45000)) {
          pixieH.gunSlots[6] = Guns.MicroMissiles;
          pixieH.gunSlots[7] = Guns.MicroMissiles;
          pixieH.AllocateGuns();
        }
      }
    }
  }
}
