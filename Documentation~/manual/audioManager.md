# Audio Manager

The `AudioManager` is a script that interfaces with the project's Audio Mixer.  Its settings are visible in the Project Settings window, like so:

![Project Settings](https://omiyagames.github.io/omiya-games-audio/resources/AudioProjectSettings.png)

As a singleton class, audio manager allows the developer to adjust the volume and pitch for one of 5 potential audio groups in an Audio Mixer from nearly anywhere:
- **Main** - Adjusting the volume and pitch of this group will affect *all* audio.
- **Music** - Affects music playing both in the background, and within the game world (e.g. a radio.)
- **Voices** - Affects any spoken lines, grunts, and other human-like voices.
- **Ambience** - Affects any ambient sound effects, usually playing in the background.
- **Sound Effects** - Affects any other sound effects not covered by above groups.

An example code will look something like below:
```csharp
using System.Collections;
using UnityEngine;
using OmiyaGames.Audio;

public class VolumeExample : MonoBehaviour
{
    // This tool adds a Sound Effect script
    [SerializeField]
    SoundEffect testSound;

    IEnumerator Start()
    {
        // IMPORTANT! Setting up audio manager is required for adjusting volume and pitch control.
        // This only needs to be called once throughout the entire game.
        // It is also safe, though not recommended, to call this function multiple times.
        yield return AudioManager.Setup();

        // Adjust the volume like so, between 0f and 1f.
        // Note that this value *does* get saved in PlayerPrefs.
        // This means the next time user loads the game, and this script calls AudioManager.Setup(),
        // the VolumePercent will be updated to the value it was set to last time the game was open.
        AudioManager.Main.VolumePercent = 0.5f;

        // Playing sound is fairly simple
        testSound.Play();
        yield return new WaitForSeconds(2f);

        // Adjusting for a different audio grou.
        // These values are also saved in PlayerPrefs.
        AudioManager.SoundEffects.VolumePercent = 0.5f;
        testSound.Play();
    }
}
```

### Time-Related Audio Distortions
Audio manager also supports pitch-shifting and distortion effects in response to changes made to `TimeManager`.  These effects utilizes the mixer's snapshots:

![Time-Related Effects Settings](https://omiyagames.github.io/omiya-games-audio/resources/TimeRelatedEffectsSettings.png)

A script utilizing these effects will look something like:
```csharp
using System.Collections;
using UnityEngine;
using OmiyaGames.Audio;
using OmiyaGames.Managers;

public class TimeEffectsExample : MonoBehaviour
{
    [SerializeField]
    SoundEffect testSound;

    IEnumerator Start()
    {
        // IMPORTANT! Setting up audio manager is also required for time-related audio effects.
        yield return AudioManager.Setup();

        // Pause the game to trigger the pause snapshots.
        TimeManager.IsManuallyPaused = true;

        testSound.Play();
        yield return new WaitForSecondsRealtime(2f);

        TimeManager.IsManuallyPaused = false;

        // Changing timescale also changes the audio
        TimeManager.TimeScale = 0.5f;
        testSound.Play();

        yield return new WaitForSecondsRealtime(2f);

        TimeManager.TimeScale = 1.5f;
        testSound.Play();
    }
}
```
