using UnityEngine;
using UnityEngine.Audio;

namespace Beat
{
    /// <summary>
    /// Scriptable Object to store configuration for the Beat Manager.
    /// </summary>
    [CreateAssetMenu(fileName = "BeatConfigSO", menuName = "Scriptable Objects/BeatConfigSO")]
    public class BeatConfigSO : ScriptableObject
    {
        [SerializeField, Range(0.01f, 1f)] private float _smoothingFactor = 0.1f;
        [SerializeField, Range(64, 8192)] private int _spectrumResolution = 512;
        [SerializeField] private FFTWindow _fftWindow = FFTWindow.Hanning;
        [SerializeField] private AudioMixerGroup _mixerGroup;
        [SerializeField] private bool _useDominantEnergyOnly = false;

        [Header("Frequency Bands")]
        [SerializeField, Range(1, 10)] private int _lowBandCount = 2;
        [SerializeField, Range(1, 10)] private int _midBandCount = 3;
        [SerializeField, Range(1, 10)] private int _highBandCount = 3;

        [Header("Energy Thresholds")]
        [SerializeField, Range(0, 1)] private float _lowThreshold = 0.3f;
        [SerializeField, Range(0, 1)] private float _midThreshold = 0.3f;
        [SerializeField, Range(0, 1)] private float _highThreshold = 0.3f;

        [Header("Event")]
        [SerializeField, Range(0.01f, 1f)] private float _eventCallInterval = 0.1f;

        /// <summary>
        /// Ensures data integrity by validating fields when values change in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            // Ensure spectrum resolution is a power of 2
            if (!IsPowerOfTwo(_spectrumResolution))
            {
                Debug.LogWarning($"{nameof(_spectrumResolution)} must be a power of 2. Adjusting to nearest power of 2.", this);
                _spectrumResolution = Mathf.ClosestPowerOfTwo(_spectrumResolution);
            }

            // Ensure at least one band is configured
            int totalBands = _lowBandCount + _midBandCount + _highBandCount;
            if (totalBands == 0)
            {
                Debug.LogWarning("Total frequency bands must be at least 1. Setting default values.", this);
                _lowBandCount = 1;
                _midBandCount = 1;
                _highBandCount = 1;
            }

            // Ensure thresholds are logically correct
            if (_lowThreshold > _midThreshold || _midThreshold > _highThreshold)
            {
                Debug.LogWarning("Energy thresholds must be in ascending order (Low ≤ Mid ≤ High). Adjusting values.", this);
                _lowThreshold = Mathf.Min(_lowThreshold, _midThreshold);
                _midThreshold = Mathf.Max(_lowThreshold, Mathf.Min(_midThreshold, _highThreshold));
                _highThreshold = Mathf.Max(_midThreshold, _highThreshold);
            }
        }

        /// <summary>
        /// Checks if a given number is a power of 2.
        /// </summary>
        /// <param name="value">The number to check.</param>
        /// <returns>True if the number is a power of 2; otherwise, false.</returns>
        private bool IsPowerOfTwo(int value)
        {
            return (value > 0) && (value & (value - 1)) == 0;
        }

        public float SmoothingFactor => _smoothingFactor;
        public int LowBandCount => _lowBandCount;
        public int MidBandCount => _midBandCount;
        public int HighBandCount => _highBandCount;
        public float LowThreshold => _lowThreshold;
        public float MidThreshold => _midThreshold;
        public float HighThreshold => _highThreshold;
        public FFTWindow FftWindow => _fftWindow;
        public bool UseDominantEnergyOnly => _useDominantEnergyOnly;
        public int SpectrumResolution => _spectrumResolution;
        public AudioMixerGroup MixerGroup => _mixerGroup;
        public float EventCallInterval => _eventCallInterval;
    }
}
