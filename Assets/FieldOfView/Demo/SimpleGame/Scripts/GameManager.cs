using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject TextDisplay;

    private Text _text;

    private void OnEnable() {
        EnemyController.TargetCaught += OnTargetCaught;
        PlayerController.PlayerExit += OnPlayerEscape;
    }

    private void OnDisable() {
        EnemyController.TargetCaught -= OnTargetCaught;
        PlayerController.PlayerExit -= OnPlayerEscape;
    }

    private void Start() {
        this._text = this.TextDisplay.GetComponent<Text>();
    }

    private void OnTargetCaught(GameObject target) {
        Time.timeScale = 0;
        
        this._text.color = Color.red;
        this._text.text = "You lose. Escape to restart.";
    }

    private void OnPlayerEscape(GameObject player) {
        Time.timeScale = 0;
        
        this._text.color = Color.green;
        this._text.text = "You win. Escape to restart.";
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            SceneManager.LoadScene("Main");

            Time.timeScale = 1;
            this._text.text = string.Empty;
        }
    }
}