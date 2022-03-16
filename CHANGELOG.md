# Change Log:

## 0.1.0-exp.1

- Initial release:
    - Added [`AudioManager`](/Runtime/AudioManager.cs) - singleton script that handles adjusting volumes and pitch per audio layer, apply audio effects when `TimeManager` changes, etc.
    - Added [`AudioManager`](/Runtime/SoundEffect.cs) - `MonoBehaviour` script that handles common audio polishes, such as mutating pitch and volume, before playing a sound effect on the attached `AudioSource`.
