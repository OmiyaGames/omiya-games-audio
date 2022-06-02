using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="ChangeBackgroundAudioOnStart.cs" company="Omiya Games">
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
	/// Changes the background music on scene start.
	/// </summary>
	public class ChangeBackgroundAudioOnStart : MonoBehaviour
	{
		/// <summary>
		/// TODO
		/// </summary>
		public enum Behavior
		{
			/// <summary>
			/// TODO
			/// </summary>
			ClearHistory,
			/// <summary>
			/// TODO
			/// </summary>
			PausePriorMusic,
			/// <summary>
			/// TODO
			/// </summary>
			StopPriorMusic
		}

		[SerializeField]
		bool useAddressables = false;
		[SerializeField]
		BackgroundAudio playMusic;
		[SerializeField]
		BackgroundAudio playAmbience;
		[SerializeField]
		AssetReferenceT<BackgroundAudio> playMusicRef;
		[SerializeField]
		AssetReferenceT<BackgroundAudio> playAmbienceRef;

		[Header("Play Behavior")]
		[SerializeField]
		double fadeInSeconds = 0.5f;
		[SerializeField]
		[Tooltip("If true, restarts the music even if it's already playing in the background.")]
		bool alwaysRestart = false;
		[SerializeField]
		[Tooltip("The behavior to apply to music playing prior to Start. Clear History unloads it from memory.")]
		Behavior historyBehavior = Behavior.ClearHistory;

		/// <summary>
		/// Sets up the <seealso cref="BackgroundAudio"/> and <seealso cref="AudioManager"/>.
		/// </summary>
		/// <returns>The coroutine for loading everything.</returns>
		public virtual IEnumerator Start()
		{
			// Setup the manager
			yield return AudioManager.Setup();

			// Verify if everthing loaded correctly
			if (AudioManager.Status == Global.Settings.Data.Status.Fail)
			{
				Debug.LogError("Unable to AudioManager.", this);
				yield break;
			}

			// Setup args
			FadeInArgs fadeInArgs = new()
			{
				DurationSeconds = fadeInSeconds,
				ForceRestart = alwaysRestart,
				FadeOut = new()
				{
					DurationSeconds = fadeInSeconds,
					Pause = (historyBehavior == Behavior.PausePriorMusic)
				}
			};

			// Setup this music
			if (useAddressables == false)
			{
				PushMusicDataToStack(playMusic, AudioManager.Music, fadeInArgs);
				PushMusicDataToStack(playAmbience, AudioManager.Ambience, fadeInArgs);
				yield break;
			}

			// Load the addressables
			List<Coroutine> allLoads = new List<Coroutine>(2);
			PushMusicDataToStack(playMusicRef, AudioManager.Music.Player, fadeInArgs, allLoads);
			PushMusicDataToStack(playAmbienceRef, AudioManager.Ambience.Player, fadeInArgs, allLoads);

			// Wait until all fade-outs are over
			foreach (var load in allLoads)
			{
				yield return load;
			}
		}

		void PushMusicDataToStack(BackgroundAudio playAudio, AudioLayer.Background backgroundAudio, FadeInArgs fadeInArgs)
		{
			// Make sure asset is valid
			if (playAudio)
			{
				// Fade the currently playing players out
				BackgroundAudio.Player[] fadingPlayers = backgroundAudio.GroupManager.GetManagedPlayers();
				foreach (var fadingPlayer in fadingPlayers)
				{
					backgroundAudio.GroupManager.FadeOut(fadingPlayer, fadeInArgs.FadeOut);
				}

				// Fade the player in
				BackgroundAudio.Player player = backgroundAudio.PlayerManager.GetOrCreatePlayer(playAudio);
				backgroundAudio.GroupManager.FadeIn(player, fadeInArgs);

				// TODO: Push this music into the history
				//history.Push(playAudio, fadeInArgs);
			}

			var cleanUp = AudioPlayerManager.AudioState.Stopped;
			if (historyBehavior == Behavior.ClearHistory)
			{
				cleanUp |= AudioPlayerManager.AudioState.NotPlaying | AudioPlayerManager.AudioState.Scheduled;
			}

			// Clean up music manager
			backgroundAudio.PlayerManager.GarbageCollect(cleanUp);
		}

		void PushMusicDataToStack(AssetReferenceT<BackgroundAudio> playAudio, MusicDataStack history, FadeInArgs fadeInArgs, List<Coroutine> allLoads)
		{
			// Check if we want to clear the music history
			if (historyBehavior == Behavior.ClearHistory)
			{
				history.Clear(fadeInArgs.FadeOut);
			}

			// Make sure asset is valid
			if (string.IsNullOrEmpty(playAudio.AssetGUID) == false)
			{
				// Push this music into the history
				allLoads.Add(StartCoroutine(history.Push(playAudio, fadeInArgs)));
			}
		}
	}
}
