using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptToLiterallyTurnOffLevelNumber : MonoBehaviour
{
  public GameBehaviour gameBehaviour = null;

  void Start() {
    gameBehaviour = GameObject.Find("GameBehaviour").GetComponent<GameBehaviour>();
  }

  void Finished() {
    gameBehaviour.ResetLevelText();
  }
}
