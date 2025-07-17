using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
# if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
# endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace Seagull.Interior_I1.SceneProps {
    [System.Flags]
    public enum AxisOption {
        None = 0,
        X = 1 << 0, // 1
        Y = 1 << 1, // 2
        Z = 1 << 2, // 4
    }
    
    [ExecuteAlways]
    public class ArrayArrangement : MonoBehaviour {
# if UNITY_EDITOR
        [HideInInspector] public int arrayArrangementId = -1;
        
        [Header("Path ")]
        private Vector3 previousPos;
        public Vector3 pos1;
        private Vector3 previousPos1;
        public Vector3 pos2;
        private Vector3 previousPos2;
        public AnimationCurve pathCurve;
        private int previousPathCurveHash;
        
        [Header("Generation Rules")]
        public int count = 1;
        private List<GameObject> insts = new();
        
        public AxisOption randomizeRotationAxis;
        private AxisOption previousRandomizeRotationAxis;
        public float3x2 randomizeRotationRangeXYZ = new(0, 0, 0, 0, 0, 0);
        private float3x2 previousRandomizeRotationRangeXYZ;

        public AxisOption randomizePositionAxis;
        private AxisOption previousRandomizePositionAxis;
        public float3x2 randomizePositionRangeXYZ = new(0, 0, 0, 0, 0, 0);
        private float3x2 previousRandomizePositionRangeXYZ;
        
        [Header("Prefab and Root")]
        public Transform root;
        private Transform previousRoot;
        public GameObject prefab;
        private GameObject previousPrefab;

        private void Update() {
            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            # else
            return;
            # endif

            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            bool removedAnyGobj = false;
            insts.ToList().ForEach(gobj => {
                if (!gobj) {
                    insts.Remove(gobj);
                    count = insts.Count;
                    removedAnyGobj = true;
                }
            });
            if (previousPos != transform.position || removedAnyGobj) {
                Vector3 delta = pos2 - pos1;
                Vector3 unitVector = Vector3.zero;
                if (insts.Count > 1) unitVector = delta / (insts.Count - 1);
                Vector3 normal = delta.normalized;
                Quaternion rotation = Quaternion.Euler(0, -90, 0);
                normal = rotation * normal;
                for (int i = 0; i < insts.Count; i++) {
                    float x = i / (insts.Count - 1f);
                    float y = pathCurve.Evaluate(x);
                    Vector3 randomOffset = getRandomOffset();
                    insts[i].transform.position = transform.position + pos1 + i * unitVector + y * normal + randomOffset;
                }
            }
            previousPos = transform.position;
            # endif
        }

        private void FixedUpdate() {
            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            # else
            return;
            # endif
            
            bool removedAnyGobj = false;
            insts.ToList().ForEach(gobj => {
                if (!gobj) {
                    insts.Remove(gobj);
                    count = insts.Count;
                    removedAnyGobj = true;
                }
            });
            if (previousPos != transform.position || removedAnyGobj) {
                Vector3 delta = pos2 - pos1;
                Vector3 unitVector = Vector3.zero;
                if (insts.Count > 1) unitVector = delta / (insts.Count - 1);
                Vector3 normal = delta.normalized;
                Quaternion rotation = Quaternion.Euler(0, -90, 0);
                normal = rotation * normal;
                for (int i = 0; i < insts.Count; i++) {
                    float x = i / (insts.Count - 1f);
                    float y = pathCurve.Evaluate(x);
                    Vector3 randomOffset = getRandomOffset();
                    insts[i].transform.position = transform.position + pos1 + i * unitVector + y * normal + randomOffset;
                }
            }
            previousPos = transform.position;
        }

        private void OnValidate() {
            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            # else
            return;
            # endif
            
            if (arrayArrangementId == -1) {
                if (!PrefabUtility.IsPartOfPrefabAsset(gameObject) && !PrefabStageUtility.GetCurrentPrefabStage()) {
                    arrayArrangementId = EditorPrefs.GetInt("Fries.UUID", 0);
                    EditorPrefs.SetInt("Fries.UUID", arrayArrangementId + 1);
                }
            }
            
            refresh(true);
            previousPathCurveHash = computeCurveHash(pathCurve);
            previousRandomizeRotationAxis = randomizeRotationAxis;
            previousRandomizeRotationRangeXYZ = randomizeRotationRangeXYZ;
            previousPrefab = prefab;
            previousRoot = root;
            previousPos1 = pos1;
            previousPos2 = pos2;
        }

        private bool compareFloat3x2(float3x2 a, float3x2 b) {
            if (a.c0.x != b.c0.x) return false;
            if (a.c0.y != b.c0.y) return false;
            if (a.c0.z != b.c0.z) return false;
            if (a.c1.x != b.c1.x) return false;
            if (a.c1.y != b.c1.y) return false;
            if (a.c1.z != b.c1.z) return false;
            return true;
        }
        
        private void refresh(bool isInEditor = true) {
            if (count < 0) count = 0;
            
            if (!isInEditor) {
                Debug.LogError("Is in editor = false is not supported yet.");
                return;
            }
            
            if (count > 500) {
                Debug.LogWarning("Count is too big. Automatically set to 500");
                count = 500;
            }
            
            if (count == 0) {
                destroyAll();
                return;
            } if (!root) {
                root = transform;
            } if (!prefab) {
                return;
            }
            
            if (previousRoot != root) 
                insts.ForEach(gobj => gobj.transform.SetParent(root));
            if (previousRandomizeRotationAxis != randomizeRotationAxis || !compareFloat3x2(previousRandomizeRotationRangeXYZ, randomizeRotationRangeXYZ)) 
                insts.ForEach(rotate);
            if (previousPrefab == null) previousPrefab = prefab;
            if (previousPrefab != prefab) {
                destroyAll();
            }

            bool countChanged = false;
            if (count > insts.Count) {
                foreach (Transform child in root) {
                    if (insts.Contains(child.gameObject)) continue;
                    if (child.name.EndsWith($":{arrayArrangementId}")) insts.Add(child.gameObject);
                }
            }
            
            if (count > insts.Count) {
                countChanged = true;
                int diff = count - insts.Count;
                for (int i = 0; i < diff; i++) {
                    GameObject finalPrefab = prefab;
                    PrefabGroup prefabGroup = prefab.GetComponent<PrefabGroup>();
                    if (prefabGroup) finalPrefab = prefabGroup.getRandomPrefab();
                    
                    GameObject inst;
                    if (isInEditor) {
                        # if UNITY_EDITOR
                        EditorApplication.delayCall += () => {
                            inst = (GameObject)PrefabUtility.InstantiatePrefab(finalPrefab, root);
                            inst.name += $" :{arrayArrangementId}";
                            insts.Add(inst);
                            rotate(inst);
                        };
                        # endif
                    }
                }
                
                # if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    Vector3 delta = pos2 - pos1;
                    Vector3 unitVector = Vector3.zero;
                    if (insts.Count > 1) unitVector = delta / (insts.Count - 1);
                    Vector3 normal = delta.normalized;
                    Quaternion rotation = Quaternion.Euler(0, -90, 0); 
                    normal = rotation * normal;
                    for (int i = 0; i < insts.Count; i++) {
                        if (!insts[i]) continue;
                        float x = i / (insts.Count - 1f);
                        float y = pathCurve.Evaluate(x);
                        Vector3 randomOffset = getRandomOffset();
                        insts[i].transform.position = transform.position + pos1 + i * unitVector + y * normal + randomOffset;
                    }
                };
                # endif
            } else if (count < insts.Count) {
                countChanged = true;
                int diff = insts.Count - count;
                for (int i = 0; i < diff; i++) {
                    GameObject inst = insts[^1];
                    insts.Remove(inst);
                    if (isInEditor) {
                        # if UNITY_EDITOR
                        EditorApplication.delayCall += () => { DestroyImmediate(inst); };
                        # endif
                    }
                }
                
                # if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    Vector3 delta = pos2 - pos1;
                    Vector3 unitVector = Vector3.zero;
                    if (insts.Count > 1) unitVector = delta / (insts.Count - 1);
                    Vector3 normal = delta.normalized;
                    Quaternion rotation = Quaternion.Euler(0, -90, 0); 
                    normal = rotation * normal;
                    for (int i = 0; i < insts.Count; i++) {
                        if (!insts[i]) continue;
                        float x = i / (insts.Count - 1f);
                        float y = pathCurve.Evaluate(x);
                        Vector3 randomOffset = getRandomOffset();
                        insts[i].transform.position = transform.position + pos1 + i * unitVector + y * normal + randomOffset;
                    }
                };
                # endif
            }

            if (previousPos1 != pos1 || previousPos2 != pos2 || countChanged || previousPathCurveHash != computeCurveHash(pathCurve) || 
                randomizePositionAxis != previousRandomizePositionAxis || !compareFloat3x2(randomizePositionRangeXYZ, previousRandomizePositionRangeXYZ)) {
                Vector3 delta = pos2 - pos1;
                Vector3 unitVector = Vector3.zero;
                if (insts.Count > 1) unitVector = delta / (insts.Count - 1);
                Vector3 normal = delta.normalized;
                Quaternion rotation = Quaternion.Euler(0, -90, 0); 
                normal = rotation * normal;
                for (int i = 0; i < insts.Count; i++) {
                    float x = i / (insts.Count - 1f);
                    float y = pathCurve.Evaluate(x);
                    Vector3 randomOffset = getRandomOffset();
                    insts[i].transform.position = transform.position + pos1 + i * unitVector + y * normal + randomOffset;
                }
            }
        }

        private void destroyAll() {
            # if UNITY_EDITOR
            EditorApplication.delayCall += () => {
                insts.ForEach(DestroyImmediate);
                insts = new List<GameObject>();
            };
            # endif
        }

        private void rotate(GameObject inst) {
            float x = 0, y = 0, z = 0;
            if ((randomizeRotationAxis & AxisOption.X) != 0) 
                x = Random.Range(randomizeRotationRangeXYZ.c0.x, randomizeRotationRangeXYZ.c1.x);
            if ((randomizeRotationAxis & AxisOption.Y) != 0)
                y = Random.Range(randomizeRotationRangeXYZ.c0.y, randomizeRotationRangeXYZ.c1.y);
            if ((randomizeRotationAxis & AxisOption.Z) != 0)
                z = Random.Range(randomizeRotationRangeXYZ.c0.z, randomizeRotationRangeXYZ.c1.z);
            inst.transform.localEulerAngles = new Vector3(x, y, z);
        }

        private Vector3 getRandomOffset() {
            Vector3 offset = new Vector3(0, 0, 0);
            if ((randomizePositionAxis & AxisOption.X) != 0)
                offset.x = Random.Range(randomizePositionRangeXYZ.c0.x, randomizePositionRangeXYZ.c1.x);
            if ((randomizePositionAxis & AxisOption.Y) != 0)
                offset.y = Random.Range(randomizePositionRangeXYZ.c0.y, randomizePositionRangeXYZ.c1.y);
            if ((randomizePositionAxis & AxisOption.Z) != 0)
                offset.z = Random.Range(randomizePositionRangeXYZ.c0.z, randomizePositionRangeXYZ.c1.z);
            return offset;
        }
        
        private void OnDrawGizmos() {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
        
            Gizmos.DrawSphere(transform.position + pos1, 0.2f);
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(transform.position + pos2, 0.2f);

            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawLine(transform.position + pos1, transform.position + pos2);
        }

        private int computeCurveHash(AnimationCurve curve) {
            if (curve == null)
                return 0;
            
            unchecked {
                int hash = 17;
                hash = hash * 23 + curve.preWrapMode.GetHashCode();
                hash = hash * 23 + curve.postWrapMode.GetHashCode();

                foreach (var key in curve.keys) {
                    hash = hash * 23 + key.time.GetHashCode();
                    hash = hash * 23 + key.value.GetHashCode();
                    hash = hash * 23 + key.inTangent.GetHashCode();
                    hash = hash * 23 + key.outTangent.GetHashCode();
                    hash = hash * 23 + key.inWeight.GetHashCode();
                    hash = hash * 23 + key.outWeight.GetHashCode();
                    hash = hash * 23 + key.weightedMode.GetHashCode();
                }

                return hash;
            }
        }
        # endif
    }
}