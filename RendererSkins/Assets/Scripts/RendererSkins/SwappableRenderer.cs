using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RendererSkins {
    [ExecuteAlways]
    public class SwappableRenderer : MonoBehaviour {
        [SerializeField] private RendererSkinSystem system;

        [NonSerialized] private GameObject prefabInstance;
        [NonSerialized] private RendererSkinSystem previousSystem;

        public RendererSkinSystem System {
            get { return system; }
            set {
                if (system != null)
                    system.onActiveSkinChanged -= UpdateSkin;

                previousSystem = system = value;
                UpdateSkin(system == null ? null : system.ActiveSkin);

                if (system != null)
                    system.onActiveSkinChanged += UpdateSkin;
            }
        }

        private void Reset() {
            System = FindObjectOfType<RendererSkinSystem>();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            //NOTE: Destroying is not permitted during OnValidate, so let's be safe and call this editor UX improvement on the next editor update frame
            EditorApplication.delayCall += () => {
                if (system != previousSystem) {
                    //NOTE: The inspector changed the system, and we need to react to it through code!
                    System = system;
                }
            };
        }
#endif

        private void OnEnable() {
            if (system != null) {
                system.onActiveSkinChanged -= UpdateSkin;
                system.onActiveSkinChanged += UpdateSkin;
            }
        }

        private void OnDisable() {
            if (system != null) {
                system.onActiveSkinChanged -= UpdateSkin;
            }
        }

        private void UpdateSkin(RendererSkin skin) {
            if (prefabInstance != null) {
                if (Application.IsPlaying(this))
                    GameObject.Destroy(prefabInstance);
                else
                    GameObject.DestroyImmediate(prefabInstance);
                prefabInstance = null;
            }

            if (skin != null) {
                prefabInstance = GameObject.Instantiate(skin.Prefab, transform, false);
                prefabInstance.hideFlags = HideFlags.DontSave;
            }
        }
    }
}
