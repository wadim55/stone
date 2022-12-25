using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject EnemyPrefab;
    public GameObject PlayerPrefab;
    
    private float _updateInterval;
    
    private List<GameObject> _players = new List<GameObject>();
    
    void Start() {
        this._updateInterval = Time.time + 1;

        // enemies
        for (int i = -5; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                GameObject enemy = Instantiate(this.EnemyPrefab, new Vector3(i * 4, 1, j * 4), Quaternion.identity);
                enemy.SetActive(true);
            }
        }
        
        // players
        for (int i = -5; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                GameObject player = Instantiate(this.PlayerPrefab, new Vector3(i * 4, 1, j * 4 + 3), Quaternion.identity);
                _players.Add(player);
            }
        }
    }

    void Update() {
        if (Time.time >= this._updateInterval) {
            this._players.ForEach(player => player.SetActive(Random.value > 0.5f));
            this._updateInterval = Time.time + 5;
        }
    }
}