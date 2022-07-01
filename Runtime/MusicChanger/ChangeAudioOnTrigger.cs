using System.Collections;
using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="ChangeAudioOnTrigger.cs" company="Omiya Games">
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2022 Omiya Games
	/// 
	/// Permission is hereby granted, free of charge, to any person obtaining a copy
	/// of this software and associated documentation files (the "Software"), to deal
	/// in the Software without restriction, including without limitation the rights
	/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	/// copies of the Software, and to permit persons to whom the Software is
	/// furnished to do so, subject to the following conditions:
	/// 
	/// The above copyright notice and this permission notice shall be included in
	/// all copies or substantial portions of the Software.
	/// 
	/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	/// THE SOFTWARE.
	/// </copyright>
	/// <list type="table">
	/// <listheader>
	/// <term>Revision</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>
	/// <strong>Version:</strong> 1.0.0<br/>
	/// <strong>Date:</strong> 4/14/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Changes the background music on collision trigger enter.
	/// </summary>
	public class ChangeAudioOnTrigger : MonoBehaviour
	{
		public event System.Action<ChangeAudioOnStart> OnBeforeAudioChange;
		public event System.Action<ChangeAudioOnStart> OnAfterAudioChange;

		[SerializeField]
		PlaybackBehavior musicBehavior = new(PlaybackBehavior.FadeBehavior.DoNothing);
		[SerializeField]
		PlaybackBehavior ambienceBehavior = new(PlaybackBehavior.FadeBehavior.DoNothing);

		[Header("Trigger Behavior")]
		[SerializeField]
		[Tooltip("Only colliders with this tag will trigger audio changes")]
		string colliderTag = "Player";
		[SerializeField]
		[Tooltip("If checked, reverts the music to the one last played")]
		bool revertOnExit = true;

		int numCollidersEntered = 0;
		Coroutine lastCoroutine = null;
		bool isMusicAdded = false, isAmbienceAdded = false;

		void OnTriggerEnter(Collider other)
		{
			// Check if this is the object we want to detect
			if (other.CompareTag(colliderTag))
			{
				// Check if other colliders are already in this trigger
				++numCollidersEntered;
				if (numCollidersEntered > 1)
				{
					// If so, don't bother playing music
					return;
				}

				// Stop the last coroutine
				if (lastCoroutine != null)
				{
					StopCoroutine(lastCoroutine);
				}

				// Play the background audio
				lastCoroutine = StartCoroutine(PlayBackgroundAudio());

				IEnumerator PlayBackgroundAudio()
				{
					// Reset flags
					isMusicAdded = false;
					isAmbienceAdded = false;

					// Start the coroutines
					Coroutine musicCoroutine = musicBehavior.StartCoroutine(this, AudioManager.Music,
						(source, player) => isMusicAdded = (player.Player != null));
					Coroutine ambienceCoroutine = ambienceBehavior.StartCoroutine(this, AudioManager.Ambience,
						(source, player) => isMusicAdded = (player.Player != null));

					// Delay the yielding so loading both music and ambience can happen at around the same time
					yield return musicCoroutine;
					yield return ambienceCoroutine;

					// Indicate the coroutine has finished
					lastCoroutine = null;
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			// Make sure this collider was actually in the trigger before
			if (revertOnExit && other.CompareTag(colliderTag))
			{
				// Check if there are still other colliders in this trigger
				--numCollidersEntered;
				if (numCollidersEntered > 0)
				{
					// If so, don't bother swapping music
					return;
				}

				// Stop the last coroutine
				if (lastCoroutine != null)
				{
					StopCoroutine(lastCoroutine);
				}

				// Play the background audio
				lastCoroutine = StartCoroutine(RevertBackgroundAudio());

				IEnumerator RevertBackgroundAudio()
				{
					// Start the coroutines
					Coroutine musicCoroutine = null, ambienceCoroutine = null;

					// Check if music has been added OnTriggerEnter
					musicCoroutine = FadeInPrevious(AudioManager.Music, musicBehavior, isMusicAdded);
					ambienceCoroutine = FadeInPrevious(AudioManager.Ambience, ambienceBehavior, isAmbienceAdded);

					// Delay the yielding so loading both music and ambience can happen at around the same time
					yield return musicCoroutine;
					yield return ambienceCoroutine;

					// Indicate the coroutine has finished
					lastCoroutine = null;

				}

				Coroutine FadeInPrevious(AudioLayer.Background layer, PlaybackBehavior behavior, bool isAdded)
				{
					if (isAdded)
					{
						return StartCoroutine(layer.PlayPreviousCoroutine(behavior.GetFadeInArgs(), behavior.GetFadeOutArgs()));
					}
					else if (behavior.Behavior == PlaybackBehavior.FadeBehavior.FadeToSilence)
					{
						return StartCoroutine(layer.FadeInCurrentPlayingCoroutine(behavior.GetFadeInArgs()));
					}
					return null;
				}
			}
		}
	}
}
