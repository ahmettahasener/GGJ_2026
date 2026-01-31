using UnityEngine;
using GGJ_2026.Managers;
using System.Collections;

namespace GGJ_2026.Machines
{
    public class FuseBox : MachineBase
    {
        [Header("Repair Settings")]
        [SerializeField] private float _repairDuration = 2.0f;
        [SerializeField] private Color _normalColor = Color.green;
        [SerializeField] private Color _brokenColor = Color.red;
        [SerializeField] private GameObject[] lighths;
        
        // This machine doesn't cost electricity to use.
        private void Awake()
        {
            _electricityCost = 0f;
        }

        private void Start()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnPowerStateChanged += HandlePowerState;
                HandlePowerState(ResourceManager.Instance.IsPowerOn);
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnPowerStateChanged -= HandlePowerState;
            }
        }

        private void HandlePowerState(bool isPowerOn)
        {
            // Visual feedback
            GetComponent<Renderer>().material.color = isPowerOn ? _normalColor : _brokenColor;
            ChangeActivity(isPowerOn);
        }

        public override void OnInteract()
        {
            if (ResourceManager.Instance == null) return;

            if (ResourceManager.Instance.IsPowerOn)
            {
                Debug.Log("Fuse Box is functioning correctly.");
            }
            else
            {
                StartCoroutine(RepairRoutine());
            }
        }

        private IEnumerator RepairRoutine()
        {
            Debug.Log("Repairing Fuse Box... Hold on...");
            // In a real implementation, we would check if player holds 'E'.
            // For now, assuming instant or short delay interaction as "RepairProcess".
            // Since PlayerInteract triggers OnInteract ONCE per press, we'll simulate a delay or assume it's a "Click to Repair" action.
            // If the user wanted "Hold", PlayerInteract needs to support "GetKey" instead of "GetKeyDown".
            // Implementation Plan assumed "Hold logic" might strictly need PlayerInteract update.
            // However, sticking to the existing Interact system: We will make it a "Process" that starts.
            
            yield return new WaitForSeconds(_repairDuration);

            Debug.Log("Fuse Box Repaired!");
            ResourceManager.Instance.RestorePower(10f, 70f); // 10% Current, 70% Max Next Night
        }

        private void ChangeActivity(bool isPowerOn)
        {
            foreach (GameObject obj in lighths)
            {
                obj.SetActive(isPowerOn);
            }
        }
    }
}
