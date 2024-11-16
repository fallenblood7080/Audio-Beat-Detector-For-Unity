using UnityEngine;
using UnityEngine.Audio;

namespace Beat
{
    [CreateAssetMenu(fileName = "BeatConfigSO", menuName = "Scriptable Objects/BeatConfigSO")]
    public class BeatConfigSO : ScriptableObject
    {
        [SerializeField, Range(0, 1)] private float _smoothingFactor = 0.1f;

        [SerializeField] private int _spectrumResolution = 512;
        [SerializeField] private FFTWindow _fftWindow = FFTWindow.Hanning;
        [SerializeField] private AudioMixerGroup _mixerGroup;

        [Header("Frequency Bands")]
        [SerializeField] private int _lowBandCount = 2;
        [SerializeField] private int _midBandCount = 3;
        [SerializeField] private int _highBandCount = 3;

        [Header("Energy Thresholds")]
        [SerializeField, Range(0, 1)] private float _lowThreshold = 0.3f;
        [SerializeField, Range(0, 1)] private float _midThreshold = 0.3f;
        [SerializeField, Range(0, 1)] private float _highThreshold = 0.3f;

        [Header("Event")]
        [SerializeField, Range(0, 1)] private float _eventCallInterval; 
 
        public float SmoothingFactor => _smoothingFactor;
        public int LowBandCount => _lowBandCount;
        public int MidBandCount => _midBandCount;
        public int HighBandCount => _highBandCount;
        public int SpectrumResolution => _spectrumResolution;
        public FFTWindow FftWindow => _fftWindow;
        public AudioMixerGroup MixerGroup => _mixerGroup;
        public float LowThreshold => _lowThreshold;
        public float MidThreshold => _midThreshold;
        public float HighThreshold => _highThreshold;

        public float EventCallInterval => _eventCallInterval;
    }
}
