using UnityEngine;
using OmiyaGames.Audio;

[RequireComponent(typeof(AudioSource))]
public class PlayAudioControl : MonoBehaviour
{
	[SerializeField]
	AudioSource audioSource;
	[SerializeField]
	UnityEngine.UI.Toggle playToggle;

	public void Setup(AudioLayer.SubLayer layer)
	{
		// Setup audio
		audioSource.ignoreListenerPause = true;
		audioSource.outputAudioMixerGroup = layer.DefaultGroup;

		// Setup toggle
		playToggle.isOn = audioSource.isPlaying;
		playToggle.onValueChanged.AddListener((isOn) =>
		{
			if (isOn)
			{
				audioSource.Play();
			}
			else
			{
				audioSource.Stop();
			}
		});
	}

	// Update is called once per frame
	void Reset()
	{
		audioSource = GetComponent<AudioSource>();
	}
}
