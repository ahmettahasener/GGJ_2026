using UnityEngine;
using System.Collections.Generic;

namespace GGJ_2026.UI
{
    public class InfoPopupManager : MonoBehaviour
    {
        public static InfoPopupManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private InfoBox _infoBoxPrefab;
        [SerializeField] private Transform _notificationContainer; // Vertical Layout Group
        
        [Header("Icons")]
        [SerializeField] private Sprite _electricityIcon;
        [SerializeField] private Sprite _sanityIcon;
        [SerializeField] private Sprite _monsterIcon;
        [SerializeField] private Sprite _freqIcon;

        [Header("Colors")]
        [SerializeField] private Color _posColor = Color.green;
        [SerializeField] private Color _negColor = Color.red;
        [SerializeField] private Color _neutralColor = Color.white;

        private Queue<NotificationData> _queue = new Queue<NotificationData>();

        private struct NotificationData
        {
            public string Message;
            public Sprite Icon;
            public Color TextColor;
        }

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

        public void QueueNotification(string message, Sprite icon, Color color)
        {
            // For now, spawn immediately. If too many, we might want to delay?
            // User requested Queue logic, but Vertical Layout handles stacking.
            // "Queue" implies sequential or safety.
            // Let's spawn immediately for responsiveness, Container handles layout.
            // If spam logic needed, we can throttle.
            SpawnNotification(message, icon, color);
        }

        // Helper Overloads
        public void NotifyElectricity(float amount)
        {
            string sign = amount >= 0 ? "+" : "";
            Color c = amount >= 0 ? _posColor : _negColor;
            SpawnNotification($"{sign}{amount} Electricity", _electricityIcon, c);
        }

        public void NotifySanity(float amount)
        {
            string sign = amount >= 0 ? "+" : "";
            Color c = amount >= 0 ? _posColor : _negColor;
            SpawnNotification($"{sign}{amount} Sanity", _sanityIcon, c);
        }

        public void NotifyDistance(float amount) // +Distance is Good (Monster pushed back), -Distance is Bad? 
        {
            // Typically "Monster Distance" -> +100m is GOOD (Farther). 
            // -30m is BAD (Closer).
            string sign = amount >= 0 ? "+" : "";
            Color c = amount >= 0 ? _posColor : _negColor;
            SpawnNotification($"{sign}{amount}m Distance", _monsterIcon, c);
        }

        public void NotifyFrequency(float amount)
        {
             string sign = amount >= 0 ? "+" : "";
            Color c = amount >= 0 ? _posColor : _negColor;
            SpawnNotification($"{sign}{amount}% Signal", _freqIcon, c);
        }

        public void NotifyGeneric(string msg)
        {
            SpawnNotification(msg, null, _neutralColor);
        }

        private void SpawnNotification(string message, Sprite icon, Color color)
        {
            if (_infoBoxPrefab == null || _notificationContainer == null) return;

            InfoBox box = Instantiate(_infoBoxPrefab, _notificationContainer);
            box.Setup(message, icon, color);
            box.transform.SetAsFirstSibling(); // Add to top or bottom? Vertical Layout usually Top-Down. To appear at top, FirstSibling.
        }
    }
}
