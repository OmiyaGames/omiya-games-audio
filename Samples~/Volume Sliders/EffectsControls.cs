using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OmiyaGames.Managers;

public class EffectsControls : MonoBehaviour
{
	[SerializeField]
	Slider timeScaleSlider;
	[SerializeField]
	Toggle pauseToggle;
	[SerializeField]
	TextMeshProUGUI timeScaleLabel;

	public void Setup()
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
	}

	void UpdateLabels()
	{
		float timeScale = TimeManager.TimeScale;
		if(TimeManager.IsManuallyPaused)
		{
			timeScale = 0f;
		}
		timeScaleLabel.text = $"x{timeScale:0.00}";
	}
}
