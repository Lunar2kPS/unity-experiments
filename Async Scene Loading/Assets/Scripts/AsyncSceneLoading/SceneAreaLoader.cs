using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AsyncSceneLoading {
    public class SceneAreaLoader : MonoBehaviour {
        [SerializeField] private Transform[] objectsOfInterest;
        [SerializeField] private SceneArea[] areas;

        private HashSet<string> inProgressLoads = new HashSet<string>();

        public IEnumerable<Transform> ObjectsOfInterest => objectsOfInterest.Where(obj => obj != null);
        public IEnumerable<SceneArea> Areas => areas.Where(a => a != null);

        #region Unity Messages
        private void OnDrawGizmosSelected() {
            foreach (SceneArea area in Areas) {
                Bounds bounds = area.Bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        private void Update() {
            //NOTE: Not fully optimized due to all the Transform.position calls
            foreach (SceneArea area in Areas) {
                if (IsInProgress(area))
                    continue;

                string scenePath = area.ScenePath;
                bool shouldBeLoaded = ContainsAnyObjectsOfInterest(area);
                bool isLoaded = SceneManager.GetSceneByPath(scenePath).IsValid();

                if (isLoaded != shouldBeLoaded) {
                    if (shouldBeLoaded) {
                        inProgressLoads.Add(scenePath);
                        SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive).completed += (AsyncOperation a) => {
                            inProgressLoads.Remove(scenePath);
                        };
                    } else {
                        inProgressLoads.Add(scenePath);
                        SceneManager.UnloadSceneAsync(scenePath).completed += (AsyncOperation a) => {
                            inProgressLoads.Remove(scenePath);
                        };
                    }
                }
            }
        }
        #endregion

        private bool ContainsAnyObjectsOfInterest(SceneArea area) {
            Bounds b = area.Bounds;
            foreach (Transform obj in ObjectsOfInterest)
                if (b.Contains(obj.position))
                    return true;
            return false;
        }

        private bool IsInProgress(SceneArea area) => area != null && IsInProgress(area.ScenePath);
        private bool IsInProgress(string scenePath) => inProgressLoads.Contains(scenePath);
    }
}
