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
	/// Changes the background music on scene start.
	/// </summary>
	public class ChangeAudioOnStart : MonoBehaviour
	{
		/// <summary>
		/// The behavior at the end of <seealso cref="ChangeAudioOnStart"/>.
		/// </summary>
		public enum Behavior
		{
			/// <summary>
			/// Clears <seealso cref="AudioHistory"/>.
			/// </summary>
			ClearHistory,
			/// <summary>
			/// Just stop playing the audio
			/// that was playing prior to this.
			/// </summary>
			StopPriorAudio
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
		PlaybackBehavior musicBehavior = new(PlaybackBehavior.FadeBehavior.FadeInNewAudio);
		[SerializeField]
		PlaybackBehavior ambienceBehavior = new(PlaybackBehavior.FadeBehavior.FadeToSilence);
		[SerializeField]
		[Tooltip("The behavior to apply to background audio playing prior to Start. Clear History removes them from history.")]
		Behavior historyBehavior = Behavior.ClearHistory;

		/// <summary>
		/// Sets up the <seealso cref="AudioManager"/> to play
		/// <seealso cref="BackgroundAudio"/>s for both music and
		/// ambience to whatever is set in the Unity inspector.
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

			// Switch the music and ambience
			Coroutine musicCoroutine = musicBehavior.StartCoroutine(this, AudioManager.Music);
			Coroutine ambienceCoroutine = ambienceBehavior.StartCoroutine(this, AudioManager.Ambience);

			// Delay the yielding so loading both music and ambience can happen at around the same time
			yield return musicCoroutine;
			yield return ambienceCoroutine;

			// Clean-up the currently loaded music
			var cleanUp = AudioPlayerManager.AudioState.Stopped;
			if (historyBehavior == Behavior.ClearHistory)
			{
				cleanUp |= AudioPlayerManager.AudioState.Scheduled;
			}
			AudioManager.Music.PlayerManager.GarbageCollect(cleanUp);
			AudioManager.Ambience.PlayerManager.GarbageCollect(cleanUp);

			// Clear all but one audio file in each layer's history
			if (historyBehavior == Behavior.ClearHistory)
			{
				while (AudioManager.Music.History.Count > 1)
				{
					AudioManager.Music.History.RemoveOldest();
				}
				while (AudioManager.Ambience.History.Count > 1)
				{
					AudioManager.Ambience.History.RemoveOldest();
				}
			}

			// Invoke event
			OnAfterAudioChange?.Invoke(this);
		}
	}
}
