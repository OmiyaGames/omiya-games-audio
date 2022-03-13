using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OmiyaGames.Audio;

public class AudioLayerControls : MonoBehaviour
{
	[SerializeField]
	Slider volumeSlider;
	[SerializeField]
	Toggle muteToggle;
	[SerializeField]
	TextMeshProUGUI percentLabel;
	[SerializeField]
	TextMeshProUGUI decibleLabel;

	public void Setup(AudioLayer layer)
	{
		// Setup the volume slider
		volumeSlider.value = layer.VolumePercent;
		UpdateLabels(layer);
		volumeSlider.onValueChanged.AddListener((value) =>
		{
			layer.VolumePercent = value;
			UpdateLabels(layer);
		});

		// Setup the mute toggle
		muteToggle.isOn = layer.IsMuted;
		muteToggle.onValueChanged.AddListener((isOn) =>
		{
			layer.IsMuted = isOn;
			UpdateLabels(layer);
		});
	}

	void UpdateLabels(AudioLayer layer)
	{
		percentLabel.text = layer.VolumePercent.ToString("0%");
		decibleLabel.text = layer.VolumeDb.ToString("0.0 dB");
	}
}
