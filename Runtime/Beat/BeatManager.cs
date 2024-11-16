using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Beat
{
    public class BeatManager : MonoSingleton<BeatManager>
    {
        private AudioSource _beatGenAudioSource;
        private float[] _spectrumData;

        private float[] _normalizedBandValues;
        private float[] _maxBandValues;

        [SerializeField] private BeatConfigSO _beatConfig;

        [SerializeField] private bool _enableBeatDebug = false;

        [Header("Unity Events")]
        public Action OnLowEnergy;
        public Action OnMidEnergy;
        public Action OnHighEnergy;

        private float _lowEnergy;
        private float _midEnergy;
        private float _highEnergy;

        private float _elaspedTimeAfterLastEvent;


        private bool _isBeatsInitialized = false;

        protected override void Awake()
        {
            base.Awake();

            if (!TryGetComponent<AudioSource>(out _beatGenAudioSource))
            {
                _beatGenAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void SetupBeat(AudioClip clip)
        {
            _isBeatsInitialized = false;

            if (!IsInitialized)
                return;

            if (_beatConfig == null)
            {
                Debug.LogError("Beat Config file is missing in Beat Manager",this);
                return;
            }

            _beatGenAudioSource.clip = clip;
            _beatGenAudioSource.outputAudioMixerGroup = _beatConfig.MixerGroup;

            _spectrumData = new float[_beatConfig.SpectrumResolution];
            _normalizedBandValues = new float[_beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount];
            _maxBandValues = new float[_beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount];

            _beatGenAudioSource.Play();
            _isBeatsInitialized = true;
        }

        private void Update()
        {
            if (!_isBeatsInitialized)
                return;

            _elaspedTimeAfterLastEvent += Time.deltaTime;

            _beatGenAudioSource.GetSpectrumData(_spectrumData, 0, _beatConfig.FftWindow);

            _lowEnergy = CalculateNormalizedEnergy(0, _beatConfig.LowBandCount);
            _midEnergy = CalculateNormalizedEnergy(_beatConfig.LowBandCount, _beatConfig.LowBandCount + _beatConfig.MidBandCount);
            _highEnergy = CalculateNormalizedEnergy(_beatConfig.LowBandCount + _beatConfig.MidBandCount, _beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount);

            // Trigger events based on thresholds
            TriggerEvent();
        }

        private void TriggerEvent()
        {
            if(_elaspedTimeAfterLastEvent <= _beatConfig.EventCallInterval)
            {
                return;
            }

            if (_highEnergy > _beatConfig.HighThreshold)
            {
                OnHighEnergy?.Invoke();
                _elaspedTimeAfterLastEvent = 0;
            }
            if (_midEnergy > _beatConfig.MidThreshold)
            {
                OnMidEnergy?.Invoke();
                _elaspedTimeAfterLastEvent = 0;
            }
            if (_lowEnergy > _beatConfig.LowThreshold)
            {
                OnLowEnergy?.Invoke();
                _elaspedTimeAfterLastEvent = 0;
            }

        }

        private float CalculateNormalizedEnergy(int startBand, int endBand)
        {
            float energy = 0f;

            for (int i = startBand; i < endBand; i++)
            {
                float bandEnergy = GetBandEnergy(i);

               _maxBandValues[i] = Mathf.Lerp(_maxBandValues[i], bandEnergy, _beatConfig.SmoothingFactor);

                _normalizedBandValues[i] = bandEnergy / Mathf.Max(_maxBandValues[i], 0.01f);

                energy += _normalizedBandValues[i];
            }

            return energy / (endBand - startBand);
        }

        private float GetBandEnergy(int bandIndex)
        {
            int startBin = bandIndex * (_beatConfig.SpectrumResolution / (_beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount));
            int endBin = (bandIndex + 1) * (_beatConfig.SpectrumResolution / (_beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount));
            float bandEnergy = 0f;

            for (int i = startBin; i < endBin; i++)
            {
                bandEnergy += _spectrumData[i];
            }

            return Mathf.Log10(1 + bandEnergy);
        }

        private void OnGUI()
        {

            if(!_enableBeatDebug)
                return;

            float barWidth = 200f;
            float barHeight = 20f;
            float spacing = 5f;
            float startX = 10f;
            float startY = 10f;

            // Draw a box for visualization
            GUI.Box(new Rect(startX - 10, startY - 10, barWidth + 40, (_normalizedBandValues.Length + 4) * (barHeight + spacing)), "Band Energies Visualization");

            // Display Low, Mid, High energy levels
            GUI.Label(new Rect(startX, startY, barWidth, barHeight), $"Low Energy: {_lowEnergy:F2}");
            GUI.HorizontalScrollbar(new Rect(startX, startY + barHeight, barWidth, barHeight), 0, _lowEnergy, 0, 1);
            startY += barHeight + spacing * 2;

            GUI.Label(new Rect(startX, startY, barWidth, barHeight), $"Mid Energy: {_midEnergy:F2}");
            GUI.HorizontalScrollbar(new Rect(startX, startY + barHeight, barWidth, barHeight), 0, _midEnergy, 0, 1);
            startY += barHeight + spacing * 2;

            GUI.Label(new Rect(startX, startY, barWidth, barHeight), $"High Energy: {_highEnergy:F2}");
            GUI.HorizontalScrollbar(new Rect(startX, startY + barHeight, barWidth, barHeight), 0, _highEnergy, 0, 1);
            startY += barHeight + spacing * 2;

            // Display each normalized band value with progress bars
            for (int i = 0; i < _normalizedBandValues.Length; i++)
            {
                GUI.Label(new Rect(startX, startY, barWidth, barHeight), $"Band {i}: {_normalizedBandValues[i]:F2}");
                GUI.HorizontalScrollbar(new Rect(startX, startY + barHeight, barWidth, barHeight), 0, _normalizedBandValues[i], 0, 1);
                startY += barHeight + spacing;
            }
        }

    }
}
