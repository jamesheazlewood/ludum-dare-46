using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    TitleScreen,
    Dialogue,
    Playing,
    Shopping,
}

public class GameBehaviour : MonoBehaviour
{
    public GameState currentState = GameState.TitleScreen;
    

    public float stateTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
      stateTimer = 0;
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
    void UpdateGame() {
      Debug.Log("UpdateGame");
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
