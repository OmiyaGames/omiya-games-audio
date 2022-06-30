# [Omiya Games](https://www.omiyagames.com/) - Audio

[![openupm](https://img.shields.io/npm/v/com.omiyagames.audio?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.omiyagames.audio/) [![Audio documentation](https://github.com/OmiyaGames/omiya-games-audio/workflows/Host%20DocFX%20Documentation/badge.svg)](https://omiyagames.github.io/omiya-games-audio/) [![Ko-fi Badge](https://img.shields.io/badge/donate-ko--fi-29abe0.svg?logo=ko-fi)](https://ko-fi.com/I3I51KS8F) [![License Badge](https://img.shields.io/github/license/OmiyaGames/omiya-games-audio)](/LICENSE.md)

**Audio** is an auditory tools package by [Omiya Games](https://www.omiyagames.com/) to provide a number of helper scripts and assets useful for game development.  As of this writing, this package provides the following tools:

## Audio Manager

The `AudioManager` is a script that interfaces with the project's Audio Mixer.  Its settings are visible in the Project Settings window, like so:

![Project Settings](https://omiyagames.github.io/omiya-games-audio/resources/AudioProjectSettings.png)

As a singleton class, audio manager allows the developer to adjust the volume and pitch for one of 5 potential audio groups in an Audio Mixer from nearly anywhere:
- **Main** - Adjusting the volume and pitch of this group will affect *all* audio.
- **Music** - Affects music playing both in the background, and within the game world (e.g. a radio.)
- **Voices** - Affects any spoken lines, grunts, and other human-like voices.
- **Ambience** - Affects any ambient sound effects, usually playing in the background.
- **Sound Effects** - Affects any other sound effects not covered by above groups.

For more details, check out the [dedicated manual page here](https://omiyagames.github.io/omiya-games-audio/manual/audioManager.html).

## Sound Effect

The `SoundEffect` script interfaces with an audio source to perform common tricks to create more varied sound effects.  It is designed to add features to Unity's built-in `AudioSource` component:

![Sound Effect Fields](https://omiyagames.github.io/omiya-games-audio/resources/SoundEffectFields.png)

Also, sound effect can be directly added into the hierarchy via the `Create...` menu, both in the hierarchy window, and right-click context menu.  This method has the added benefit of setting the audio source's output to the mixer group set in the Sound Effects settings under Audio Manager's Project Settings:

![Create Sound Effect](https://omiyagames.github.io/omiya-games-audio/resources/CreateSoundEffect.png)

For more details, check out the [dedicated manual page here](https://omiyagames.github.io/omiya-games-audio/manual/soundEffect.html).

## Looping Music

The `SingleLoopingMusic` asset allows defining a looping music with some extra goodies.  It's created in the Project dialog through the usual "Create" context menu:

![Create Looping Music context menu](https://omiyagames.github.io/omiya-games-audio/resources/CreateLoopingMusic.png)

The asset also allows adding an optional intro stinger that's played before the loop starts, and how long to delay the loop while the intro is playing (by default, set to how long the stinger is:)

![Looping Music Unity inspector](https://omiyagames.github.io/omiya-games-audio/resources/LoopingMusicInspector.png)

Note that `SingleLoopingMusic` is an instance of [`BackgroundAudio`](https://omiyagames.github.io/omiya-games-audio/api/OmiyaGames.Audio.BackgroundAudio.html).  To create other similar assets that can be played on [`AudioManager`](https://omiyagames.github.io/omiya-games-audio/manual/audioManager.html), consider extending [`BackgroundAudio`](https://omiyagames.github.io/omiya-games-audio/api/OmiyaGames.Audio.BackgroundAudio.html) and [`BackgroundAudio.Player`](https://omiyagames.github.io/omiya-games-audio/api/OmiyaGames.Audio.BackgroundAudio.Player.html).

For more details, check out the [dedicated manual page here](https://omiyagames.github.io/omiya-games-audio/manual/loopingMusic.html).

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
12. (Optional) In this sample, there are a lot of assets under the `Savers` folder.  These files contains the default volume and mute settings for each audio category.  It's recommended to edit these files' default values to your liking.
    
    ![Saver files](https://omiyagames.github.io/omiya-games-audio/resources/SaverFiles.png)
13. Select `File -> Save Project` to save all the above settings.
    
    ![Save Project](https://omiyagames.github.io/omiya-games-audio/resources/SaveProject.png)

## Resources

- [Documentation](https://omiyagames.github.io/omiya-games-audio/)
- [Change Log](/CHANGELOG.md)

## LICENSE

Overall package is licensed under [MIT](/LICENSE.md), unless otherwise noted in the [3rd party licenses](/THIRD%20PARTY%20NOTICES.md) file and/or source code.

Copyright (c) 2019-2022 Omiya Games
