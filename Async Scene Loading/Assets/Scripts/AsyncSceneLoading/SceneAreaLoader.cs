using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AsyncSceneLoading {
    /// <summary>
    /// A <see cref="MonoBehaviour"/> script that checks every on Update whether it should be loading or unloading scenes based on the Transform positions of the <see cref="ObjectsOfInterest"/>.
    /// </summary>
    [ExecuteAlways]
    public class SceneAreaLoader : MonoBehaviour {
        [Tooltip("By default, gizmos are drawn based on any scene object selections you currently have in the editor.\n\n" +
            "Set this value to false to stop drawing gizmos.")]
        [SerializeField] private bool drawGizmos = true;

        [Tooltip("Defines one or more object(s) that determine the areas around which maps should be loaded at any given time.")]
        [SerializeField] private Transform[] objectsOfInterest;

        [Tooltip("The list of all loadable scenes and their respective world-space bounds.")]
        [SerializeField] private SceneArea[] areas;

        /// <summary>
        /// A collection of all the scene asset paths of in-progress async scene loads and unloads.<br />
        /// This allows us to prevent trying to start an additional scene load or scene unload while that same scene is already loading or unloading.
        /// </summary>
        private HashSet<string> inProgressLoads = new HashSet<string>();

        private List<string> selectedScenePaths = null;

        /// <summary>
        /// A collection of all the <see cref="objectsOfInterest"/> without <c>null</c> values.
        /// </summary>
        public IEnumerable<Transform> ObjectsOfInterest => objectsOfInterest.Where(obj => obj != null);

        /// <summary>
        /// A collection of all the <see cref="areas"/> without <c>null</c> values.
        /// </summary>
        public IEnumerable<SceneArea> Areas => areas.Where(a => a != null);

        #region Unity Messages
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (drawGizmos)
                DrawGizmos(selectedScenePaths);
        }

        private void OnEnable() {
            selectedScenePaths = new List<string>();
            Selection.selectionChanged -= UpdateSelectedScenePaths;
            Selection.selectionChanged += UpdateSelectedScenePaths;
        }

        private void OnDisable() {
            Selection.selectionChanged -= UpdateSelectedScenePaths;
            selectedScenePaths = null;
        }
#endif

        private void Update() {
            if (!Application.IsPlaying(this))
                return;

            //NOTE: Not fully optimized due to all the Transform.position calls
            foreach (SceneArea area in Areas) {
                if (IsInProgress(area))
                    continue;

                string scenePath = area.ScenePath;
                bool shouldBeLoaded = ContainsAnyObjectsOfInterest(area);
                bool isLoaded = SceneManager.GetSceneByPath(scenePath).IsValid();

                if (isLoaded != shouldBeLoaded) { //If there's a DIFFERENCE between the scene's state of being loading vs. what it SHOULD be,
                    if (shouldBeLoaded)
                        StartLoadingAsync(scenePath);
                    else
                        StartUnloadingAsync(scenePath);
                }
            }
        }
#endregion

        /// <summary>
        /// Checks if any of the <see cref="ObjectsOfInterest"/> are contained within the world-space bounds of the given <see cref="SceneArea"/>.
        /// </summary>
        private bool ContainsAnyObjectsOfInterest(SceneArea area) {
            Bounds b = area.Bounds;
            foreach (Transform obj in ObjectsOfInterest)
                if (b.Contains(obj.position))
                    return true;
            return false;
        }

        /// <summary>
        /// Checks if the scene given by <paramref name="area"/> is currently loading or unloading already.
        /// </summary>
        private bool IsInProgress(SceneArea area) => area != null && IsInProgress(area.ScenePath);

        /// <summary>
        /// Checks if the scene given by <paramref name="scenePath"/> is currently loading or unloading already.
        /// </summary>
        private bool IsInProgress(string scenePath) => inProgressLoads.Contains(scenePath);

        private AsyncOperation StartLoadingAsync(string scenePath) {
            inProgressLoads.Add(scenePath);
            AsyncOperation operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            operation.completed += (AsyncOperation a) => inProgressLoads.Remove(scenePath);
            return operation;
        }

        private AsyncOperation StartUnloadingAsync(string scenePath) {
            inProgressLoads.Add(scenePath);
            AsyncOperation operation = SceneManager.UnloadSceneAsync(scenePath);
            operation.completed += (AsyncOperation a) => inProgressLoads.Remove(scenePath);
            return operation;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Updates the list of scenes that the user has objects selected inside of.<br />
        /// For UX, we only draw the gizmos of scenes that the user is selected objects inside of, so they can get an idea of the extents of each scene's bounds.
        /// </summary>
        private void UpdateSelectedScenePaths() {
            selectedScenePaths.Clear();
            foreach (Object obj in Selection.objects) {
                switch (obj) {
                    case GameObject gameObject:
                        selectedScenePaths.Add(gameObject.scene.path);
                        break;
                    case Component component:
                        selectedScenePaths.Add(component.gameObject.scene.path);
                        break;
                }
            }
            selectedScenePaths.RemoveAll(s => Areas.FirstOrDefault(a => a.ScenePath == s) == null);
        }

        private void DrawGizmos(IEnumerable<string> scenePaths) {
            int i = 0;
            foreach (string scenePath in scenePaths) {
                SceneArea area = Areas.FirstOrDefault(s => s.ScenePath == scenePath);
                if (area == null) {
                    Debug.LogWarning("Failed to draw gizmos for scenePath = " + scenePath);
                    continue;
                }
                DrawGizmos(area, i++);
            }
        }

        private void DrawGizmos(SceneArea area, int seed) {
            Color prev = Gizmos.color;
            Random.State prevState = Random.state;
            try {
                Random.InitState(seed);
                Color color = Color.HSVToRGB(Random.Range(0f, 1f), 0.8f, 0.8f);
                color.a = 0.3f;
                Color outlineColor = 2 * color;
                outlineColor.a = 0.3f;

                Gizmos.color = color;
                Bounds bounds = area.Bounds;
                Gizmos.DrawCube(bounds.center, bounds.size);

                Gizmos.color = outlineColor;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            } finally {
                Gizmos.color = prev;
                Random.state = prevState;
            }
        }
#endif
    }
}
