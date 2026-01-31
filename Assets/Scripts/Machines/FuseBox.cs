using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class FuseBox : MachineBase
    {
        [Header("Fuse Box Mechanics")]
        [SerializeField] private Transform _door;
        [SerializeField] private List<Transform> _switches; // The 'Toggle' parts to rotate
        [SerializeField] private float _doorOpenAngle = 140f;
        
        [Header("Colors (Optional Feedback)")]
        [SerializeField] private Renderer _statusRenderer;
        [SerializeField] private Material _matOn;
        [SerializeField] private Material _matOff;

        private bool[] _switchStates; // true = ON (50), false = OFF (-50)
        private Coroutine _doorCoroutine;
        
        // Internal state
        private bool _isFocusing = false;

        private Collider _collider;

        private void Start()
        {
            InitializeSwitches();
            _collider = GetComponent<Collider>();
            
            // Ensure collider is enabled initially
            if (_collider != null) _collider.enabled = true;
        }

        private void Update()
        {
            // Debug Key
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("Debug: Triggering Power Outage manually.");
                ResourceManager.Instance.TriggerPowerOutage();
            }
        }

        private void InitializeSwitches()
        {
            if (_switches == null) return;
            
            _switchStates = new bool[_switches.Count];
            for (int i = 0; i < _switches.Count; i++)
            {
                _switchStates[i] = true; // All start ON
                SetSwitchVisual(i, true);
            }
        }

        protected override void ConsumeElectricity()
        {
            // FuseBox typically doesn't consume, it Just Exists.
            // Or maybe it consumes a tiny bit? Base implementation applies if cost > 0.
        }

        protected override void UseMachine()
        {
            // Called when we Enter interaction (inherited from OnInteract -> UseMachine)
        }

        public override void OnInteract()
        {
            base.OnInteract(); // Calls UseMachine
            
            _isFocusing = true;
            
            // 0. Disable Main Collider to allow clicking inside
            if (_collider != null) _collider.enabled = false;

            // 1. Open Door
            MoveDoor(true);

            // 2. Unlock Cursor for mini-game (Handled by PlayerInteract, but double check doesn't hurt)
             Cursor.lockState = CursorLockMode.None;
             Cursor.visible = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            
            _isFocusing = false;

            // 0. Re-enable Main Collider
            if (_collider != null) _collider.enabled = true;

            // 1. Close Door
            MoveDoor(false);

            // 2. Lock Cursor (Handled by PlayerInteract usually)
        }

        public override void OnInteractStay()
        {
            if (!_isFocusing) return;

            // Mini-game Input Logic - LEFT CLICK
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Use a smaller distance or specific layer if needed
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    // Check if we hit a switch
                    int index = _switches.IndexOf(hit.transform);
                    
                    // If not found, try parent (in case collider is on a container)
                    if (index == -1 && hit.transform.parent != null)
                    {
                        index = _switches.IndexOf(hit.transform.parent);
                    }

                    if (index != -1)
                    {
                        ToggleSwitch(index);
                    }
                }
            }
        }

        private void ToggleSwitch(int index)
        {
            // Logic: Can we toggle ANY switch? Or only OFF ones?
            // User: "kapalı olan şalterin üzerine gelip 'SPACE' tuşuna bastığında şalterin X rotasyonu 50'ye gelmeli (Açılmalı)."
            // Implies we function as a repair man. Maybe we can flip ON to OFF too? 
            // Better to allow toggling for realism, or strictly Repair.
            // Let's implement Toggle (Flip).
            
            bool newState = !_switchStates[index];
            _switchStates[index] = newState;
            SetSwitchVisual(index, newState);

            Debug.Log($"Switch {index} flipped to {newState}");

            CheckAllSwitches();
        }

        private void SetSwitchVisual(int index, bool isOn)
        {
            if (_switches[index] != null)
            {
                float angle = isOn ? 50f : -50f;
                _switches[index].localRotation = Quaternion.Euler(angle, 0, 0);
            }
        }

        private void CheckAllSwitches()
        {
            bool allOn = true;
            foreach (bool s in _switchStates)
            {
                if (!s) 
                {
                    allOn = false; 
                    break;
                }
            }

            if (allOn)
            {
                if (ResourceManager.Instance != null && !ResourceManager.Instance.IsPowerOn)
                {
                    Debug.Log("FuseBox Fixed! Restoring Power.");
                    // Restore to 100% current, but next night cap is 70%
                    ResourceManager.Instance.RestorePower(ResourceManager.Instance.GetElectricity(), 70f);
                    
                    if (_statusRenderer != null) _statusRenderer.material = _matOn;
                }
            }
        }

        // Called by ResourceManager via Event or direct reference?
        // Since ResourceManager doesn't know about FuseBox, FuseBox should listen? 
        // OR ResourceManager calls TriggerPowerOutage, and we need to respond.
        // We can subscribe to OnPowerStateChanged.
        
        private void OnEnable()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnPowerStateChanged += HandlePowerState;
            }
        }

        private void OnDisable()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnPowerStateChanged -= HandlePowerState;
            }
        }

        private void HandlePowerState(bool isPowerOn)
        {
            if (!isPowerOn)
            {
                // Fuse Blown! Scramble switches.
                ScrambleSwitches();
                if (_statusRenderer != null) _statusRenderer.material = _matOff;
            }
            else
            {
                 if (_statusRenderer != null) _statusRenderer.material = _matOn;
            }
        }

        private void ScrambleSwitches()
        {
            // User: "rastgele belirlenen birkaçı (örn: 3-5 adet) 'Kapalı' konuma geçmeli."
            if (_switchStates == null) return;

            int countToFail = Random.Range(3, 6); // 3, 4, or 5
            
            // Allow duplicates for simplicity (some might stay on/off), or ensure unique?
            // Let's ensure at least some turn off.
            
            for (int i = 0; i < countToFail; i++)
            {
                int rnd = Random.Range(0, _switches.Count);
                _switchStates[rnd] = false; // Force OFF
                SetSwitchVisual(rnd, false);
            }
        }

        // --- Door Animation ---
        private void MoveDoor(bool open)
        {
            if (_door == null) return;
            
            if (_doorCoroutine != null) StopCoroutine(_doorCoroutine);
            _doorCoroutine = StartCoroutine(AnimateDoor(open));
        }

        private IEnumerator AnimateDoor(bool open)
        {
            Quaternion startRot = _door.localRotation;
            Quaternion targetRot = Quaternion.Euler(0, open ? _doorOpenAngle : 0f, 0);
            
            float duration = 0.5f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                // Cubic ease out
                t = 1f - Mathf.Pow(1f - t, 3);
                
                _door.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }
            _door.localRotation = targetRot;
        }
    }
}
