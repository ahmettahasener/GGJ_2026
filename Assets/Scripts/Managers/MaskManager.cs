using GGJ_2026.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GGJ_2026.Managers
{
    public class MaskManager : MonoBehaviour
    {
        public static MaskManager Instance { get; private set; }

        [Header("Mask Library")]
        [SerializeField] private List<MaskData> _allMasks;

        public MaskData ActiveMask { get; private set; }

        private Coroutine _clearRoutine;

        public Action cardSelectedEvent;

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
            }
        }

        [Header("Spawning")]
        [SerializeField] private GameObject _maskObjectPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private List<GameObject> _spawnedMasks = new List<GameObject>();

        public void StartSelectionPhase()
        {
            ClearSpawnedMasksImmediate();

            List<MaskData> options = GetRandomMasks(3);
            if (options.Count == 0 || _maskObjectPrefab == null)
                return;

            for (int i = 0; i < options.Count; i++)
            {
                if (i >= _spawnPoints.Length) break;

                GameObject obj = Instantiate(
                    _maskObjectPrefab,
                    _spawnPoints[i].position,
                    _spawnPoints[i].rotation
                );

                var maskScript = obj.GetComponent<Machines.MaskObject>();
                if (maskScript != null)
                    maskScript.Initialize(options[i]);

                _spawnedMasks.Add(obj);

                Debug.Log($"Spawned {_spawnedMasks.Count} masks for selection.");
            }
        }

        public void SelectMaskFromObject(MaskData mask)
        {
            ActivateMask(mask);
            ApplyMaskEffects(mask);
            ClearSpawnedMasksDelayed();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Gameplay);
            }
            cardSelectedEvent?.Invoke();
        }

        private void ApplyMaskEffects(MaskData mask)
        {
            if (ResourceManager.Instance == null) return;

            // Reset first
            ResourceManager.Instance.ResetModifiers();

            switch (mask.Type)
            {
                case MaskType.Observer:
                    // Canavar %50 yavaş
                    ResourceManager.Instance.SetMonsterAdvanceMultiplier(0.5f);
                    break;

                case MaskType.StrongPusher:
                    // Güçlü İtici: Radar push increased
                    ResourceManager.Instance.SetRadarPushMultiplier(1.5f); 
                    break;

                case MaskType.EfficientRadar:
                    // Verimli Radar: Cost lower, Fuse chance 0
                    ResourceManager.Instance.SetRadarCostMultiplier(0.2f); // Very cheap
                    ResourceManager.Instance.SetRadarFuseChanceMultiplier(0f);
                    break;

                case MaskType.RiskTaker:
                    // Risk Alan: Radio x2 speed, Monster x1.5 speed
                    ResourceManager.Instance.SetRadioFrequencyMultiplier(2.0f);
                    ResourceManager.Instance.SetMonsterAdvanceMultiplier(1.5f);
                    break;

                case MaskType.MadnessPerk:
                    // Delilik Avantajı: Dynamic handled in RadioMachine, likely no static modifier here 
                    // or base multiplier is 1 defined in Reset.
                    break;

                case MaskType.DurableLine:
                    // Dayanıklı Hat: Fuse never blows
                    ResourceManager.Instance.SetFuseBlockActive(true);
                    break;

                case MaskType.FreeMedicine:
                    // Bedava İlaç
                    ResourceManager.Instance.SetMedicineCostMultiplier(0f);
                    break;
            }

            Debug.Log($"Mask Effects Applied for: {mask.MaskName}");
        }
        private void ClearSpawnedMasksImmediate()
        {
            for (int i = _spawnedMasks.Count - 1; i >= 0; i--)
            {
                if (_spawnedMasks[i] != null)
                {
                    Destroy(_spawnedMasks[i]);
                }
            }

            _spawnedMasks.Clear();
        }
        private void ClearSpawnedMasksDelayed()
        {
            if (_clearRoutine != null)
                StopCoroutine(_clearRoutine);

            _clearRoutine = StartCoroutine(ClearSpawnedMasksRoutine());
        }

        private IEnumerator ClearSpawnedMasksRoutine()
        {
            // Interaction'ı kapat
            foreach (var mask in _spawnedMasks)
            {
                if (mask == null) continue;

                var col = mask.GetComponent<Collider>();
                if (col != null)
                    col.enabled = false;
            }

            // Interaction sisteminin frame’i bitsin
            yield return null;

            // Destroy
            for (int i = _spawnedMasks.Count - 1; i >= 0; i--)
            {
                if (_spawnedMasks[i] != null)
                {
                    Debug.Log(_spawnedMasks[i] + " card destroyed.");
                    Destroy(_spawnedMasks[i]);
                }
            }

            _spawnedMasks.Clear();
        }

        // --- Helper Methods ---
        public List<MaskData> GetRandomMasks(int count = 3)
        {
            if (_allMasks == null || _allMasks.Count == 0) return new List<MaskData>();
            
            // Simple shuffle and take
            return _allMasks.OrderBy(x => UnityEngine.Random.value).Take(Mathf.Min(count, _allMasks.Count)).ToList();
        }

        public void ActivateMask(MaskData mask)
        {
            ActiveMask = mask;
            Debug.Log($"Mask Activated: {mask.MaskName} - {mask.Description}");
        }

        public void ClearMask()
        {
            ActiveMask = null;
        }

        public bool IsEffectActive(MaskType type)
        {
            return ActiveMask != null && ActiveMask.Type == type;
        }
    }
}
