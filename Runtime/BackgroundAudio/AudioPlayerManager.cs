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
	/// <strong>Version:</strong> 1.0.0<br/>
	/// <strong>Date:</strong> 5/22/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Handles managing <see cref="BackgroundAudio.Player"/>s, including
	/// generating and auto-removing unused players.  Generally used like
	/// a <see cref="Dictionary{TKey, TValue}"/>, where
	/// <see cref="BackgroundAudio"/> is mapped to a list of
	/// <see cref="BackgroundAudio.Player"/>s.
	/// </summary>
	public class AudioPlayerManager : MonoBehaviour
	{
		/// <summary>
		/// An equivalent of <seealso cref="BackgroundAudio.PlayState"/>,
		/// where multiple values can be combined like a flag.
		/// </summary>
		[System.Flags]
		public enum AudioState : byte
		{
			/// <summary>
			/// Empty flags.  Represents no state.
			/// </summary>
			None = 0,
			/// <summary>
			/// Equivalent of <seealso cref="BackgroundAudio.PlayState.Playing"/>.
			/// Matches any <see cref="BackgroundAudio.Player"/>s that's playing.
			/// </summary>
			Playing = 1 << 1,
			/// <summary>
			/// Equivalent of <seealso cref="BackgroundAudio.PlayState.Stopped"/>.
			/// Matches any <see cref="BackgroundAudio.Player"/>s that's stopped.
			/// </summary>
			Stopped = 1 << 2,
			/// <summary>
			/// Equivalent of <seealso cref="BackgroundAudio.PlayState.Scheduled"/>.
			/// Matches any <see cref="BackgroundAudio.Player"/>s that scheduled
			/// to play at a later point.
			/// </summary>
			Scheduled = 1 << 3,
			/// <summary>
			/// Equivalent of <seealso cref="BackgroundAudio.PlayState.Paused"/>.
			/// </summary>
			//Paused = 1 << 2,

			/// <summary>
			/// Represents all the flags in this enum.
			/// </summary>
			All = Playing | Stopped | Scheduled/* | Paused*/,
		}

		/// <summary>
		/// Helper class tracking the state of <see cref="BackgroundAudio.Player"/>.
		/// </summary>
		class PlayerMetaData
		{
			public float LastStateChanged
			{
				get;
				set;
			}
			public Coroutine GarbageCollector
			{
				get;
				set;
			}
		}

		[SerializeField]
		[Range(1f, 20f)]
		float garbageCollectEverySeconds = 5;

		readonly System.Text.StringBuilder nameBuilder = new();
		readonly Dictionary<AssetRef<BackgroundAudio>, Dictionary<BackgroundAudio.Player, PlayerMetaData>> generatedPlayers = new();

		/// <summary>
		/// Creates a new <see cref="GameObject"/>, and attaching
		/// an instance of <see cref="AudioPlayerManager"/>.
		/// </summary>
		/// <param name="parent">
		/// The <see cref="Transform"/> this manager will be added as a child to.
		/// Can be <see langword="null"/>
		/// </param>
		/// <param name="gameObjectName">
		/// The name of the new <see cref="GameObject"/>
		/// that's created by this method.
		/// </param>
		/// <returns>
		/// A new instance of <see cref="AudioPlayerManager"/>,
		/// attached by a new <see cref="GameObject"/>, which is a child
		/// of <paramref name="parent"/>.
		/// </returns>
		public static AudioPlayerManager CreateManager(Transform parent, string gameObjectName)
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
		/// Checks if an <see cref="BackgroundAudio.Player"/> is at a state that matches
		/// any of the flags in <paramref name="state"/>.
		/// </summary>
		/// <param name="player">
		/// The player to check the state.
		/// </param>
		/// <param name="state">
		/// Flags to compare the state of the <paramref name="player"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the <param name="player"/>
		/// is in a state that matches any one of flags in
		/// <paramref name="state"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="player"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsPlayerMatchingState(BackgroundAudio.Player player, AudioState state)
		{
			if (player == null)
			{
				throw new System.ArgumentNullException(nameof(player));
			}

			AudioState compareState = AudioState.None;
			switch (player.State)
			{
				case BackgroundAudio.PlayState.Playing:
					compareState = AudioState.Playing;
					break;
				//case BackgroundAudio.PlayState.Paused:
				//	compareState = AudioState.Paused;
				//	break;
				case BackgroundAudio.PlayState.Stopped:
					compareState = AudioState.Stopped;
					break;
				case BackgroundAudio.PlayState.Scheduled:
					compareState = AudioState.Scheduled;
					break;
			}
			return (state & compareState) != 0;
		}

		/// <summary>
		/// Gets or sets how long an idle (basically, stopped)
		/// <see cref="BackgroundAudio.Player"/> remains in memory
		/// before being auto-deleted by this manager.
		/// </summary>
		public float GarbageCollectAfterSeconds
		{
			get => garbageCollectEverySeconds;
			set => garbageCollectEverySeconds = value;
		}

		/// <summary>
		/// Gets the first <see cref="BackgroundAudio.Player"/> associated
		/// with an audio asset at a specific state.
		/// </summary>
		/// <param name="audio">
		/// The audio file that generates players.
		/// </param>
		/// <param name="playerState">
		/// Filters <see cref="BackgroundAudio.Player"/>s based on state.
		/// </param>
		/// <returns>
		/// The first <see cref="BackgroundAudio.Player"/> that plays
		/// the audio clips from <paramref name="audio"/> that is in the same
		/// state as <paramref name="playerState"/>. Returns
		/// <see langword="null"/> if none is found.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="audio"/>'s state is
		/// <see cref="AssetRef.State.Null"/>.
		/// </exception>
		/// <seealso cref="GetPlayers(AssetRef{BackgroundAudio}, AudioState)"/>
		/// <seealso cref="GetPlayers(AssetRef{BackgroundAudio}, AudioState, in List{BackgroundAudio.Player})"/>
		public BackgroundAudio.Player GetPlayer(AssetRef<BackgroundAudio> audio, AudioState playerState = AudioState.All)
		{
			if (audio.CurrentState == AssetRef.State.Null)
			{
				throw new System.ArgumentNullException(nameof(audio));
			}

			// Check if there's a map of players available for the audio
			if (generatedPlayers.TryGetValue(audio, out var metaData))
			{
				// If so, go through each player
				foreach (var player in metaData.Keys)
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
		/// Starts a coroutine that generates a new
		/// <see cref="BackgroundAudio.Player"/> from <paramref name="audio"/>.
		/// </summary>
		/// <param name="audio">
		/// The audio file that generates players.
		/// </param>
		/// <param name="onPlayerCreated">
		/// Delegate that triggers at the end of this coroutine.
		/// Argument will be set to the newly generated
		/// <see cref="BackgroundAudio.Player"/>.
		/// </param>
		/// <returns>
		/// A coroutine that creates a new
		/// <see cref="BackgroundAudio.Player"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="audio"/>'s state is
		/// <see cref="AssetRef.State.Null"/>.
		/// </exception>
		public IEnumerator CreatePlayerCoroutine(AssetRef<BackgroundAudio> audio, System.Action<BackgroundAudio.Player> onPlayerCreated = null)
		{
			// Null-check
			if (audio.CurrentState == AssetRef.State.Null)
			{
				throw new System.ArgumentNullException(nameof(audio));
			}

			// Check if a player already exists
			if (generatedPlayers.TryGetValue(audio, out var playerMap) == false)
			{
				// If not, find or create a new audio player map
				playerMap = new();
				generatedPlayers.Add(audio, playerMap);
			}

			// Load the metadata (the next couple of steps can be lengthy, so don't yield yet)
			IEnumerator loadAssetHandle = audio.LoadAssetAsync();

			// Develop name of append
			nameBuilder.Append(audio.Name);
			nameBuilder.Append(" (");
			nameBuilder.Append(playerMap.Count);
			nameBuilder.Append(')');

			// Create a new object
			GameObject newObject = new GameObject(nameBuilder.ToString());
			newObject.transform.SetParent(transform);
			newObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			newObject.transform.localScale = Vector3.one;

			// Wait until asset is fully loaded
			nameBuilder.Clear();
			yield return loadAssetHandle;

			// Generate the new player
			BackgroundAudio.Player newPlayer = audio.Asset.GeneratePlayer(newObject);

			// Generate a metadata
			PlayerMetaData newMetaData = new()
			{
				LastStateChanged = Time.unscaledTime
			};
			newPlayer.OnAfterChangeState += OnAfterChangeState;

			// Add all that into the map
			playerMap.Add(newPlayer, newMetaData);

			// Invoke action
			onPlayerCreated?.Invoke(newPlayer);

			void OnAfterChangeState(BackgroundAudio.PlayState _, BackgroundAudio.PlayState newState)
			{
				// Update time stamp
				newMetaData.LastStateChanged = Time.unscaledTime;

				// Check new state
				if ((newState == BackgroundAudio.PlayState.Stopped) && (newMetaData.GarbageCollector == null))
				{
					// Start the garbage collection coroutine if player is stopped
					newMetaData.GarbageCollector = newPlayer.StartCoroutine(DelayGarbageCollect());
				}
				else if ((newState != BackgroundAudio.PlayState.Stopped) && (newMetaData.GarbageCollector != null))
				{
					// Otherwise, stop the garbage collection sequence
					StopCoroutine(newMetaData.GarbageCollector);
					newMetaData.GarbageCollector = null;
				}
			}

			IEnumerator DelayGarbageCollect()
			{
				// Wait for desired seconds
				yield return new WaitForSecondsRealtime(GarbageCollectAfterSeconds);

				// Perform garbage collection
				if (generatedPlayers.TryGetValue(audio, out var audioMetaData))
				{
					// Destroy the player
					audioMetaData.Remove(newPlayer);
					Destroy(newPlayer.gameObject);

					// Check if map is empty
					if (audioMetaData.Count <= 0)
					{
						// Search for the original key
						foreach (var key in generatedPlayers.Keys)
						{
							if (key.Equals(audio))
							{
								// Release the asset on this key
								key.ReleaseAsset();
								break;
							}
						}

						// Remove the key from the map
						generatedPlayers.Remove(audio);
					}
				}
			}
		}

		/// <summary>
		/// <seealso cref="Object.Destroy(Object)">Destroys</seealso> any
		/// <see cref="BackgroundAudio.Player"/>s that has the same state
		/// as <paramref name="destroyPlayersWithStates"/>.
		/// </summary>
		/// <param name="destroyPlayersWithStates">
		/// The state of <see cref="BackgroundAudio.Player"/>s to destroy.
		/// </param>
		public void GarbageCollect(AudioState destroyPlayersWithStates = AudioState.Stopped)
		{
			// Check if there's a map of players available for the audio
			List<AssetRef<BackgroundAudio>> audioToDelete = new(generatedPlayers.Count);
			foreach (var metaData in generatedPlayers)
			{
				// If so, go through each player
				List<BackgroundAudio.Player> playersToDelete = new(metaData.Value.Count);
				foreach (var player in metaData.Value.Keys)
				{
					// Check if condition matches
					if (IsPlayerMatchingState(player, destroyPlayersWithStates))
					{
						// Mark the player for deletion
						playersToDelete.Add(player);
					}
				}

				// Delete the player from the map
				foreach (var player in playersToDelete)
				{
					metaData.Value.Remove(player);
					Destroy(player.gameObject);
				}

				// Check if there are any players left in the map
				if (metaData.Value.Count <= 0)
				{
					audioToDelete.Add(metaData.Key);
				}
			}

			// Check if we want to delete audio files
			foreach (var audioFiles in audioToDelete)
			{
				generatedPlayers.Remove(audioFiles);
				audioFiles.ReleaseAsset();
			}
		}

		/// <summary>
		/// Gets a list of players associated with <paramref name="audio"/>,
		/// filtered by <paramref name="playerState"/>.  This method recycles
		/// a <see cref="List{T}"/>.
		/// </summary>
		/// <param name="audio">
		/// The audio file that generates players.
		/// </param>
		/// <param name="playerState">
		/// Filters <see cref="BackgroundAudio.Player"/>s based on state.
		/// </param>
		/// <param name="returnPlayers">
		/// The list to append <see cref="BackgroundAudio.Player"/>s to.
		/// Effectively the return variable.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="audio"/>'s state is
		/// <see cref="AssetRef.State.Null"/>.
		/// </exception>
		public void GetPlayers(AssetRef<BackgroundAudio> audio, AudioState playerState, in List<BackgroundAudio.Player> returnPlayers)
		{
			if (returnPlayers == null)
			{
				throw new System.ArgumentNullException(nameof(returnPlayers));
			}

			// Check if there's a map of players available for the audio
			if (generatedPlayers.TryGetValue(audio, out var metaData))
			{
				// If so, go through each player
				foreach (var player in metaData.Keys)
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
		/// Gets a list of players associated with <paramref name="audio"/>,
		/// filtered by <paramref name="playerState"/>.  This method creates
		/// a new <see cref="List{T}"/>.
		/// </summary>
		/// <param name="audio">
		/// The audio file that generates players.
		/// </param>
		/// <param name="playerState">
		/// Filters <see cref="BackgroundAudio.Player"/>s based on state.
		/// </param>
		/// <returns>
		/// List of <see cref="BackgroundAudio.Player"/>s associated with
		/// <paramref name="audio"/>, and has the same state as
		/// <paramref name="playerState"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="audio"/>'s state is
		/// <see cref="AssetRef.State.Null"/>.
		/// </exception>
		/// <seealso cref="GetPlayers(AssetRef{BackgroundAudio}, AudioState, in List{BackgroundAudio.Player})"/>
		public List<BackgroundAudio.Player> GetPlayers(AssetRef<BackgroundAudio> audio, AudioState playerState = AudioState.All)
		{
			// By default, return null
			List<BackgroundAudio.Player> returnPlayers = new();
			GetPlayers(audio, playerState, in returnPlayers);
			return returnPlayers;
		}

		void OnDestroy()
		{
			StopAllCoroutines();
		}
	}
}
