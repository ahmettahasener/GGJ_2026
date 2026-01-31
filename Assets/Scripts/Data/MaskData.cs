using UnityEngine;

namespace GGJ_2026.Data
{
    public enum MaskType
    {
        Observer,       // Gözlemci
        StrongPusher,   // Güçlü İtici
        EfficientRadar, // Verimli Radar
        RiskTaker,      // Risk Alan
        MadnessPerk,    // Delilik Avantajı
        DurableLine,    // Dayanıklı Hat
        FreeMedicine    // Bedava İlaç
    }

    [CreateAssetMenu(fileName = "NewMaskData", menuName = "GGJ 2026/Mask Data")]
    public class MaskData : ScriptableObject
    {
        [Header("General Info")]
        [SerializeField] private string _maskName;
        [TextArea(3, 5)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Type")]
        [SerializeField] private MaskType _maskType;

        public string MaskName => _maskName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public MaskType Type => _maskType;
    }
}
