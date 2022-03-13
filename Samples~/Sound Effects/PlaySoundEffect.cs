using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using OmiyaGames.Audio;

public class PlaySoundEffect : MonoBehaviour
{
	[SerializeField]
	EventTrigger clickArea;
	[SerializeField]
	SoundEffect testSoundEffect;

	// Start is called before the first frame update
	void Start()
	{
		SetupClickToPlaySoundEffectBehavior();
	}

	void SetupClickToPlaySoundEffectBehavior()
	{
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener((e) =>
		{
			if (e is PointerEventData)
			{
				// Position the sound effect
				testSoundEffect.transform.position = ((PointerEventData)e).pointerCurrentRaycast.worldPosition;
			}

			// Play the sound effect
			testSoundEffect.Play();
		});
		clickArea.triggers.Add(entry);
	}
}
