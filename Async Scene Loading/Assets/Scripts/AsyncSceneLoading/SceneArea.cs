using UnityEngine;

namespace AsyncSceneLoading {
    /// <summary>
    /// Defines a scene that gets automatically loaded when object(s) of interest enter a certain area.
    /// </summary>
    [CreateAssetMenu]
    public class SceneArea : ScriptableObject {
        [Tooltip("The asset path of the scene to load when an object of interest enters the given area.")]
        [SerializeField] private string scenePath;

        [Tooltip("Defines the volume in world-space that triggers the automatic loading of this scene.\n" +
            "When the object(s) of interest leave this area, the scene will be unloaded.")]
        [SerializeField] private Bounds bounds;

        public string ScenePath => scenePath;
        public Bounds Bounds => bounds;
    }
}
