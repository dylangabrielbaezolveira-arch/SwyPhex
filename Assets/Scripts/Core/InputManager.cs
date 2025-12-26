using UnityEngine;
using System.Collections.Generic;

namespace SwyPhexLeague.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        
        [System.Serializable]
        public class ControlScheme
        {
            public string name;
            public Vector2 joystickPosition = new Vector2(0.1f, 0.1f);
            public float joystickSize = 150f;
            public Vector2 jumpButtonPosition = new Vector2(-150f, 100f);
            public Vector2 boostButtonPosition = new Vector2(-150f, 0f);
            public Vector2 abilityButtonPosition = new Vector2(-150f, -100f);
        }
        
        [Header("Control Schemes")]
        public ControlScheme[] controlSchemes;
        public int currentSchemeIndex = 0;
        
        [Header("Mobile Input")]
        public GameObject virtualJoystick;
        public RectTransform joystickHandle;
        public RectTransform joystickBackground;
        public UnityEngine.UI.Button jumpButton;
        public UnityEngine.UI.Button boostButton;
        public UnityEngine.UI.Button abilityButton;
        
        [Header("Input State")]
        private Vector2 joystickInput = Vector2.zero;
        private bool jumpPressed = false;
        private bool jumpHeld = false;
        private bool boostPressed = false;
        private bool boostHeld = false;
        private bool abilityPressed = false;
        
        private float lastJumpTime = 0f;
        private float doubleTapThreshold = 0.3f;
        private int tapCount = 0;
        
        private bool isMobile = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            DetectPlatform();
            SetupControls();
        }
        
        private void DetectPlatform()
        {
            #if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
            #else
            isMobile = false;
            #endif
        }
        
        private void SetupControls()
        {
            if (isMobile && virtualJoystick)
            {
                virtualJoystick.SetActive(true);
                ApplyControlScheme();
            }
            else if (virtualJoystick)
            {
                virtualJoystick.SetActive(false);
            }
            
            SetupButtons();
        }
        
        private void ApplyControlScheme()
        {
            if (controlSchemes.Length == 0) return;
            
            ControlScheme scheme = controlSchemes[currentSchemeIndex];
            
            if (joystickBackground)
            {
                joystickBackground.anchoredPosition = scheme.joystickPosition;
                joystickBackground.sizeDelta = new Vector2(scheme.joystickSize, scheme.joystickSize);
            }
            
            // Posicionar botones
            // (Implementar según tu UI específica)
        }
        
        private void SetupButtons()
        {
            if (jumpButton)
            {
                jumpButton.onClick.AddListener(() => {
                    jumpPressed = true;
                    jumpHeld = true;
                    CheckDoubleTap();
                });
            }
            
            if (boostButton)
            {
                boostButton.onClick.AddListener(() => {
                    boostPressed = true;
                    boostHeld = true;
                });
            }
            
            if (abilityButton)
            {
                abilityButton.onClick.AddListener(() => {
                    abilityPressed = true;
                });
            }
        }
        
        private void Update()
        {
            if (isMobile)
            {
                HandleMobileInput();
            }
            else
            {
                HandleDesktopInput();
            }
            
            UpdateButtonStates();
        }
        
        private void HandleMobileInput()
        {
            // Joystick virtual
            if (joystickHandle && joystickBackground)
            {
                Vector2 joystickDirection = joystickHandle.anchoredPosition - joystickBackground.anchoredPosition;
                float maxDistance = joystickBackground.sizeDelta.x / 2f;
                
                joystickInput = joystickDirection / maxDistance;
                joystickInput = Vector2.ClampMagnitude(joystickInput, 1f);
            }
        }
        
        private void HandleDesktopInput()
        {
            joystickInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
            jumpHeld = Input.GetKey(KeyCode.Space);
            
            boostPressed = Input.GetKeyDown(KeyCode.LeftShift);
            boostHeld = Input.GetKey(KeyCode.LeftShift);
            
            abilityPressed = Input.GetKeyDown(KeyCode.E);
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckDoubleTap();
            }
        }
        
        private void CheckDoubleTap()
        {
            if (Time.time - lastJumpTime < doubleTapThreshold)
            {
                tapCount++;
                if (tapCount >= 2)
                {
                    // Double tap detected
                    tapCount = 0;
                }
            }
            else
            {
                tapCount = 1;
            }
            
            lastJumpTime = Time.time;
        }
        
        private void UpdateButtonStates()
        {
            if (jumpPressed)
            {
                jumpPressed = false;
            }
            
            if (boostPressed)
            {
                boostPressed = false;
            }
            
            if (abilityPressed)
            {
                abilityPressed = false;
            }
            
            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
            {
                jumpHeld = false;
            }
            
            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.LeftShift))
            {
                boostHeld = false;
            }
        }
        
        public float GetHorizontalAxis()
        {
            return joystickInput.x;
        }
        
        public float GetVerticalAxis()
        {
            return joystickInput.y;
        }
        
        public bool GetJumpDown()
        {
            return jumpPressed;
        }
        
        public bool GetJump()
        {
            return jumpHeld;
        }
        
        public bool GetBoostDown()
        {
            return boostPressed;
        }
        
        public bool GetBoost()
        {
            return boostHeld;
        }
        
        public bool GetAbilityDown()
        {
            return abilityPressed;
        }
        
        public bool GetDoubleTapJump()
        {
            bool doubleTap = tapCount >= 2;
            if (doubleTap)
            {
                tapCount = 0;
            }
            return doubleTap;
        }
        
        public void SetControlScheme(int index)
        {
            if (index >= 0 && index < controlSchemes.Length)
            {
                currentSchemeIndex = index;
                ApplyControlScheme();
            }
        }
        
        public ControlScheme GetCurrentScheme()
        {
            return controlSchemes.Length > 0 ? controlSchemes[currentSchemeIndex] : null;
        }
        
        public bool IsMobile => isMobile;
        
        public void OnJoystickDrag(Vector2 position)
        {
            if (joystickBackground)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickBackground, 
                    position, 
                    null, 
                    out localPoint
                );
                
                float maxDistance = joystickBackground.sizeDelta.x / 2f;
                Vector2 clampedPoint = Vector2.ClampMagnitude(localPoint, maxDistance);
                
                if (joystickHandle)
                {
                    joystickHandle.anchoredPosition = clampedPoint;
                }
            }
        }
        
        public void OnJoystickRelease()
        {
            if (joystickHandle)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
            joystickInput = Vector2.zero;
        }
    }
}
