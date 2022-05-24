using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AudioPlayerManager.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 5/22/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// TODO
	/// </summary>
	public class AudioPlayerManager : MonoBehaviour
	{
		/// <summary>
		/// TODO
		/// </summary>
		[System.Flags]
		public enum AudioState : byte
		{
			/// <summary>
			/// TODO
			/// </summary>
			None = 0,
			/// <summary>
			/// TODO
			/// </summary>
			Playing = 1 << 1,
			/// <summary>
			/// TODO
			/// </summary>
			Paused = 1 << 2,
			/// <summary>
			/// TODO
			/// </summary>
			Stopped = 1 << 3,
			/// <summary>
			/// TODO
			/// </summary>
			Scheduled = 1 << 4,

			/// <summary>
			/// TODO
			/// </summary>
			NotPlaying = Paused | Stopped,
			/// <summary>
			/// TODO
			/// </summary>
			All = Playing | Paused | Stopped | Scheduled,
		}

		class AudioMetaData
		{
			public Dictionary<BackgroundAudio.Player, PlayerMetaData> PlayerMap
			{
				get;
			} = new();
		}

		class PlayerMetaData
		{
			public float lastStateChanged;
		}

		[SerializeField]
		[Range(1f, 20f)]
		float garbageCollectEverySeconds = 10;

		WaitForSecondsRealtime waitInterval = null;
		readonly System.Text.StringBuilder nameBuilder = new();
		readonly HashSet<BackgroundAudio.Player> lockedPlayers = new();
		readonly Dictionary<BackgroundAudio, AudioMetaData> generatedPlayers = new();

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="gameObjectName"></param>
		/// <returns></returns>
		public static AudioPlayerManager Create(Transform parent, string gameObjectName)
		{
			// Create the GameObject
			GameObject gameObject = new GameObject(gameObjectName);

			// Position the GameObject
			gameObject.transform.SetParent(parent);
			gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			gameObject.transform.localScale = Vector3.one;

			// Create the manager script
			return gameObject.AddComponent<AudioPlayerManager>();
		}

		/// <summary>
		/// TODO
		/// </summary>
		public float GarbageCollectEverySeconds
		{
			get => garbageCollectEverySeconds;
			set => garbageCollectEverySeconds = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="audio"></param>
		/// <param name="playerState"></param>
		/// <returns></returns>
		public BackgroundAudio.Player GetPlayer(BackgroundAudio audio, AudioState playerState = AudioState.All)
		{
			// Check if there's a map of players available for the audio
			if (generatedPlayers.TryGetValue(audio, out var metaData))
			{
				// If so, go through each player
				foreach (var player in metaData.PlayerMap.Keys)
				{
					// Check if condition matches
					if (IsPlayerMatchingState(player, playerState))
					{
						return player;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="audio"></param>
		/// <param name="playerState"></param>
		/// <returns></returns>
		public BackgroundAudio.Player GetOrCreatePlayer(BackgroundAudio audio, AudioState playerState = AudioState.All)
		{
			// Check if a player already exists
			BackgroundAudio.Player returnPlayer = GetPlayer(audio, playerState);
			if (returnPlayer == null)
			{
				// If not, find or create a new audio player map
				if (generatedPlayers.TryGetValue(audio, out var metaData) == false)
				{
					// Create a new map
					metaData = new();

					// Make sure this is in the record
					generatedPlayers.Add(audio, metaData);
				}

				// Create a new player and its metadata
				returnPlayer = CreatePlayer(audio, metaData.PlayerMap.Count);
				metaData.PlayerMap.Add(returnPlayer, CreateMetaData());
			}
			return returnPlayer;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="removeStates"></param>
		public void GarbageCollect(AudioState removeStates = AudioState.NotPlaying)
		{
			// FIXME: do something!
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Prevent an audio player from being destroyed by the garbage collector.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool LockPlayer(BackgroundAudio.Player player) => lockedPlayers.Add(player);

		/// <summary>
		/// Release an audio player to the garbage collector.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool ReleasePlayer(BackgroundAudio.Player player) => lockedPlayers.Remove(player);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="audio"></param>
		/// <param name="returnPlayers"></param>
		/// <param name="playerState"></param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void GetPlayers(BackgroundAudio audio, AudioState playerState, in List<BackgroundAudio.Player> returnPlayers)
		{
			if (returnPlayers == null)
			{
				throw new System.ArgumentNullException(nameof(returnPlayers));
			}

			// Check if there's a map of players available for the audio
			if (generatedPlayers.TryGetValue(audio, out var metaData))
			{
				// If so, go through each player
				foreach (var player in metaData.PlayerMap.Keys)
				{
					// Check if condition matches
					if (IsPlayerMatchingState(player, playerState))
					{
						// Create a list of these matching players
						returnPlayers.Add(player);
					}
				}
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="audio"></param>
		/// <param name="returnPlayers"></param>
		/// <param name="playerState"></param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void GetOrCreatePlayers(BackgroundAudio audio, AudioState playerState, in List<BackgroundAudio.Player> returnPlayers)
		{
			if (returnPlayers == null)
			{
				throw new System.ArgumentNullException(nameof(returnPlayers));
			}

			// Grab the players
			int numPlayers = returnPlayers.Count;
			GetPlayers(audio, playerState, in returnPlayers);

			// Check if any was returned
			if (returnPlayers.Count == numPlayers)
			{
				// If not, find or create a new audio player map
				if (generatedPlayers.TryGetValue(audio, out var metaData) == false)
				{
					// Create a new map
					metaData = new();

					// Make sure this is in the record
					generatedPlayers.Add(audio, metaData);
				}

				// Create a new player and its metadata
				var newPlayer = CreatePlayer(audio, metaData.PlayerMap.Count);
				metaData.PlayerMap.Add(newPlayer, CreateMetaData());

				// Add the new player into the list
				returnPlayers.Add(newPlayer);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="audio"></param>
		/// <param name="playerState"></param>
		/// <returns></returns>
		public List<BackgroundAudio.Player> GetPlayers(BackgroundAudio audio, AudioState playerState = AudioState.All)
		{
			// By default, return null
			List<BackgroundAudio.Player> returnPlayers = new();
			GetPlayers(audio, playerState, in returnPlayers);
			return returnPlayers;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="audio"></param>
		/// <param name="playerState"></param>
		/// <returns></returns>
		public List<BackgroundAudio.Player> GetOrCreatePlayers(BackgroundAudio audio, AudioState playerState = AudioState.All)
		{
			// By default, return null
			List<BackgroundAudio.Player> returnPlayers = new();
			GetOrCreatePlayers(audio, playerState, in returnPlayers);
			return returnPlayers;
		}

		void Start()
		{
			waitInterval = new WaitForSecondsRealtime(GarbageCollectEverySeconds);
			StartCoroutine(DelayGarbageCollect());
		}

		void OnDestroy()
		{
			StopAllCoroutines();
		}

		#region Helper Methods
		static bool IsPlayerMatchingState(BackgroundAudio.Player player, AudioState state)
		{
			AudioState compareState = AudioState.None;
			switch (player.State)
			{
				case BackgroundAudio.PlayState.Playing:
					compareState = AudioState.Playing;
					break;
				case BackgroundAudio.PlayState.Paused:
					compareState = AudioState.Paused;
					break;
				case BackgroundAudio.PlayState.Stopped:
					compareState = AudioState.Stopped;
					break;
				case BackgroundAudio.PlayState.Scheduled:
					compareState = AudioState.Scheduled;
					break;
			}
			return (state & compareState) != 0;
		}

		IEnumerator DelayGarbageCollect()
		{
			// Run until OnDestroy
			while (true)
			{
				// Wait for desired seconds
				waitInterval.waitTime = GarbageCollectEverySeconds;
				yield return waitInterval;

				// FIXME: Perform garbage collection
				//GarbageCollect(AudioState.Stopped);
			}
		}

		BackgroundAudio.Player CreatePlayer(BackgroundAudio audio, int appendNumber)
		{
			// Develop name of append
			nameBuilder.Append(audio.name);
			nameBuilder.Append(" (");
			nameBuilder.Append(appendNumber);
			nameBuilder.Append(')');

			// Create a new object
			GameObject newObject = new GameObject(nameBuilder.ToString());
			newObject.transform.SetParent(transform);
			newObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			newObject.transform.localScale = Vector3.one;

			// Reset stuff
			nameBuilder.Clear();

			return audio.GeneratePlayer(newObject);
		}

		static PlayerMetaData CreateMetaData()
		{
			return new PlayerMetaData()
			{
				lastStateChanged = Time.unscaledTime
			};
		}
		#endregion
	}
}
