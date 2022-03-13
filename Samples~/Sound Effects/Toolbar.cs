using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OmiyaGames.Managers;
using OmiyaGames.Audio;

public class Toolbar : MonoBehaviour
{
	[SerializeField]
	SoundEffect sfx;

	[Header("Mutate")]
	[SerializeField]
	Toggle pitch;
	[SerializeField]
	Toggle volume;

	[Header("Other")]
	[SerializeField]
	TMP_Dropdown layerDropDown;
	[SerializeField]
	Toggle pauseOnTimeStop;

	[Header("Time")]
	[SerializeField]
	Slider timeScaleSlider;
	[SerializeField]
	TextMeshProUGUI timeScaleLabel;
	[SerializeField]
	Toggle pauseToggle;

	IEnumerator Start()
	{
		// Setup the audio
		yield return AudioManager.Setup();

		SetupMutateToggles();
		SetupOtherSfxControls();
		SetupTimeControls();
	}

	void SetupMutateToggles()
	{
		pitch.isOn = sfx.IsMutatingPitch;
		volume.isOn = sfx.IsMutatingVolume;

		pitch.onValueChanged.AddListener((isOn) =>
		{
			sfx.IsMutatingPitch = isOn;
		});
		volume.onValueChanged.AddListener((isOn) =>
		{
			sfx.IsMutatingVolume = isOn;
		});
	}

	void SetupOtherSfxControls()
	{
		layerDropDown.value = sfx.NumberOfLayers - 1;
		pauseOnTimeStop.isOn = sfx.IsPausedOnTimeStop;

		layerDropDown.onValueChanged.AddListener((index) =>
		{
			sfx.NumberOfLayers = index + 1;
		});
		pauseOnTimeStop.onValueChanged.AddListener((isOn) =>
		{
			sfx.IsPausedOnTimeStop = isOn;
		});
	}

	void SetupTimeControls()
	{
		// Setup the volume slider
		timeScaleSlider.value = TimeManager.TimeScale;
		UpdateLabels();
		timeScaleSlider.onValueChanged.AddListener((value) =>
		{
			TimeManager.TimeScale = value;
			UpdateLabels();
		});

		// Setup the mute toggle
		pauseToggle.isOn = TimeManager.IsManuallyPaused;
		pauseToggle.onValueChanged.AddListener((isOn) =>
		{
			TimeManager.IsManuallyPaused = isOn;
			UpdateLabels();
		});

		void UpdateLabels()
		{
			float timeScale = TimeManager.TimeScale;
			if (TimeManager.IsManuallyPaused)
			{
				timeScale = 0f;
			}
			timeScaleLabel.text = $"x{timeScale:0.0}";
		}
	}
}
