using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RendererSkins {
    public class RendererSkinSystem : MonoBehaviour {
        [Tooltip("The current skin that is applied to all " + nameof(SwappableRenderer) + "s in the scene.")]
        [SerializeField] private RendererSkin activeSkin;

        //NOTE: This variable is just for Editor GUI change-checking,
        //So that when the user changes the activeSkin field in the inspector,
        //OnValidate() notices immediately, and sends C# events for the rest of the scene to update.
        [NonSerialized] private RendererSkin previousSkin;

        public event Action<RendererSkin> onActiveSkinChanged;

        public RendererSkin ActiveSkin {
            get { return activeSkin; }
            set {
                previousSkin = activeSkin = value;
                Debug.Log("CHANGED to " + (activeSkin == null ? "null" : activeSkin.name));
                onActiveSkinChanged?.Invoke(activeSkin);
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            //NOTE: Destroying is not permitted during OnValidate, so let's be safe and call this editor UX improvement on the next editor update frame
            EditorApplication.delayCall += () => {
                if (activeSkin != previousSkin) {
                    //NOTE: The inspector changed the activeSkin, and we need to react to it through code!
                    ActiveSkin = activeSkin;
                }
            };
        }
#endif
    }
}
