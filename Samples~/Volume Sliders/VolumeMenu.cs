using System.Collections;
using UnityEngine;
using OmiyaGames.Audio;

public class VolumeMenu : MonoBehaviour
{
	[SerializeField]
	AudioLayerControls mainControls;

	[Header("Music")]
	[SerializeField]
	PlayAudioControl playMusic;
	[SerializeField]
	AudioLayerControls musicControls;

	[Header("Sound Effects")]
	[SerializeField]
	PlayAudioControl playSoundEffects;
	[SerializeField]
	AudioLayerControls soundEffectsControls;

	[Header("Voices")]
	[SerializeField]
	PlayAudioControl playVoices;
	[SerializeField]
	AudioLayerControls voicesControls;

	[Header("Ambience")]
	[SerializeField]
	PlayAudioControl playAmbience;
	[SerializeField]
	AudioLayerControls ambienceControls;

	[Header("Effects")]
	[SerializeField]
	EffectsControls effectsControls;

	// Start is called before the first frame update
	IEnumerator Start()
	{
		// Setup audio manager
		yield return AudioManager.Setup();

		// Setup all controls
		mainControls.Setup(AudioManager.Main);

		playMusic.Setup(AudioManager.Music);
		musicControls.Setup(AudioManager.Music);

		playSoundEffects.Setup(AudioManager.SoundEffects);
		soundEffectsControls.Setup(AudioManager.SoundEffects);

		playVoices.Setup(AudioManager.Voices);
		voicesControls.Setup(AudioManager.Voices);

		playAmbience.Setup(AudioManager.Ambience);
		ambienceControls.Setup(AudioManager.Ambience);

		effectsControls.Setup();
	}
}
