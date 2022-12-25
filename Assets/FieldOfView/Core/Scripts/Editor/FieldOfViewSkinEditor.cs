using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class FieldOfViewSkinEditor : ShaderGUI {

    private MaterialProperty _skinType;
    
    private MaterialProperty _passiveColor;
    private MaterialProperty _activeColor;
    
    private MaterialProperty _passiveTexture;
    private MaterialProperty _activeTexture;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        LoadProperties(properties);
        
        GUILayout.Label("Skin Settings", EditorStyles.boldLabel);

        materialEditor.ShaderProperty(this._skinType, new GUIContent("Skin Type"));

        EditorGUILayout.Space();

        if (this._skinType.floatValue == 0) {
            materialEditor.ColorProperty(this._passiveColor, "Passive Color");
            EditorGUILayout.Space();
            materialEditor.ColorProperty(this._activeColor, "Active Color");
        }
        else {
            materialEditor.TextureProperty(this._passiveTexture, "Passive Texture");
            EditorGUILayout.Space();
            materialEditor.TextureProperty(this._activeTexture, "Active Texture");
        }
        
        EditorGUILayout.Space();
    }

    private void LoadProperties(MaterialProperty[] properties) {
        this._skinType = FindProperty("_SkinType", properties);
        Assert.IsNotNull(this._skinType);
        
        this._passiveColor = FindProperty("_PassiveColor", properties);
        Assert.IsNotNull(this._passiveColor);
        
        this._activeColor = FindProperty("_ActiveColor", properties);
        Assert.IsNotNull(this._activeColor);
        
        this._passiveTexture = FindProperty("_PassiveTexture", properties);
        Assert.IsNotNull(this._passiveTexture);
        
        this._activeTexture = FindProperty("_ActiveTexture", properties);
        Assert.IsNotNull(this._activeTexture);
    }
}
