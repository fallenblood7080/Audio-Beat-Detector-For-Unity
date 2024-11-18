using System;
using UnityEngine;

namespace Beat
{
    /// <summary>
    /// Manages beat detection from audio input, analyzing frequency bands and triggering energy events.
    /// </summary>
    public class BeatManager : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float[] _spectrumData;
        private float[] _normalizedBandValues;
        private float[] _maxBandValues;

        [SerializeField] private BeatConfigSO _beatConfig;
        [SerializeField] private bool _enableDebugVisualization = false;

        private float _lowEnergy;
        private float _midEnergy;
        private float _highEnergy;

        private float _elapsedTimeAfterLastEvent;
        private bool _isInitialized;

        #region Beat Actions

        /// <summary>
        /// Action triggered when the energy level of the low-frequency band exceeds the configured threshold.
        /// </summary>
        /// <remarks>
        /// Low energy corresponds to the bass and sub-bass frequencies in the audio spectrum.
        /// This is typically associated with beats or deep sound effects in music.
        /// </remarks>
        public Action OnLowEnergy;

        /// <summary>
        /// Action triggered when the energy level of the mid-frequency band exceeds the configured threshold.
        /// </summary>
        /// <remarks>
        /// Mid energy corresponds to the midrange frequencies in the audio spectrum, including vocals, instruments, and melody.
        /// This band is crucial for the clarity and richness of the audio.
        /// </remarks>
        public Action OnMidEnergy;

        /// <summary>
        /// Action triggered when the energy level of the high-frequency band exceeds the configured threshold.
        /// </summary>
        /// <remarks>
        /// High energy corresponds to the treble and high-pitched frequencies in the audio spectrum, such as cymbals and high vocals.
        /// These frequencies add sharpness and detail to the sound.
        /// </remarks>
        public Action OnHighEnergy;

        #endregion

        private void Awake()
        {
            // Ensure AudioSource component exists
            if (!TryGetComponent(out _audioSource))
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Sets up the beat detection system with the provided audio clip.
        /// </summary>
        /// <param name="clip">Audio clip to analyze.</param>
        public void SetupBeat(AudioClip clip)
        {
            _isInitialized = false;

            if (_beatConfig == null)
            {
                Debug.LogError("Beat Config file is missing in Beat Manager.", this);
                return;
            }

            _audioSource.clip = clip;

            if (_beatConfig.MixerGroup != null)
            {
                _audioSource.outputAudioMixerGroup = _beatConfig.MixerGroup;
            }

            int totalBands = _beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount;
            _spectrumData = new float[_beatConfig.SpectrumResolution];
            _normalizedBandValues = new float[totalBands];
            _maxBandValues = new float[totalBands];

            _audioSource.Play();
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            _elapsedTimeAfterLastEvent += Time.deltaTime;

            _audioSource.GetSpectrumData(_spectrumData, 0, _beatConfig.FftWindow);

            _lowEnergy = CalculateNormalizedEnergy(0, _beatConfig.LowBandCount);
            _midEnergy = CalculateNormalizedEnergy(_beatConfig.LowBandCount, _beatConfig.LowBandCount + _beatConfig.MidBandCount);
            _highEnergy = CalculateNormalizedEnergy(_beatConfig.LowBandCount + _beatConfig.MidBandCount,
                                                     _beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount);

            TriggerEvent();
        }

        /// <summary>
        /// Triggers events based on energy thresholds and configuration.
        /// </summary>
        private void TriggerEvent()
        {
            if (_elapsedTimeAfterLastEvent <= _beatConfig.EventCallInterval) return;

            if (_beatConfig.UseDominantEnergyOnly)
            {
                float dominantEnergy = Mathf.Max(_lowEnergy, _midEnergy, _highEnergy);

                if (dominantEnergy > _beatConfig.LowThreshold || dominantEnergy > _beatConfig.MidThreshold || dominantEnergy > _beatConfig.HighThreshold)
                {
                    if (dominantEnergy == _highEnergy && _highEnergy > _beatConfig.HighThreshold)
                    {
                        OnHighEnergy?.Invoke();
                    }
                    else if (dominantEnergy == _midEnergy && _midEnergy > _beatConfig.MidThreshold)
                    {
                        OnMidEnergy?.Invoke();
                    }
                    else if (dominantEnergy == _lowEnergy && _lowEnergy > _beatConfig.LowThreshold)
                    {
                        OnLowEnergy?.Invoke();
                    }

                    _elapsedTimeAfterLastEvent = 0;
                }
            }
            else
            {
                if (_highEnergy > _beatConfig.HighThreshold) { OnHighEnergy?.Invoke(); }
                if (_midEnergy > _beatConfig.MidThreshold) { OnMidEnergy?.Invoke(); }
                if (_lowEnergy > _beatConfig.LowThreshold) { OnLowEnergy?.Invoke(); }

                _elapsedTimeAfterLastEvent = 0;
            }
        }

        /// <summary>
        /// Calculates the normalized energy for a specified band range.
        /// </summary>
        /// <param name="startBand">Start index of the band range.</param>
        /// <param name="endBand">End index of the band range.</param>
        /// <returns>Normalized energy value for the band range.</returns>
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

        /// <summary>
        /// Calculates the energy for a specific band.
        /// </summary>
        /// <param name="bandIndex">Index of the band to analyze.</param>
        /// <returns>Log-scaled energy value for the band.</returns>
        private float GetBandEnergy(int bandIndex)
        {
            int totalBands = _beatConfig.LowBandCount + _beatConfig.MidBandCount + _beatConfig.HighBandCount;
            int startBin = bandIndex * (_beatConfig.SpectrumResolution / totalBands);
            int endBin = (bandIndex + 1) * (_beatConfig.SpectrumResolution / totalBands);

            float bandEnergy = 0f;
            for (int i = startBin; i < endBin; i++)
            {
                bandEnergy += _spectrumData[i];
            }

            return Mathf.Log10(1 + bandEnergy);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!_isInitialized || !_enableDebugVisualization) return;

            float barWidth = 200f;
            float barHeight = 20f;
            float spacing = 5f;
            float startX = 10f;
            float startY = 10f;

            GUI.Box(new Rect(startX - 10, startY - 10, barWidth + 40, (_normalizedBandValues.Length + 4) * (barHeight + spacing)), "Band Energies Visualization");

            DisplayEnergyBar(ref startY, "Low Energy", _lowEnergy, barWidth, barHeight, startX, spacing);
            DisplayEnergyBar(ref startY, "Mid Energy", _midEnergy, barWidth, barHeight, startX, spacing);
            DisplayEnergyBar(ref startY, "High Energy", _highEnergy, barWidth, barHeight, startX, spacing);

            for (int i = 0; i < _normalizedBandValues.Length; i++)
            {
                DisplayEnergyBar(ref startY, $"Band {i}", _normalizedBandValues[i], barWidth, barHeight, startX, spacing);
            }
        }
#endif

        /// <summary>
        /// Displays an energy bar in the debug visualization.
        /// </summary>
        private void DisplayEnergyBar(ref float startY, string label, float energy, float barWidth, float barHeight, float startX, float spacing)
        {
            GUI.Label(new Rect(startX, startY, barWidth, barHeight), $"{label}: {energy:F2}");
            GUI.HorizontalScrollbar(new Rect(startX, startY + barHeight, barWidth, barHeight), 0, energy, 0, 1);
            startY += barHeight + spacing;
        }

    }
}
