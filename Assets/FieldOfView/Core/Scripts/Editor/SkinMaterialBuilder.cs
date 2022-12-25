using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FieldOfViewAsset.Supplements {
    
    public class SkinMaterialBuilder : EditorWindow {

        private const string DefaultSkinName = "FieldOfViewSkin";

        private FieldOfView[] _selectedObjects;
        private string[] _availableFolders;
        private int _selectedFolder;
        private string _materialName;

        public static void Build() {
            GetWindow<SkinMaterialBuilder>();
        }
        
        private void Reset() {
            InitWindow();
            InitData();
        }
        
        private void InitWindow() {
            int width = 400;
            int height = 220;
            
            titleContent = new GUIContent("Skin Material Builder");
            position = new Rect(Screen.width / 2, Screen.height / 2, width, height);
            minSize = new Vector2(width, height);
            maxSize = new Vector2(width, height);
            ShowAuxWindow();
        }

        private void InitData() {
            this._availableFolders = Directory
                    .GetDirectories(Application.dataPath, "*.*", SearchOption.AllDirectories)
                    .Select(directory => directory.Replace(Application.dataPath + Path.DirectorySeparatorChar, ""))
                    .ToArray();

            this._materialName = DefaultSkinName;

            this._selectedObjects = Selection.GetFiltered<FieldOfView>(SelectionMode.Editable);
        }

        private void OnGUI() {
            GUI.skin.label.wordWrap = true; 
            
            GUILayout.Label("Field of View component dynamically generates material that is used to visualize its area.");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Do you want to save it as an asset?");
            
            GUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "If this game object is intended to be a Prefab then material must be saved. " +
                "Set the preferred name and location of the material and confirm it with [Save] button.",
                MessageType.Info);
            
            EditorGUILayout.HelpBox(
                "Otherwise the material is not required to be saved separately and will be serialized within the scene. " +
                "So this operatin can be safely cancelled.",
                MessageType.Info);
            
            GUILayout.Space(5);
            
            this._materialName = EditorGUILayout.TextField("Material Name", this._materialName);
            
            this._selectedFolder =
                    EditorGUILayout.Popup("Material Location", this._selectedFolder, this._availableFolders);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Save", GUILayout.Width(60))) {
                BuildMaterial();
                Close();
            }
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(60))) {
                Close();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void BuildMaterial() {
            Material skin = new Material(Shader.Find("Hidden/Field Of View/Skin Swap"));
            
            char slash = Path.DirectorySeparatorChar;
            string location = "Assets" + slash + this._availableFolders[this._selectedFolder] + slash;

            string materialName = this._materialName.Trim().Length == 0 ? DefaultSkinName : this._materialName;
            skin.name = materialName;

            string fullPath = location + materialName + ".mat";
        
            AssetDatabase.CreateAsset(skin, fullPath);
            AssetDatabase.SaveAssets();

            foreach (FieldOfView selectedObject in this._selectedObjects) {
                selectedObject.GetComponent<MeshRenderer>().sharedMaterial = 
                        AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}