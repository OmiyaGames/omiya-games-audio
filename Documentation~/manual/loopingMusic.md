# Looping Music

The `SingleLoopingMusic` asset allows defining a looping music with some extra goodies.  It's created in the Project dialog through the usual "Create" context menu:

![Create Looping Music context menu](https://omiyagames.github.io/omiya-games-audio/resources/CreateLoopingMusic.png)

The asset also allows adding an optional intro stinger that's played before the loop starts, and how long to delay the loop while the intro is playing (by default, set to how long the stinger is:)

![Looping Music Unity inspector](https://omiyagames.github.io/omiya-games-audio/resources/LoopingMusicInspector.png)

For starters, helper scripts `ChangeAudioOnStart.cs` and `ChangeAudioOnTrigger.cs` has been added to this package.  The former allows one to change the background music and/or ambience when the scene first loads in; the latter does the same thing when an object with a specific tag (usually the "Player") enters a trigger collider.  Both scripts support loading Addressables as well as project files directly.

For a more custom behavior, one can play the asset on the `Music` or `Ambience` layer in the [`AudioManager`](https://omiyagames.github.io/omiya-games-audio/manual/audioManager.html):
```csharp
using System.Collections;
using UnityEngine;
using OmiyaGames.Audio;

public class MusicExample : MonoBehaviour
{
    [SerializeField]
    SingleLoopingMusic testMusic;

    IEnumerator Start()
    {
        // Setting up audio manager so the music will play at the right volume
        yield return AudioManager.Setup();

        // Play the music on music mixer layer
        yield return StartCoroutine AudioManager.Music.PlayNextCoroutine(testMusic,
            // The optional FadeInArgs can be added to fade this music in, and the prior music out. 
            new FadeInArgs()
            {
                // Setting the fade duration for half-a-second
                DurationSeconds = 0.5f
            }
        );
    }
}
```

Note that `SingleLoopingMusic` is an instance of [`BackgroundAudio`](https://omiyagames.github.io/omiya-games-audio/api/OmiyaGames.Audio.BackgroundAudio.html).  To create other similar assets that can be played on [`AudioManager`](https://omiyagames.github.io/omiya-games-audio/manual/audioManager.html), consider extending [`BackgroundAudio`](https://omiyagames.github.io/omiya-games-audio/api/OmiyaGames.Audio.BackgroundAudio.html) and [`BackgroundAudio.Player`](https://omiyagames.github.io/omiya-games-audio/api/OmiyaGames.Audio.BackgroundAudio.Player.html).