using UnityEngine;

namespace ProceduralMines.Unity
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CavernExplorer : MonoBehaviour
    {
        public Transform View;
        public CavernDemo Owner;
        private CharacterController controller;
        private float pitch, verticalSpeed;

        private void Awake() => controller = GetComponent<CharacterController>();
        public void ResetLook() { pitch = 0; verticalSpeed = 0; if (View != null) View.localRotation = Quaternion.identity; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Unlock();
            // Leave the settings panel usable; no movement while entering a seed.
            var guiMouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (Input.GetMouseButtonDown(0) && (!Owner.ShowInterface || !Owner.SettingsPanelRect.Contains(guiMouse)))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            transform.Rotate(0, Input.GetAxisRaw("Mouse X") * 2f, 0);
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * 2f, -85, 85);
            View.localRotation = Quaternion.Euler(pitch, 0, 0);
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1);
            float speed = Input.GetKey(KeyCode.LeftShift) ? 6f : 3.5f;
            Vector3 movement = (transform.right * input.x + transform.forward * input.y) * speed;
            verticalSpeed = controller.isGrounded ? -2f : Mathf.Max(verticalSpeed - 20f * Time.deltaTime, -30f);
            movement.y = verticalSpeed;
            controller.Move(movement * Mathf.Min(Time.deltaTime, 0.05f));
            if (Input.GetKeyDown(KeyCode.R)) Owner.ResetPlayer();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) Unlock(); }
        private void OnDisable() => Unlock();
        private static void Unlock() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
