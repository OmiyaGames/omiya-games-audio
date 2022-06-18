using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
	/// <strong>Version:</strong> 1.1.0-pre.1<br/>
	/// <strong>Date:</strong> 4/14/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Changes the background music on trigger enter.
	/// </summary>
	public class ChangeAudioOnTrigger : MonoBehaviour
	{
		[SerializeField]
		AssetRefSerialized<BackgroundAudio> playMusic;
		[SerializeField]
		AssetRefSerialized<BackgroundAudio> playAmbience;

		[Header("Trigger Behavior")]
		[SerializeField]
		[Tooltip("Only colliders with this tag will trigger audio changes")]
		string colliderTag = "Player";
		[SerializeField]
		[Tooltip("If checked, reverts the music to the one last played")]
		bool revertOnExit = true;

		[Header("Play Behavior")]
		[SerializeField]
		float fadeInSeconds = 0.5f;
		[SerializeField]
		[Tooltip("If true, restarts the music even if it's already playing in the background.")]
		bool alwaysRestart = false;

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
				lastCoroutine = StartCoroutine(PlayBackgroundAudio(new()
				{
					DurationSeconds = fadeInSeconds,
					ForceRestart = alwaysRestart,
					FadeOut = new()
					{
						DurationSeconds = fadeInSeconds
					}
				}));

				IEnumerator PlayBackgroundAudio(FadeInArgs fadeInArgs)
				{
					yield return StartCoroutine(AudioManager.PlayMusicAndAmbience(playMusic, playAmbience, fadeInArgs));
					isMusicAdded = playMusic.HasValue;
					isAmbienceAdded = playAmbience.HasValue;

					// Indicate the coroutine has finished
					lastCoroutine = null;
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			// Make sure this collider was actually in the trigger before
			if (other.CompareTag(colliderTag))
			{
				// Check if there are still other colliders in this trigger
				--numCollidersEntered;
				if ((numCollidersEntered > 0) || (revertOnExit == false))
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
				lastCoroutine = StartCoroutine(RevertBackgroundAudio(new()
				{
					DurationSeconds = fadeInSeconds,
					FadeOut = new()
					{
						DurationSeconds = fadeInSeconds
					}
				}));

				IEnumerator RevertBackgroundAudio(FadeInArgs fadeInArgs)
				{
					// Check if music has been added OnTriggerEnter
					// Pop the latest music from history, and grab the next latest one.
					AssetRef<BackgroundAudio> lastMusic = isMusicAdded ? PopMusicFromHistory(AudioManager.Music.History) : new();

					// Pop the latest music from history, and grab the next latest one.
					AssetRef<BackgroundAudio> lastAmbience = isAmbienceAdded ? PopMusicFromHistory(AudioManager.Ambience.History) : new();

					// Play the last tunes from history
					yield return StartCoroutine(AudioManager.PlayMusicAndAmbience(lastMusic, lastAmbience, fadeInArgs));

					// Indicate the coroutine has finished
					lastCoroutine = null;
				}
			}
		}

		static AssetRef<BackgroundAudio> PopMusicFromHistory(AudioHistory history)
		{
			// Setup null
			AssetRef<BackgroundAudio> lastMusic = new();

			// Pop a music off from the history
			history.RemoveLatest();

			// Return the next newest music
			AssetRef<BackgroundAudio>? poppedMusic = history.Latest;
			if (poppedMusic.HasValue)
			{
				lastMusic = poppedMusic.Value;
			}
			return lastMusic;
		}
	}
}
