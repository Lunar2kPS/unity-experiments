using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AsyncSceneLoading {
    public class PlayerController : MonoBehaviour {
        //NOTE: I would use a InputActionAsset instead of Key values directly, but this is for prototyping quickly!
        [Serializable]
        public struct InputKeys {
            public static InputKeys Default => new InputKeys {
                left = Key.LeftArrow,
                right = Key.RightArrow,
                up = Key.UpArrow,
                down = Key.DownArrow
            };

            public Key left;
            public Key right;
            public Key down;
            public Key up;
        }

        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private float speed = 5;
        [SerializeField] private InputKeys keys = InputKeys.Default;

        private Vector2Int input;

        private Vector3 DesiredVelocity {
            get {
                Vector3 result = default;
                result.x = input.x;
                result.z = input.y;
                return result.normalized * speed;
            }
        }

        #region Unity Messages
        private void Reset() {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update() {
            Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
            if (keyboard == null) {
                Debug.LogError("Failed to find keyboard device!");
                return;
            }

            UpdateInputStates(keyboard);
        }

        private void FixedUpdate() {
            rigidbody.velocity = DesiredVelocity;
        }
        #endregion

        private void UpdateInputStates(Keyboard keyboard) {
            input.Set(0, 0);
            input.x += keyboard[keys.left].isPressed ? -1 : 0;
            input.x += keyboard[keys.right].isPressed ? 1 : 0;
            input.y += keyboard[keys.down].isPressed ? -1 : 0;
            input.y += keyboard[keys.up].isPressed ? 1 : 0;
        }
    }
}
