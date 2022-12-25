using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace FieldOfViewAsset {

    /// <summary>
    /// Field of View
    /// v.1.1
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [HelpURL("https://docs.google.com/document/d/1CD9tbXvZ1LILZfF1meBRCV6cMfNMrGhHrpL47Y6JyLg")]
    public class FieldOfView : MonoBehaviour {

        #region LifecycleCallbacks

        /// <summary>
        /// invoked when the target has just appear in the field fo view (it is not yet recognized but already spotted)
        /// </summary>
        public delegate void OnTargetSpotted(GameObject target);
        public event OnTargetSpotted TargetSpotted;

        /// <summary>
        /// invoked when the target stays wihtin the field of view long enough (check 'Detect Target Time' configuration) 
        /// and is already recognized
        /// </summary>
        public delegate void OnTargetDetected(GameObject target);
        public event OnTargetDetected TargetDetected;

        /// <summary>
        /// invoked when the target has just gone from the field of view
        /// </summary>
        public delegate void OnTargetLost(GameObject target);
        public event OnTargetLost TargetLost;
        
        #endregion
        
        #region ConfigurationConstants
        
        public const int MinAngle = 1;
        public const int MaxAngle = 360;

        public const float MinResolution = 0.5f;
        public const float MaxResolution = 2.0f;
        
        public const int MinMeshAccuracy = 2;
        public const int MaxMeshAccuracy = 10;

        public enum VisibilityRules {
            None,
            AlwaysVisible,
            IfTargetDetected,
            IfCoolDown
        }
        
        #endregion
        
        #region ExternalAPI

        /// <summary>
        /// simulate 'look at target' behavior, turns the center of the field of view to the target
        /// </summary>
        public void LookAt(Vector3 target, float duration) {
            if (this._customLookAtCoroutine != null) {
                StopCoroutine(this._customLookAtCoroutine);
            }
            
            transform.LookAt(target);
            this._customLookAtCoroutine = StartCoroutine(DisableLookAtScheduled(duration));
        }

        /// <summary>
        /// pause field of view rotation
        /// </summary>
        public void PauseRotation() {
            this._isRotationPaused = true;
        }

        /// <summary>
        /// resume field of view rotation
        /// </summary>
        public void ResumeRotation() {
            this._isRotationPaused = false;
        }

        /// <summary>
        /// resulting collection contains both types of targets: spotted and detected
        /// </summary>
        public List<GameObject> GetAllVisibleTargets() {
            return this._spottedTargets.Concat(this._detectedTargets).ToList();
        }

        public float ViewRadius {
            get { return this._viewRadius; }
            set { this._viewRadius = value; }
        }

        public int ViewAngle {
            get { return this._viewAngle; }
            set { this._viewAngle = Mathf.Clamp(value, MinAngle, MaxAngle); }
        }
        
        public float ViewResolution {
            get { return this._viewResolution; }
            set { this._viewResolution = Mathf.Clamp(value, MinResolution, MaxResolution); }
        }

        /// <summary>
        /// exceptional case: enable 'none' => disable all
        /// </summary>
        public void EnableVisibilityRule(VisibilityRules rule) {
            SetVisibilityRule(rule, true);
        }
        
        /// <summary>
        /// exceptional case: disable 'none' => enable all
        /// </summary>
        public void DisableVisibilityRule(VisibilityRules rule) {
            SetVisibilityRule(rule, false);
        }
        
        private void SetVisibilityRule(VisibilityRules rule, bool flag) {
            switch (rule) {
                case VisibilityRules.None:
                    this._alwaysVisible = !flag;
                    this._visibleIfTargetDetected = !flag;
                    this._visibleDuringCoolDown = !flag;
                    break;

                case VisibilityRules.AlwaysVisible:
                    this._alwaysVisible = flag;
                    break;

                case VisibilityRules.IfTargetDetected:
                    this._visibleIfTargetDetected = flag;
                    break;
                    
                case VisibilityRules.IfCoolDown:
                    this._visibleDuringCoolDown = flag;
                    break;
            }
        }

        public bool IsVisibilityRuleEnabled(VisibilityRules rule) {
            switch (rule) {
                case VisibilityRules.None:
                    return !this._alwaysVisible && !this._visibleIfTargetDetected && !this._visibleDuringCoolDown;

                case VisibilityRules.AlwaysVisible:
                    return this._alwaysVisible;

                case VisibilityRules.IfTargetDetected:
                    return this._visibleIfTargetDetected;
                    
                case VisibilityRules.IfCoolDown:
                    return this._visibleDuringCoolDown;
            }

            Debug.LogError("Requested rule " + rule + " is unavailable");
            return false;
        }
        
        public LayerMask TargetsLayer {
            get { return this._targetsLayer; }
            set { this._targetsLayer = value; }
        }

        public AnimationCurve DefaultRotation {
            get { return this._defaultRotationPattern; }
            set { this._defaultRotationPattern = value; }
        }

        public AnimationCurve CoolDownRotation {
            get { return this._coolDownRotationPattern; }
            set { this._coolDownRotationPattern = value; }
        }

        public float DetectionTime {
            get { return this._detectionTime; }
            set { this._detectionTime = value; }
        }
        
        public float CoolDownTime {
            get { return this._coolDownTime; }
            set { this._coolDownTime = value; }
        }

        public int CurveAccuracy {
            get { return this._curveAccuracy; }
            set { this._curveAccuracy = Mathf.Clamp(value, MinMeshAccuracy, MaxMeshAccuracy); }
        }
        
        #endregion

        #region ConfigurationItems
        
        [SerializeField]
        private float _viewRadius;

        [SerializeField]
        [Range(MinAngle, MaxAngle)]
        private int _viewAngle;

        [Tooltip("Sectors per degree")]
        [SerializeField]
        [Range(MinResolution, MaxResolution)]
        private float _viewResolution;

        [SerializeField]
        private bool _alwaysVisible;

        [SerializeField]
        private bool _visibleIfTargetDetected;

        [SerializeField]
        private bool _visibleDuringCoolDown;

        [Tooltip("Ratation pattern used whenever the target has not yet been detected")]
        [SerializeField]
        private AnimationCurve _defaultRotationPattern;

        [Tooltip("Ratation pattern used whenever the target has just moved away from the field of view but not yet forgotten")]
        [SerializeField]
        private AnimationCurve _coolDownRotationPattern;
        
        /// <summary>
        /// layer that will be scanned for targets
        /// </summary>
        [SerializeField]
        private LayerMask _targetsLayer;

        /// <summary>
        /// time to fill the field of view with 'active' skin
        /// can be used to give some time to target to hide wihtout being detected
        /// </summary>
        [SerializeField]
        private float _detectionTime;

        /// <summary>
        /// time to fill the field of view with 'passive' skin
        /// can be used to simulate the period of time until the target is completely forgotten
        /// </summary>
        [SerializeField]
        private float _coolDownTime;

        /// <summary>
        /// configuration of collisions with other layers
        /// used to define transparency per layer
        /// transparency level decreases the length of the field of view area
        /// </summary>
        [SerializeField]
        private LayersConfigurationEntry[] _layersConfiguration;
        
        /// <summary>
        /// loops number of mesh smoothing procedure
        /// </summary>
        [SerializeField]
        [Range(MinMeshAccuracy, MaxMeshAccuracy)]
        private int _curveAccuracy;
        
        /// <summary>
        /// debug information of mesh
        /// </summary>
        [SerializeField]
        private bool _showSectorEdges;
        
        #endregion

        #region InternalServiceVariables

        private Mesh _mesh;

        private MeshRenderer _meshRenderer;

        /// <summary>
        /// holds current state of skin
        /// </summary>
        private SkinMode _currentSkinMode;

        /// <summary>
        /// holds the border position between passive and active skins
        /// </summary>
        private float _skinsOffset;

        /// <summary>
        /// collection of targets that are inside the field of view but not yet 'detected' due to detection interval
        /// </summary>
        private List<GameObject> _spottedTargets = new List<GameObject>();

        /// <summary>
        /// collection of targets that are inside the field of view for enough long time and are already 'detected'
        /// </summary>
        private List<GameObject> _detectedTargets = new List<GameObject>();

        /// <summary>
        /// holds 'Look At' coroutine reference for scheduled 'delay end' trigger
        /// </summary>
        private Coroutine _customLookAtCoroutine;

        /// <summary>
        /// pauses default rotation
        /// </summary>
        private bool _isRotationPaused;
        
        #endregion
        
        private void Reset() {
            SetupDefaultSettings();
            SetupDefaultSkinMaterial();
        }

        private void SetupDefaultSettings() {
            this._viewRadius = 3.0f;
            this._viewAngle = 45;
            this._viewResolution = 0.75f;

            this._alwaysVisible = true;
            
            this._detectionTime = 2;
            this._coolDownTime = 5;
            
            this._curveAccuracy = 5;

            this._layersConfiguration = new[] {new LayersConfigurationEntry(-1, 0)}; // 'all layers with 0-transparency'
            this._currentSkinMode = SkinMode.NONE;
            this._showSectorEdges = true;
        }
        
        private void SetupDefaultSkinMaterial() {
            // disabled redundant default configuration
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            
            Material skin = new Material(Shader.Find("Hidden/Field Of View/Skin Swap"));
            skin.name = "FieldOfViewSkin";
            meshRenderer.sharedMaterial = skin;
        }

        private void Start() {
            this._meshRenderer = GetComponent<MeshRenderer>();
            this._mesh = new Mesh { name = "Field Of View Mesh" };
            this._mesh.MarkDynamic();
            GetComponent<MeshFilter>().mesh = this._mesh;

            SetSkinMode(SkinMode.PASSIVE_MODE);
        }
        
        /// <summary>
        /// switches field of view skin to different modes
        /// controlls shader processing
        /// </summary>
        private void SetSkinMode(SkinMode skinMode) {
            if (this._currentSkinMode == skinMode) {
                return;
            }

            // disable all enabled skins
            Enum.GetValues(typeof(SkinMode)).Cast<SkinMode>().ToList()
                    .ForEach(skin => this._meshRenderer.material.DisableKeyword(skin.ToString()));

            this._currentSkinMode = skinMode;

            // pass variables to shader
            this._meshRenderer.material.EnableKeyword(skinMode.ToString());
            this._meshRenderer.material.SetFloat("_ExecutionStartTime", Time.time);
            this._meshRenderer.material.SetFloat("_FadeInSpeed", this._viewRadius / this._detectionTime);
            this._meshRenderer.material.SetFloat("_FadeOutSpeed", this._viewRadius / this._coolDownTime);
            this._meshRenderer.material.SetFloat("_SkinsOffset", this._skinsOffset);
        }

        private void Update() {
            this._meshRenderer.material.SetFloat("_GameTime", Time.time);
        }

        private void LateUpdate() {
            List<RayCastResult> fieldOfViewData = CastFieldOfView();

            if (IsViewVisible()) {
                BuildFieldOfViewMesh(fieldOfViewData);
            }
            else {
                if (this._mesh.vertexCount > 0) {
                    this._mesh.Clear();
                }
            }
            
            DetectTargets(fieldOfViewData);
        }

        /// <summary>
        /// used for conditional visibility logic
        /// </summary>
        private bool IsViewVisible() {
            bool visibleTargetsExist = this._detectedTargets.Concat(this._spottedTargets).ToList().Count > 0;
            bool visibleWhenTargetDetected = visibleTargetsExist && this._visibleIfTargetDetected;
            bool visibleDuringCoolDown = this._currentSkinMode == SkinMode.FADE_OUT_MODE && this._visibleDuringCoolDown;

            return this._alwaysVisible || visibleWhenTargetDetected || visibleDuringCoolDown;
        }

        /// <summary>
        /// generate mesh vertices
        /// </summary>
        private List<RayCastResult> CastFieldOfView() {
            float sectorsCount = Mathf.Max(1, this._viewAngle * this._viewResolution);
            float sectorAngle = this._viewAngle / sectorsCount;

            List<RayCastResult> fieldOfViewMeshPointsList = new List<RayCastResult>();

            RayCastResult previousRayResult = null;

            for (int i = 0; i <= sectorsCount; i++) {
                float currentRayAngle = GetDeviationAngle() - this._viewAngle / 2.0f + i * sectorAngle;
                RayCastResult currentRayCastInfo = CastViewRay(currentRayAngle);

                if (previousRayResult != null) {
                    fieldOfViewMeshPointsList.AddRange(GenerateSubsegments(previousRayResult, currentRayCastInfo));
                }
                
                fieldOfViewMeshPointsList.Add(currentRayCastInfo);
                previousRayResult = currentRayCastInfo;

                if (this._showSectorEdges) {
                    // segment border
                    Debug.DrawLine(transform.position, currentRayCastInfo.EndPoint, Color.blue);
                }
            }

            return fieldOfViewMeshPointsList;
        }
        
        private float GetDeviationAngle() {
            if (this._spottedTargets.Count == 0 && this._detectedTargets.Count == 0) {
                if (this._isRotationPaused) {
                    return 0;
                }
                
                return this._currentSkinMode == SkinMode.FADE_OUT_MODE
                    ? this._coolDownRotationPattern.Evaluate(Time.time)
                    : this._defaultRotationPattern.Evaluate(Time.time);
            }
            else {
                return GetTargetAngle();
            }
        }
        
        /// <summary>
        /// find an angle to the first target
        /// </summary>
        private float GetTargetAngle() {
            List<GameObject> targets = this._spottedTargets.Concat(this._detectedTargets).ToList();
            Vector3 relativeTarget = transform.InverseTransformPoint(targets.First().transform.position);
            return Mathf.Atan2(relativeTarget.x, relativeTarget.z) * Mathf.Rad2Deg;
        }

        private void BuildFieldOfViewMesh(List<RayCastResult> fieldOfViewRays) {

            int vertexCount = fieldOfViewRays.Count + 1; // +1 for root one

            Vector3[] vertices = new Vector3[vertexCount];
            vertices[0] = Vector3.zero;
            for (int i = 1; i < vertexCount; i++) {
                vertices[i] = transform.InverseTransformPoint(fieldOfViewRays[i - 1].EndPoint);
            }

            int[] triangles = new int[(vertexCount - 2) * 3];
            for (int i = 0; i < vertexCount - 2; i++) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            // middle UV coordinate of texture
            const float uvMiddle = 0.5f;
                
            // used for normalizing, but must be a doube value to clamp the result value in a range -0.5..0.5
            // this range can be easily moved to 0..1 which corresponds to regular texture UVs
            float doubeRadius = this._viewRadius * 2;
            
            Vector2[] uvs = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++) {
                Vector3 scaledVertex = Vector3.Scale(vertices[i], transform.lossyScale);
                float x = scaledVertex.x / doubeRadius + uvMiddle;
                float z = scaledVertex.z / doubeRadius + uvMiddle;
                uvs[i] = new Vector2(x, z);
            }

            this._mesh.Clear();

            this._mesh.vertices = vertices;
            this._mesh.triangles = triangles;
            this._mesh.uv = uvs;

            this._mesh.RecalculateBounds();
            this._mesh.RecalculateNormals();
        }

        private IEnumerator DisableLookAtScheduled(float duration) {
            yield return new WaitForSeconds(duration);
            DisableLookAt();
        }
        
        private void DisableLookAt() {
            transform.localRotation = Quaternion.identity;
        }
        
        private void DetectTargets(List<RayCastResult> fieldOfViewData) {
            List<KeyValuePair<GameObject, float>> discoveredTargets = 
                    fieldOfViewData.SelectMany(ray => ray.DiscoveredTargets).ToList();

            // filter out all except shortest rays
            discoveredTargets = discoveredTargets
                    .GroupBy(pair => pair.Key)
                    .Select(group => group.OrderBy(pair => pair.Value).First())
                    .ToList();

            foreach (KeyValuePair<GameObject, float> discoveredTarget in discoveredTargets) {

                // target has been already spotted
                if (this._spottedTargets.Contains(discoveredTarget.Key)) {
                    if (this._skinsOffset >= discoveredTarget.Value) {
                        this._spottedTargets.Remove(discoveredTarget.Key);
                        this._detectedTargets.Add(discoveredTarget.Key);

                        if (TargetDetected != null) {
                            TargetDetected(discoveredTarget.Key);
                        }
                    }
                }

                // new target
                else if (!this._detectedTargets.Contains(discoveredTarget.Key)) {
                    // current offset is taken into account
                    this._spottedTargets.Add(discoveredTarget.Key);

                    if (TargetSpotted != null) {
                        TargetSpotted(discoveredTarget.Key);
                    }
                }
            }

            IEnumerable<GameObject> previouslyDiscoveredTargets = this._spottedTargets.Concat(this._detectedTargets);
            
            List<GameObject> lostTargets = 
                    previouslyDiscoveredTargets.Except(discoveredTargets.Select(pair => pair.Key)).ToList();
            
            for (int i = 0; i < lostTargets.Count; i++) {
                if (this._spottedTargets.Contains(lostTargets[i])) {
                    this._spottedTargets.Remove(lostTargets[i]);
                }
                else if (this._detectedTargets.Contains(lostTargets[i])) {
                    this._detectedTargets.Remove(lostTargets[i]);
                }

                if (TargetLost != null) {
                    TargetLost(lostTargets[i]);
                }
            }

            // get attention back
            if (this._spottedTargets.Concat(this._detectedTargets).ToList().Count > 0) {
                DisableLookAt();
            }

            UpdateMeshSkin(fieldOfViewData.Max(ray => ray.Length));
        }

        /// <summary>
        /// smooth swap between passive / active skins
        /// </summary>
        private void UpdateMeshSkin(float maxFieldOfViewRadius) {
            if (this._detectedTargets.Count != 0) {
                this._skinsOffset = maxFieldOfViewRadius;
                SetSkinMode(SkinMode.ACTIVE_MODE);
            }
            else if (this._spottedTargets.Count == 0 && Mathf.Approximately(this._skinsOffset, 0)) {
                this._skinsOffset = 0;
                SetSkinMode(SkinMode.PASSIVE_MODE);
            }
            // swap to passive skin
            else if (this._spottedTargets.Count == 0) {
                this._skinsOffset -= Time.deltaTime * this._viewRadius / this._coolDownTime;

                if (this._skinsOffset < 0) {
                    this._skinsOffset = 0;
                }

                SetSkinMode(SkinMode.FADE_OUT_MODE);
            }
            // swap to active skin
            else if (this._spottedTargets.Count != 0) {
                this._skinsOffset += Time.deltaTime * this._viewRadius / this._detectionTime;

                if (this._skinsOffset > maxFieldOfViewRadius) {
                    this._skinsOffset = maxFieldOfViewRadius;
                }

                SetSkinMode(SkinMode.FADE_IN_MODE);
            }
        }

        /// <summary>
        /// recursively divide field of view sector into subsectors in case if edge rays have different count 
        /// of hit obstacles (it means that one edge of the sector hits an obstacle whereas another one - not)
        /// </summary>
        private List<RayCastResult> GenerateSubsegments(RayCastResult startRay, RayCastResult endRay) {
            float sectorAngle = Mathf.Abs(endRay.Angle - startRay.Angle);

            if (startRay.ObstacleHits != endRay.ObstacleHits && sectorAngle > 0.1f) {

                SegmentEdgeRays intermediateRays = FindSubsegmentEdges(startRay, endRay);

                List<RayCastResult> intermediatePoints = new List<RayCastResult>();

                if (intermediateRays.StartRay != null) {
                    intermediatePoints.Add(intermediateRays.StartRay);
                    intermediatePoints.AddRange(GenerateSubsegments(startRay, intermediateRays.StartRay));

                    if (this._showSectorEdges) {
                        // sub-segment border (start)
                        Debug.DrawLine(transform.position, intermediateRays.StartRay.EndPoint, Color.yellow);
                    }
                }

                if (intermediateRays.EndRay != null) {
                    intermediatePoints.Add(intermediateRays.EndRay);
                    intermediatePoints.AddRange(GenerateSubsegments(intermediateRays.EndRay, endRay));

                    if (this._showSectorEdges) {
                        // sub-segment border (end)
                        Debug.DrawLine(transform.position, intermediateRays.EndRay.EndPoint, Color.green);
                    }
                }

                return intermediatePoints;
            }

            return Enumerable.Empty<RayCastResult>().ToList();
        }

        /// <summary>
        /// adapting particular segment shape with the obstacle being collided
        ///
        /// algorithm: 
        /// step-by-step narrow the segment by taking new middle of it until the edge of obstacle is detected
        /// </summary>
        private SegmentEdgeRays FindSubsegmentEdges(RayCastResult startRay, RayCastResult endRay) {
            float startAngle = startRay.Angle;
            float endAngle = endRay.Angle;

            RayCastResult intermediateStartRay = null;
            RayCastResult intermediateEndRay = null;

            for (int i = 0; i < this._curveAccuracy; i++) {
                float middleAngle = (startAngle + endAngle) / 2.0f;
                RayCastResult middleRay = CastViewRay(middleAngle);

                if (middleRay.ObstacleHits == startRay.ObstacleHits) {
                    startAngle = middleAngle;
                    intermediateStartRay = middleRay;
                }
                else {
                    endAngle = middleAngle;
                    intermediateEndRay = middleRay;
                }
            }

            return new SegmentEdgeRays(intermediateStartRay, intermediateEndRay);
        }

        /// <summary>
        /// 'smart' ray casting.
        /// ray calculates its lenght depends on obstacles (and transparency level of them) that are being hit
        /// </summary>
        private RayCastResult CastViewRay(float angle) {
            Vector3 castDirectionVector =
                    new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
            
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.TransformDirection(castDirectionVector), this._viewRadius);
            hits = hits.OrderBy(hit => hit.distance).ToArray();

            // due to obstacles some hits can become irrelevant as non-reached
            int realHitsCount = 0;
            
            if (hits.Length > 0) {
                int joinedLayersMask = this._layersConfiguration.Aggregate(0, (mask, entry) => mask | entry.Layer.value);

                float rayLenght = this._viewRadius;
                Dictionary<GameObject, float> detectedTargets = new Dictionary<GameObject, float>();

                foreach (RaycastHit hit in hits) {
                    // ray is shorter than next hit target
                    if (rayLenght < hit.distance) {
                        break;
                    }

                    // ignore parent object or other owner's child objects
                    if (hit.transform.gameObject == transform.parent.gameObject || hit.transform.parent == transform.parent) {
                        continue;
                    }

                    realHitsCount++;

                    // ray hits the target => register target detection
                    if (this._targetsLayer == (this._targetsLayer | (int) Mathf.Pow(2, hit.transform.gameObject.layer))) {
                        detectedTargets.Add(hit.transform.gameObject, hit.distance);
                    }

                    // ray hits the obstacle => recalculate ray length
                    if (joinedLayersMask == (joinedLayersMask | (int) Mathf.Pow(2, hit.transform.gameObject.layer))) {
                        float lenghtOfRayAfterHit = rayLenght - hit.distance;

                        int layerTransparency = 0;

                        // must be sorted
                        // it will allow 'everything' layer to be processed first and provide 'default' transparency
                        // and later partucular configuration can override this default value
                        IOrderedEnumerable<LayersConfigurationEntry> transparencyConfiguration = 
                                this._layersConfiguration.OrderBy(entry => entry.Layer.value);

                        foreach (LayersConfigurationEntry layerConfiguration in transparencyConfiguration) {
                            if (((int) Mathf.Pow(2, hit.transform.gameObject.layer) & layerConfiguration.Layer.value) > 0) {
                                layerTransparency = layerConfiguration.Transparency;
                            }
                        }

                        float lengthOfInvisiblePartOfRay = lenghtOfRayAfterHit * (100 - layerTransparency) / 100;

                        rayLenght = rayLenght - lengthOfInvisiblePartOfRay;
                    }
                }

                Vector3 destinationPoint = transform.position + transform.rotation * castDirectionVector * rayLenght;
                return new RayCastResult(realHitsCount, detectedTargets, destinationPoint, rayLenght, angle);
            }

            Vector3 rayEndPoint = transform.position + transform.rotation * castDirectionVector * this._viewRadius;
            return new RayCastResult(0, new Dictionary<GameObject, float>(), rayEndPoint, this._viewRadius, angle);
        }
        
        /// <summary>
        /// skin modes
        /// possible modes are:
        /// - none, initial state, equivalent to not initialized state
        /// - passive mode, skin is completely 'green', no spotted targets at this moment
        /// - active mode, skin is 'red', detected target exists
        /// - fade-in mode, skin continuously moves to 'red', spotted target exists, but not yet detected
        /// - fade-out mode, skin continuously moves to 'green', spotted/detected target has just gone
        /// 
        /// note: modes are defined in upper case to fit the naming used within the shader
        /// </summary>
        private enum SkinMode {
            NONE,
            PASSIVE_MODE,
            ACTIVE_MODE,
            FADE_IN_MODE,
            FADE_OUT_MODE
        }
        
        private class RayCastResult {

            public readonly int ObstacleHits;
            public readonly Dictionary<GameObject, float> DiscoveredTargets; // objects + ditance from fov center
            public readonly Vector3 EndPoint;
            public readonly float Length;
            public readonly float Angle;

            public RayCastResult(int hits, Dictionary<GameObject, float> hitTargets, Vector3 end, float length, float angle) {
                this.ObstacleHits = hits;
                this.DiscoveredTargets = hitTargets;
                this.EndPoint = end;
                this.Length = length;
                this.Angle = angle;
            }
        }

        private class SegmentEdgeRays {

            public readonly RayCastResult StartRay;
            public readonly RayCastResult EndRay;

            public SegmentEdgeRays(RayCastResult startRay, RayCastResult endRay) {
                this.StartRay = startRay;
                this.EndRay = endRay;
            }
        }

        [Serializable]
        private class LayersConfigurationEntry {

            public LayerMask Layer;

            [Range(0, 100)]
            public int Transparency;

            public LayersConfigurationEntry(LayerMask layer, int transparency) {
                this.Layer = layer;
                this.Transparency = transparency;
            }
        }
    }
}