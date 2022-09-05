using UnityEngine;

namespace RendererSkins {
    /// <summary>
    /// Represents how objects in a scene look visually, based on the environment or current game level.
    /// </summary>
    [CreateAssetMenu]
    public class RendererSkin : ScriptableObject {
        [SerializeField] private GameObject prefab;

        public GameObject Prefab => prefab;
    }
}
