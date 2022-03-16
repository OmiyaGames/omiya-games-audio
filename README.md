# [Omiya Games](https://www.omiyagames.com/) - Audio

[![openupm](https://img.shields.io/npm/v/com.omiyagames.audio?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.omiyagames.audio/) [![Audio documentation](https://github.com/OmiyaGames/omiya-games-audio/workflows/Host%20DocFX%20Documentation/badge.svg)](https://omiyagames.github.io/omiya-games-audio/) [![Ko-fi Badge](https://img.shields.io/badge/donate-ko--fi-29abe0.svg?logo=ko-fi)](https://ko-fi.com/I3I51KS8F) [![License Badge](https://img.shields.io/github/license/OmiyaGames/omiya-games-audio)](/LICENSE.md)

**Audio** is an experimental tools package by [Omiya Games](https://www.omiyagames.com/), to eventually provide a number of audio-related tools useful for game development.  As of this writing, this package provides the following tools:

## Audio Manager

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

## Sound Effect

The `SoundEffect` script interfaces with an audio source to perform common tricks to create more varied sound effects.  It is designed to add features to Unity's built-in `AudioSource` component:

![Sound Effect Fields](https://omiyagames.github.io/omiya-games-audio/resources/SoundEffectFields.png)

Note that an audio clip doesn't have to be added into the audio source for `SoundEffect` to work: the script will automatically choose a random clip from the clip variations list and set the audio source's clip.  That said, if a clip has been added to the attached audio source, that will be added to the clip variations list on script awake, with a default `Frequency` of one.  Lastly, most adjustments made to the attached audio source -- besides volume and pitch, if `Mutate Volume` and `Mutate Pitch` fields are checked, respectively -- *will* affect the sound played by `SoundEffect`, including all the overlapping sound effects generated by this script.

Using the sound effect script in code is pretty simple:
```csharp
using System.Collections;
using UnityEngine;
using OmiyaGames.Audio;

public class TimeEffectsExample : MonoBehaviour
{
    [SerializeField]
    SoundEffect testSound;

    IEnumerator Start()
    {
        // Setting up audio manager so the sound effect will play at the right volume
        yield return AudioManager.Setup();

        // Play a random clip in the clip variations list
        testSound.Play();
    }
}
```

Also, sound effect can be directly added into the hierarchy via the `Create...` menu, both in the hierarchy window, and right-click context menu.  This method has the added benefit of setting the audio source's output to the mixer group set in the Sound Effects settings under Audio Manager's Project Settings:

![Create Sound Effect](https://omiyagames.github.io/omiya-games-audio/resources/CreateSoundEffect.png)

## Install

This (we swear, one-time!) setup is a bit of a doozy.

1. First, install the package via [OpenUPM's command line tool](https://openupm.com/), which handles installing this package and its many, many dependencies:
    1. If you haven't already [installed OpenUPM](https://openupm.com/docs/getting-started.html#installing-openupm-cli), you can do so through Node.js's `npm` (obviously have Node.js installed in your system first):
        ```
        npm install -g openupm-cli
        ```
    2. Then, to install this package, just run the following command at the root of your Unity project:
        ```
        openupm add com.omiyagames.audio
        ```
2. Open Unity.
3. One of this package's dependency is Unity's `Addressables`, which needs setup:
    1. Select `Window -> Asset Managerment -> Addressables -> Groups` from the file menu bar.
        
        ![Addressables Groups context menu](https://omiyagames.github.io/omiya-games-audio/resources/AddressablesGroupsContextMenu.png)
    2. A pop-up with a single button will appear. Click `Create Addressables Settings`.
        
        ![Addressables Groups pop-up](https://omiyagames.github.io/omiya-games-audio/resources/AddressablesGroupsPopUp.png)
    3. After some new assets are created in the project, close the pop-up window. Addressables are now setup.
4. Another dependency that needs setup is Omiya Games' `Saves` package:
    1. Select `Edit -> Project Settings...` from the file menu bar.
        
        ![Project Settings context menu](https://omiyagames.github.io/omiya-games-audio/resources/ProjectSettingsContextMenu.png)
    2. On the left sidebar, select `Omiya Games -> Saves`.
        
        ![Saves project settings](https://omiyagames.github.io/omiya-games-audio/resources/SavesProjectSettings.png)
    3. Click on `Create...`, and save the new package settings file to any location within the project's `Assets` folder
        
        ![Save file pop-up](https://omiyagames.github.io/omiya-games-audio/resources/SaveFilePopUp.png)
    3. Saves are now setup.
5. Select `Window -> Package Manager...` from the file menu bar to open the package manager dock.
    
    ![Package Manager context menu](https://omiyagames.github.io/omiya-games-audio/resources/PackageManagerContextMenu.png)
6. Import this package's `Custom Settings` sample.
    
    ![Package Manager menu](https://omiyagames.github.io/omiya-games-audio/resources/PackageManagerMenu.png)
7. Move all the imported files to a folder more accessible location.  You will likely be editing these files during development.
8. Select `Edit -> Project Settings...` from the file menu bar to open project settings dock again.
9. On the left sidebar, select `Omiya Games -> Audio`.
10. Drag-and-drop the imported settings file, `Audio Settings - Custom`, into the `Active Settings` field.
    
    ![Audio project settings - Empty](https://omiyagames.github.io/omiya-games-audio/resources/AudioProjectSettingsEmpty.png)
11. With the window content drastically changed, scroll to the bottom of the settings window, and click the `Add Savers To Saves Settings` button.
    
    ![Audio project settings - Filled](https://omiyagames.github.io/omiya-games-audio/resources/AudioProjectSettingsFilled.png)
12. (Optional) In this sample, there are a lot of assets starting with the phrase, "`Savers - `".  These files contains the default volume and mute settings for each audio category.  It's recommended to edit these files' default values to your liking.
    
    ![Saver files](https://omiyagames.github.io/omiya-games-audio/resources/SaverFiles.png)
13. Select `File -> Save Project` to save all the above settings.
    
    ![Save Project](https://omiyagames.github.io/omiya-games-audio/resources/SaveProject.png)

## Resources

- [Documentation](https://omiyagames.github.io/omiya-games-audio/)
- [Change Log](/CHANGELOG.md)

## LICENSE

Overall package is licensed under [MIT](/LICENSE.md), unless otherwise noted in the [3rd party licenses](/THIRD%20PARTY%20NOTICES.md) file and/or source code.

Copyright (c) 2019-2022 Omiya Games
