
# AudioBeatDetector for Unity

**AudioBeatDetector** is a Unity package that allows you to detect beats in an audio track by analyzing frequency bands and triggering events based on energy levels. It is ideal for creating rhythm-based games, music visualization, or any project that requires beat detection.

## Installation

You can install **AudioBeatDetector** directly into your Unity project using the Git URL.

1. Open your Unity project.
2. Go to **Window > Package Manager**.
3. In the Package Manager, click on the **+** button in the top left corner and select **"Add package from Git URL..."**.
4. Enter the following Git URL:

   ```
   https://github.com/fallenblood7080/Audio-Beat-Detector-For-Unity.git
   ```

5. Click **Add**, and Unity will automatically download and import the package into your project.

## Usage

### 1. **Setup BeatManager**

The `BeatManager` class manages the beat detection process, which uses frequency spectrum data to trigger events when specific energy levels exceed the configured thresholds.

#### a) **Add the BeatManager Component**
1. Attach the `BeatManager` script to a GameObject in your scene (e.g., an empty GameObject).
2. Assign an `AudioClip` to be analyzed in the **BeatManager** component.

#### b) **Configure Beat Detection**
The `BeatManager` uses a `BeatConfigSO` ScriptableObject to configure frequency bands, thresholds, smoothing factors, and event call intervals.

1. **Create a BeatConfigSO**:
   - Right-click in the **Assets** window > **Create** > **Scriptable Objects** > **BeatConfigSO**.
   - Configure the values for low, mid, and high-frequency bands, thresholds, and event intervals.

2. **Assign BeatConfigSO**:
   - Drag the `BeatConfigSO` file into the **BeatManager** component in the Unity Inspector.
3. **Adding Audio Source**(Optional):
	- Add the Audio Source Component to Beat Manager, it's the optional step, if you don't**BeatManager* will automatically add the Audio Source.
4. **Audio Clip**:
	- To assign audio Clip, call the `SetupBeat(clip)`.
	```csharp
	beatManager.SetupBeat(audioClip);
	```

5. **Debug Visualization** (optional):
   - Enable **Enable Debug Visualization** in the BeatManager to visualize the energy levels for each frequency band.

### 2. **Event Listeners**
You can attach actions to be triggered when the energy levels of specific frequency bands exceed a threshold.

- **OnLowEnergy**: Triggered when low-frequency energy exceeds the low threshold.
- **OnMidEnergy**: Triggered when mid-frequency energy exceeds the mid threshold.
- **OnHighEnergy**: Triggered when high-frequency energy exceeds the high threshold.

For example:
```csharp
BeatManager beatManager = GetComponent<BeatManager>();
beatManager.SetupClip(yourClip);
beatManager.OnLowEnergy += HandleLowEnergy;
beatManager.OnMidEnergy += HandleMidEnergy;
beatManager.OnHighEnergy += HandleHighEnergy;

private void HandleLowEnergy() { Debug.Log("Low Energy Detected!"); }
private void HandleMidEnergy() { Debug.Log("Mid Energy Detected!"); }
private void HandleHighEnergy() { Debug.Log("High Energy Detected!"); }
```

### 3. **BeatConfigSO Settings**

The `BeatConfigSO` ScriptableObject allows you to fine-tune the beat detection behavior:

- **SmoothingFactor**: The factor used to smooth the energy values for each frequency band.
- **SpectrumResolution**: The resolution for the frequency spectrum. Higher values provide more granular frequency analysis.
- **MixerGroup** (Optional): - If you want to use a specific `AudioMixerGroup` to control audio routing, you can assign the `MixerGroup` in the BeatManager component. This is optional and is typically used if you want to control the volume or effects of the audio dynamically.
- **Low/Mid/High Band Count**: Defines how many frequency bands are used for each energy level.
- **Low/Mid/High Thresholds**: Thresholds to trigger the corresponding events when energy exceeds the set values.
- **Event Call Interval**: Defines how often events are triggered after the previous event.
- **FFT Window Type**: The windowing function used for the Fast Fourier Transform (FFT), which influences frequency analysis.
- **UseDominantEnergyOnly**: If enabled, only the highest energy band is considered for triggering events.

### 4. **Debugging (Optional)**

For development and testing, you can visualize the energy levels for each frequency band using Unity’s **OnGUI()** method. This is only visible in the editor or during development builds and can be toggled by enabling **Enable Debug Visualization** in the BeatManager component.

---

## Example Usage

Here is a simple example demonstrating how to set up and trigger events based on energy levels:

```csharp
using UnityEngine;
using Beat;

public class AudioBeatExample : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private BeatManager beatManager;

    private void Start()
    {
        beatManager.SetupBeat(audioClip);

        beatManager.OnLowEnergy += OnLowEnergy;
        beatManager.OnMidEnergy += OnMidEnergy;
        beatManager.OnHighEnergy += OnHighEnergy;
    }

    private void OnLowEnergy()
    {
        Debug.Log("Low Energy Triggered");
    }

    private void OnMidEnergy()
    {
        Debug.Log("Mid Energy Triggered");
    }

    private void OnHighEnergy()
    {
        Debug.Log("High Energy Triggered");
    }
}
```

In this example, when the energy levels for low, mid, or high frequencies exceed the configured thresholds, the corresponding event handler is called, and a log is generated.

## License

This package is open-source and available under the MIT License. Feel free to modify and use it in your projects.

## Contributing

If you find a bug or have a feature request, please open an issue on the [GitHub repository](https://github.com/fallenblood7080/Audio-Beat-Detector-For-Unity).

---

**AudioBeatDetector** provides an easy and efficient way to analyze audio data and trigger events based on frequency bands, making it a powerful tool for rhythm-based games and audio-reactive applications.