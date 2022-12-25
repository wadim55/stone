using System.Collections.Generic;
using System.Linq;
using FieldOfViewAsset.Supplements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace FieldOfViewAsset {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(FieldOfView))]
    public class FieldOfViewConfigurationEditor : Editor {

        private SerializedProperty _viewRadius;
        private SerializedProperty _viewAngle;
        private SerializedProperty _viewResolution;
        
        private SerializedProperty _alwaysVisible;
        private SerializedProperty _visibleIfTargetDetected;
        private SerializedProperty _visibleDuringCoolDown;
        
        private SerializedProperty _defaultRotationPattern;
        private SerializedProperty _coolDownRotationPattern;
        
        private SerializedProperty _targetsLayer;
        private SerializedProperty _detectionTime;
        private SerializedProperty _coolDownTime;

        private SerializedProperty _layersConfiguration;
        
        private SerializedProperty _curveAccuracy;
        
        private SerializedProperty _showSectorEdges;
        
        private bool _showGeneralSettings = true;
        private bool _showMaterialSettings = true;
        private bool _showVisibilitySettings = true;
        private bool _showViewRotationSettings = true;
        private bool _showDetectionSettings = true;
        private bool _showAccuracySettings = true;
        private bool _showTransparencySettings = true;
        private bool _showDebugSettings = true;

        public void OnEnable() {
            this._viewRadius = serializedObject.FindProperty("_viewRadius");
            this._viewAngle = serializedObject.FindProperty("_viewAngle");
            this._viewResolution = serializedObject.FindProperty("_viewResolution");
            
            this._alwaysVisible = serializedObject.FindProperty("_alwaysVisible");
            this._visibleIfTargetDetected = serializedObject.FindProperty("_visibleIfTargetDetected");
            this._visibleDuringCoolDown = serializedObject.FindProperty("_visibleDuringCoolDown");
            
            this._defaultRotationPattern = serializedObject.FindProperty("_defaultRotationPattern");
            this._coolDownRotationPattern = serializedObject.FindProperty("_coolDownRotationPattern");

            this._targetsLayer = serializedObject.FindProperty("_targetsLayer");
            this._detectionTime = serializedObject.FindProperty("_detectionTime");
            this._coolDownTime = serializedObject.FindProperty("_coolDownTime");

            this._layersConfiguration = serializedObject.FindProperty("_layersConfiguration");
            
            this._curveAccuracy = serializedObject.FindProperty("_curveAccuracy");

            this._showSectorEdges = serializedObject.FindProperty("_showSectorEdges");

            CheckConfigurationLoad();
        }

        private void CheckConfigurationLoad() {
            Assert.IsNotNull(this._viewRadius);
            Assert.IsNotNull(this._viewAngle);
            Assert.IsNotNull(this._viewResolution);
            
            Assert.IsNotNull(this._alwaysVisible);
            Assert.IsNotNull(this._visibleIfTargetDetected);
            Assert.IsNotNull(this._visibleDuringCoolDown);
            
            Assert.IsNotNull(this._defaultRotationPattern);
            Assert.IsNotNull(this._coolDownRotationPattern);

            Assert.IsNotNull(this._targetsLayer);
            Assert.IsNotNull(this._detectionTime);
            Assert.IsNotNull(this._coolDownTime);

            Assert.IsNotNull(this._layersConfiguration);
            
            Assert.IsNotNull(this._curveAccuracy);

            Assert.IsNotNull(this._showSectorEdges);
        }
        
        public void OnSceneGUI() {
            DrawMainArea((FieldOfView) target);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            this._showGeneralSettings = EditorGUILayout.Foldout(this._showGeneralSettings, "General Settings");
            if (this._showGeneralSettings) {
                DrawGeneralSettings();
                DrawGroupSeparator();
            }
            
            this._showMaterialSettings = EditorGUILayout.Foldout(this._showMaterialSettings, "Material Settings");
            if (this._showMaterialSettings) {
                DrawMaterialSettings();
                DrawGroupSeparator();
            }
            
            this._showVisibilitySettings = EditorGUILayout.Foldout(this._showVisibilitySettings, "Visibility Settings");
            if (this._showVisibilitySettings) {
                DrawVisibilitySettings();
                DrawGroupSeparator();
            }
            
            this._showViewRotationSettings = EditorGUILayout.Foldout(this._showViewRotationSettings, "Rotation Settings");
            if (this._showViewRotationSettings) {
                DrawViewRotationSettings();
                DrawGroupSeparator();
            }
            
            this._showDetectionSettings = EditorGUILayout.Foldout(this._showDetectionSettings, "Detection Settings");
            if (this._showDetectionSettings) {
                DrawDetectionSettings();
                DrawGroupSeparator();
            }

            this._showTransparencySettings = EditorGUILayout.Foldout(this._showTransparencySettings, "Transparency Settings");
            if (this._showTransparencySettings) {
                DrawTransparencySettings();
                DrawGroupSeparator();
            }
            
            this._showAccuracySettings = EditorGUILayout.Foldout(this._showAccuracySettings, "Accuracy Settings");
            if (this._showAccuracySettings) {
                DrawAccuracySettings();
                DrawGroupSeparator();
            }

            this._showDebugSettings = EditorGUILayout.Foldout(this._showDebugSettings, "Debug Settings");
            if (this._showDebugSettings) {
                DrawDebugSettings();
                DrawGroupSeparator();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralSettings() {
            EditorGUILayout.PropertyField(this._viewRadius, new GUIContent("Radius"));

            if (this._viewRadius.floatValue < 0) {
                this._viewRadius.floatValue = 0;
            }
            
            EditorGUILayout.PropertyField(this._viewAngle, new GUIContent("Angle"));
            EditorGUILayout.PropertyField(this._viewResolution, new GUIContent("Resolution"));
        }
        
        private void DrawMaterialSettings() {
            if (GUILayout.Button("Persist Material", EditorStyles.miniButton)) {
                SkinMaterialBuilder.Build();
            }
        }

        private void DrawVisibilitySettings() {
            DrawCheckbox(this._alwaysVisible, "Always Visible");

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(this._alwaysVisible.boolValue);
            GUILayout.Label("Conditionally Visible");
            
            EditorGUI.indentLevel++;
            DrawCheckbox(this._visibleIfTargetDetected, "Show if Target is spotted/detected");
            DrawCheckbox(this._visibleDuringCoolDown, "Show during Cool Down");
            EditorGUI.indentLevel--;

            EditorGUI.EndDisabledGroup();

            if (!this._alwaysVisible.boolValue 
                    && !this._visibleIfTargetDetected.boolValue 
                    && !this._visibleDuringCoolDown.boolValue) {
                
                EditorGUILayout.HelpBox(Messages.VisibilityConfigurationWarninig, MessageType.Warning);
            }
        }

        private void DrawCheckbox(SerializedProperty property, string label) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.Width(12 * (EditorGUI.indentLevel + 1)));
            GUILayout.Label(label);
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawViewRotationSettings() {
            DrawRotationPattern(this._defaultRotationPattern, "Default Rotation");
            EditorGUILayout.Space();
            DrawRotationPattern(this._coolDownRotationPattern, "Cool-down Rotation");
        }
        
        private void DrawDetectionSettings() {
            EditorGUILayout.PropertyField(this._targetsLayer);

            if (this._targetsLayer.intValue == 0) {
                EditorGUILayout.HelpBox(Messages.TargetsLayerIsEmptyInfo, MessageType.Info);
            }

            EditorGUILayout.PropertyField(this._detectionTime);
            EditorGUILayout.PropertyField(this._coolDownTime, new GUIContent("Cool-down Time"));

        }

        private void DrawTransparencySettings() {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Layers", GUILayout.Width(95));
            
            GUILayout.Label("Transparency");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(18.0f))) {
                this._layersConfiguration.InsertArrayElementAtIndex(this._layersConfiguration.arraySize);
            }

            EditorGUILayout.EndHorizontal();

            if (this._layersConfiguration.arraySize == 0) {
                EditorGUILayout.HelpBox(Messages.ObstacleLayesrAreEmptyInfo, MessageType.Info);
            }

            List<int> configuredLayers = new List<int>();
            
            EditorGUIUtility.labelWidth = 1;

            for (int i = 0; i < this._layersConfiguration.arraySize; i++) {
                SerializedProperty entry = this._layersConfiguration.GetArrayElementAtIndex(i);

                SerializedProperty entryLayer = entry.FindPropertyRelative("Layer");
                configuredLayers.AddRange(SplitIntoSingleLayers(entryLayer.intValue));
                
                SerializedProperty entryTransparency = entry.FindPropertyRelative("Transparency");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(entryLayer, GUIContent.none, GUILayout.Width(100.0f));

                EditorGUILayout.PropertyField(entryTransparency, GUIContent.none);

                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(18.0f))) {
                    this._layersConfiguration.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            // reset to default
            EditorGUIUtility.labelWidth = 0;

            List<string> ambiguousLayers = configuredLayers
                    .GroupBy(layer => layer)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key >= 0 ? LayerMask.LayerToName(group.Key) : "Everything")
                    .ToList();

            if (ambiguousLayers.Count > 0) {
                EditorGUILayout.HelpBox(
                    Messages.ObstacleLayesrHasAmbiguousConfigurationError + string.Join(", ", ambiguousLayers.ToArray()),
                    MessageType.Error);
            }
        }
        
        private void DrawAccuracySettings() {
            EditorGUIUtility.labelWidth = 125;
            EditorGUILayout.PropertyField(this._curveAccuracy, new GUIContent("Mesh Accuracy Level"));
            EditorGUIUtility.labelWidth = 0;

            if (this._curveAccuracy.intValue > 5) {
                EditorGUILayout.HelpBox(Messages.CurveAccuracyLevelWarning, MessageType.Warning);
            }
        }

        private void DrawDebugSettings() {
            DrawCheckbox(this._showSectorEdges, "Show Sector Edges");
        }
        
        private void DrawRotationPattern(SerializedProperty rotationPattern, string label) {
            EditorGUILayout.PropertyField(rotationPattern, new GUIContent(label));
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Slow Wave", EditorStyles.miniButton)) {
                AnimationCurve slowWaveCurve = new AnimationCurve {
                    keys = new[] {
                        new Keyframe(0, 0),
                        new Keyframe(1, 45),
                        new Keyframe(2, 0),
                        new Keyframe(3, -45),
                        new Keyframe(4, 0)
                    },
                    
                    postWrapMode = WrapMode.Loop
                };

                rotationPattern.animationCurveValue = slowWaveCurve;
            }
            
            if (GUILayout.Button("Fast Wave", EditorStyles.miniButton)) {
                AnimationCurve fastWaveCurve = new AnimationCurve {
                    keys = new[] {
                        new Keyframe(0, 0),
                        new Keyframe(0.5f, 45),
                        new Keyframe(1, 60),
                        new Keyframe(1.5f, 0),
                        new Keyframe(2, -45),
                        new Keyframe(2.5f, -60),
                        new Keyframe(3, 0)
                    },
                    
                    postWrapMode = WrapMode.Loop
                };

                rotationPattern.animationCurveValue = fastWaveCurve;
            }
            
            if (GUILayout.Button("Linear", EditorStyles.miniButton)) {
                
                AnimationCurve lineCurve = new AnimationCurve {
                    keys = new[] {
                        new Keyframe(0, 0),
                        new Keyframe(1, 45),
                        new Keyframe(2, 0),
                        new Keyframe(3, -45),
                        new Keyframe(4, 0)
                    },
                    
                    postWrapMode = WrapMode.Loop
                };
                
                for (int i = 0; i < lineCurve.keys.Length; i++) {
                    AnimationUtility.SetKeyLeftTangentMode(lineCurve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(lineCurve, i, AnimationUtility.TangentMode.Linear);
                }
                
                rotationPattern.animationCurveValue = lineCurve;
            }
            
            if (GUILayout.Button("Empty", EditorStyles.miniButton)) {
                rotationPattern.animationCurveValue = new AnimationCurve();
            }
            
            EditorGUILayout.EndHorizontal();

        }
        
        private static List<int> SplitIntoSingleLayers(int layersMask) {
            List<int> result = new List<int>();

            // 'everything' layer
            if (layersMask == -1) {
                result.Add(-1);
                return result;
            }

            // 'nothing' layer
            if (layersMask == 0) {
                return new List<int>();
            }

            int currentLayer = 1;
            while (currentLayer <= layersMask) {
                if (layersMask == (layersMask | currentLayer)) {
                    result.Add((int) Mathf.Log(currentLayer, 2));
                }
                currentLayer = currentLayer << 1;
            }

            return result;
        }
        
        private void DrawMainArea(FieldOfView fieldOfView) {
            if (Application.isPlaying) {
                return;
            }
            
            Handles.color = new Color(0, 1, 0, 0.15f);

            Handles.DrawSolidArc(
                fieldOfView.transform.position,
                fieldOfView.transform.up,
                Quaternion.Euler(0, -fieldOfView.ViewAngle / 2.0f, 0) * fieldOfView.transform.forward,
                fieldOfView.ViewAngle,
                fieldOfView.ViewRadius);
        }
        
        private void DrawGroupSeparator() {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }
        
        private class Messages {
            public const string TargetsLayerIsEmptyInfo =
                    "It must be set, othervise detection by Field of View is disabled";

            public const string CurveAccuracyLevelWarning =
                    "Selected value is quite high. Acceptable quality of curve can be reached at 5";

            public const string ObstacleLayesrAreEmptyInfo =
                    "It must be set, othervise the Field of View area will not be affected by any obstacle";

            public const string ObstacleLayesrHasAmbiguousConfigurationError =
                    "Some of the layers have ambiguous configuration. It will lead to runtime errors. " +
                    "Please fix the configuration first. Incorrect layer(s): ";

            public const string VisibilityConfigurationWarninig = "All the visibility rulles are off. " +
                    "With this configuration the Field of View will be completely invisible";
        }
    }
}