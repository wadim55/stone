using UnityEngine;
using UnityEngine.UI;

public class FpsMeter : MonoBehaviour {

    private Text textField;
    private float updateInterval;
    
    void Start() {
        this.textField = GetComponent<Text>();
        this.updateInterval = Time.time + 0.5f;
    }

    void Update() {
        if (Time.time >= this.updateInterval) {
            float fps = 1.0f / Time.deltaTime;
            this.textField.text = "FPS: " + fps.ToString("0.00");
            this.textField.color = Color.Lerp(Color.red, new Color(0.01f, 0.5f, 0), Mathf.InverseLerp(0, 50, fps));
            this.updateInterval = Time.time + 1;
        }
    }
}