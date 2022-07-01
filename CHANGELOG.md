# Change Log:

## 1.1.0

- Enhancing [`BackgroundAudio`](/Runtime/BackgroundAudio/BackgroundAudio.cs) - Adding property `BackgroundAudio.Player.IsPausedOnTimeStop` so player can auto-pause when `Time.timeScale` is zero.  Ironically, this was already the default behavior; this property adds the option to disable it.

## 1.0.0

- Added [`SingleLoopingMusic`](/Runtime/BackgroundAudio/SingleLoopingMusic.cs) - `ScriptableObject` that generates a `MonoBehaviour` script (called [`BackgroundAudio.Player`](/Runtime/BackgroundAudio/BackgroundAudio.cs)), playing an optional intro stinger first, before playing the looping main music.
- Added abstract [`BackgroundAudio`](/Runtime/BackgroundAudio/BackgroundAudio.cs) so one can create more `ScriptableObject` like [`SingleLoopingMusic`](/Runtime/BackgroundAudio/SingleLoopingMusic.cs).
- Enhancing [`AudioLayer.Background`](/Runtime/AudioLayers/AudioLayer.Background.cs) - Adding new method, `PlayNextCoroutine()` to play a new [`BackgroundAudio`](/Runtime/BackgroundAudio/BackgroundAudio.cs).  Adding property `History` that tracks what music has been played.  Adding helper methods, `PlayPreviousCoroutine`, `FadeOutCurrnetPlaying`, and `FadeInCurrentPlayingCoroutine` to play music from `History`.

## 0.1.0-exp.1

- Initial release:
    - Added [`AudioManager`](/Runtime/AudioManager.cs) - singleton script that handles adjusting volumes and pitch per audio layer, apply audio effects when `TimeManager` changes, etc.
    - Added [`SoundEffect`](/Runtime/SoundEffect.cs) - `MonoBehaviour` script that handles common audio polishes, such as mutating pitch and volume, before playing a sound effect on the attached `AudioSource`.
