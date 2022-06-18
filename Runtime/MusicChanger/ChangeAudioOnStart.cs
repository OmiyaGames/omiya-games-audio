using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="ChangeAudioOnStart.cs" company="Omiya Games">
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
	public class ChangeAudioOnStart : MonoBehaviour
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
			StopPriorMusic
		}

		class AudioFilePlayerPair
		{
			public AssetRef<BackgroundAudio> File
			{
				get;
				set;
			}
			public BackgroundAudio.Player Player
			{
				get;
				set;
			}
		}

		public event System.Action<ChangeAudioOnStart> OnBeforeAudioChange;
		public event System.Action<ChangeAudioOnStart> OnAfterAudioChange;

		[SerializeField]
		AssetRefSerialized<BackgroundAudio> playMusic;
		[SerializeField]
		AssetRefSerialized<BackgroundAudio> playAmbience;

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

			// Start the event
			OnBeforeAudioChange?.Invoke(this);

			// Setup args
			FadeInArgs fadeInArgs = new()
			{
				DurationSeconds = fadeInSeconds,
				ForceRestart = alwaysRestart,
			};
			FadeOutArgs fadeOutArgs = new()
			{
				DurationSeconds = fadeInSeconds,
			};

			// Switch the music and ambience
			yield return StartCoroutine(AudioManager.PlayMusicAndAmbience(playMusic, playAmbience, fadeInArgs, fadeOutArgs));

			// Clean-up the currently loaded music
			GarbageCollect(AudioManager.Music);
			GarbageCollect(AudioManager.Ambience);

			// Clear history
			if (historyBehavior == Behavior.ClearHistory)
			{
				PruneHistory(AudioManager.Music.History, playMusic);
				PruneHistory(AudioManager.Ambience.History, playAmbience);
			}

			// Invoke event
			OnAfterAudioChange?.Invoke(this);
		}

		void GarbageCollect(AudioLayer.Background backgroundAudio)
		{
			var cleanUp = AudioPlayerManager.AudioState.Stopped;
			if (historyBehavior == Behavior.ClearHistory)
			{
				cleanUp |= AudioPlayerManager.AudioState.NotPlaying | AudioPlayerManager.AudioState.Scheduled;
			}

			// Clean up music manager
			backgroundAudio.PlayerManager.GarbageCollect(cleanUp);
		}

		void PruneHistory(AudioHistory history, AssetRefSerialized<BackgroundAudio> playMusic)
		{
			if (playMusic.HasValue)
			{
				while (history.Count > 1)
				{
					history.RemoveOldest();
				}
			}
			else
			{
				history.Clear();
			}
		}
	}
}
