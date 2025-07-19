using System;
using System.Collections.Generic;
using System.Linq;
# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
# endif
using UnityEngine;

namespace Seagull.Interior_I1.SceneProps {
    public enum SingleAxisOption {
        X, Y, Z
    }
    
    [ExecuteAlways]
    public class FencePlacement : MonoBehaviour {
        # if UNITY_EDITOR
        
        [HideInInspector] public int fencePlacementId = -1;
        
        [Header("Path ")]
        public Vector3 pos1;
        public Vector3 pos2;
        public AnimationCurve pathCurve;
        
        [Min(0.01f)]
        public float size = 1;
        private float previousSize;
        public SingleAxisOption frozenAxis;
        
        [Header("Prefab and Root")]
        public Transform root;
        private Transform previousRoot;
        public GameObject prefab;
        private GameObject previousPrefab;
        private List<GameObject> fences = new();

        [Header("End Cap")] 
        public GameObject endCapPrefab;
        private GameObject previousEndCapPrefab;
        private GameObject endCapInst;

        private Vector3 actualEnd;
        private Quaternion endRotation;

        private void Update() {
            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            # else
            return;
            # endif

            if (size < 0.01f) size = 0.01f;

            transform.localScale = new Vector3(1, 1, 1);
            transform.eulerAngles = new Vector3(0, 0, 0);
            
            # if UNITY_EDITOR
            bool removedAnyGobj = false;
            if (!EditorApplication.isPlaying) {
                fences.ToList().ForEach(gobj => {
                    if (gobj) return;
                    fences.Remove(gobj);
                    removedAnyGobj = true;
                });
                if (removedAnyGobj) generateFences(true);
            }
            # endif
        }

        private void FixedUpdate() {
            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            # else
            return;
            # endif
            
            bool removedAnyGobj = false;
            if (!EditorApplication.isPlaying) {
                fences.ToList().ForEach(gobj => {
                    if (gobj) return;
                    fences.Remove(gobj);
                    removedAnyGobj = true;
                });
                if (removedAnyGobj) generateFences(true);
            }
        }

        private void OnValidate() {
            # if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
            # else
            return;
            # endif
            
            if (fencePlacementId == -1) {
                if (!PrefabUtility.IsPartOfPrefabAsset(gameObject) && !PrefabStageUtility.GetCurrentPrefabStage()) {
                    fencePlacementId = EditorPrefs.GetInt("Fries.UUID", 0);
                    EditorPrefs.SetInt("Fries.UUID", fencePlacementId + 1);
                }
            }
            
            if (!root) {
                root = transform;
            } if (!prefab) {
                return;
            } if (!prefab.GetComponent<FenceData>()) {
                Debug.LogWarning("Prefab is not a Fence, please attach FenceData to the Prefab to make it a Fence. This script won't do anything before the fix");
                return;
            } if (endCapPrefab && !endCapPrefab.GetComponent<FenceData>()) {
                Debug.LogWarning(
                    "End Cap Prefab is not a Fence, please attach FenceData to the Prefab to make it a Fence. This script won't do anything before the fix");
                return;
            } if (size == 0) {
                return;
            }

            if (previousRoot != root) {
                fences.ForEach(gobj => gobj.transform.SetParent(root));
                if (endCapInst) endCapInst.transform.SetParent(root);
            }
            if (previousPrefab != prefab) {
                fences.ForEach(gobj => {
                    # if UNITY_EDITOR
                    EditorApplication.delayCall += () => { DestroyImmediate(gobj); };
                    # endif
                });
                fences = new List<GameObject>();
            }
            if (previousEndCapPrefab != endCapPrefab) {
                # if UNITY_EDITOR
                EditorApplication.delayCall += () => { 
                    DestroyImmediate(endCapInst); 
                    endCapInst = null;
                };
                # endif
            }
            
            generateFences(true);

            previousEndCapPrefab = endCapPrefab;
            previousPrefab = prefab;
            previousRoot = root;
            previousSize = size;
        }

        private readonly float tolerance = 0.0001f;
        private (float, Vector3) approachingForEnd(float startX, Vector3 start, float length) {
            int i = 0;
            float step = 0.5f;
            float x = startX;
            Vector3 pathStart = transform.position + pos1;
            float diff = -1f;
            Vector3 end = default;
            
            Vector3 delta = pos2 - pos1;
            Vector3 normal = delta.normalized;
            Quaternion rotation = Quaternion.Euler(0, -90, 0);
            Vector3 rotatedNormal = rotation * normal;
            while (Mathf.Abs(diff) > tolerance) {
                i++;
                if (i >= 100) break;
                
                if (diff > 0) { x -= step; step /= 2f; }
                else { x += step; step /= 2; }
                
                float y = pathCurve.Evaluate(x);
                end = pathStart + delta * x + rotatedNormal * y;
                float distance = (end - start).magnitude;
                diff = distance - length;
            }

            return (x, end);
        }

        private void generateFences(bool isInValidate = true) {
            if (!isInValidate) {
                Debug.LogError("Is in validate = false is not supported yet.");
                return;
            }
            
            float totalLength = getPathLength(pathCurve);

            GameObject finalPrefab = prefab;
            PrefabGroup prefabGroup = prefab.GetComponent<PrefabGroup>();
            if (prefabGroup) finalPrefab = prefabGroup.getRandomPrefab();
            
            Vector3 start = transform.position + pos1;
            FenceData fenceData = finalPrefab.GetComponent<FenceData>();
            float length = fenceData.getLength() * size;
            int fenceCount = (int)(totalLength / length);

            if (fenceCount > 500) {
                size = previousSize;
                fenceCount = 500;
                Debug.LogWarning("Input size is too small and created too many prefabs (Prefab limit is 500)!");
            }
            
            if (fenceCount > fences.Count) {
                foreach (Transform child in root) {
                    if (fences.Contains(child.gameObject)) continue;
                    if (child.name.EndsWith($":{fencePlacementId}")) fences.Add(child.gameObject);
                    else if (child.name.EndsWith($":C{fencePlacementId}")) endCapInst = child.gameObject;
                }
            }
            
            if (fenceCount > fences.Count) {
                instantiate(fenceCount - fences.Count, () => {
                    arrangeFences(start, length, isInValidate);
                }, isInValidate);
            } else if (fenceCount < fences.Count) {
                # if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    for (int i = 0; i < fences.Count - fenceCount; i++) {
                        GameObject fence = fences[^1];
                        fences.Remove(fence);
                        DestroyImmediate(fence);
                    }
                    arrangeFences(start, length, isInValidate);
                };
                # endif
            }
            else {
                arrangeFences(start, length, isInValidate);
            }
            
            # if UNITY_EDITOR
            EditorApplication.delayCall += () => {
                EditorApplication.delayCall += () => {
                    arrangeFences(start, length, isInValidate);
                };
            };
            # endif
        }

        private void arrangeFences(Vector3 start, float length, bool isInValidate) {
            float lastX = 0;

            if (!this) return;
            
            for (int i = 0; i < fences.Count; i++) {
                (float x, Vector3 end) data = approachingForEnd(lastX, start, length);
                lastX = data.x;

                Vector3 cachedStart = start;
                start = data.end;

                Vector3 upward = frozenAxis switch {
                    SingleAxisOption.X => Vector3.right,
                    SingleAxisOption.Y => Vector3.up,
                    SingleAxisOption.Z => Vector3.forward,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var i1 = i;
                GameObject inst = fences[i1];
                inst.transform.rotation = Quaternion.LookRotation(data.end - cachedStart, upward);
                inst.transform.localScale = new Vector3(size, size, size);
                FenceData fenceData = inst.GetComponent<FenceData>();
                Vector3 moveTo = cachedStart - (fenceData.getStartWorldPos() - fenceData.transform.position);
                fenceData.transform.position = moveTo;

                actualEnd = data.end;
                endRotation = inst.transform.rotation;
            }

            if (endCapPrefab) {
                instantiateEndCap(() => {
                    endCapInst.transform.rotation = endRotation;
                    endCapInst.transform.localScale = new Vector3(size, size, size);
                    FenceData fenceData = endCapInst.GetComponent<FenceData>();
                    Vector3 moveTo = actualEnd - (fenceData.getStartWorldPos() - fenceData.transform.position);
                    endCapInst.transform.position = moveTo;
                }, isInValidate);
            }
        }

        private void instantiateEndCap(Action onComplete, bool isInValidate = false) {
            GameObject finalEndCapPrefab = endCapPrefab;
            PrefabGroup endCapPrefabGroup = endCapPrefab.GetComponent<PrefabGroup>();
            if (endCapPrefabGroup) finalEndCapPrefab = endCapPrefabGroup.getRandomPrefab();

            if (isInValidate) {
                # if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    if (!endCapInst) {
                        endCapInst = (GameObject)PrefabUtility.InstantiatePrefab(finalEndCapPrefab, root);
                        endCapInst.name += $" :C{fencePlacementId}";
                    }
                    onComplete();
                };
                # endif
            }
        }

        private void instantiate(int num, Action action, bool isInValidate = false) {
            GameObject finalPrefab = prefab;
            PrefabGroup prefabGroup = prefab.GetComponent<PrefabGroup>();
            if (prefabGroup) finalPrefab = prefabGroup.getRandomPrefab();
            
            GameObject inst;
            
            if (isInValidate) {
                # if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    for (int i = 0; i < num; i++) {
                        inst = (GameObject)PrefabUtility.InstantiatePrefab(finalPrefab, root);
                        inst.name += $" :{fencePlacementId}";
                        fences.Add(inst);
                    }
                    action();
                };
                # endif
            }
        }

        private float getPathLength(AnimationCurve curve, int sampleCount = 30) {
            if (curve == null || curve.length < 2)
                return (pos2 - pos1).magnitude;

            float length = 0f;
            float timeStart = 0;
            float deltaTime = 1f / sampleCount;

            Vector3 pathStart = transform.position + pos1;
            Vector3 previousPoint = pathStart;

            for (int i = 1; i <= sampleCount; i++) {
                float currentTime = timeStart + deltaTime * i;
                
                Vector3 delta = pos2 - pos1;
                Vector3 normal = delta.normalized;
                Quaternion rotation = Quaternion.Euler(0, -90, 0); 
                Vector3 rotatedNormal = rotation * normal;
                float y = curve.Evaluate(currentTime);
                Vector3 currentPoint = pathStart + delta * currentTime + rotatedNormal * y;
                
                length += Vector3.Distance(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            return length;
        }

        private void OnDrawGizmos() {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawSphere(transform.position + pos1, 0.2f);
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(transform.position + pos2, 0.2f);
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawLine(transform.position + pos1, transform.position + pos2);
        }
        # endif
    }
}