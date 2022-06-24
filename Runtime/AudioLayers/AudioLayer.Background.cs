using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OmiyaGames.Audio
{
	public partial class AudioLayer
	{
		///-----------------------------------------------------------------------
		/// <remarks>
		/// <copyright file="Background.cs" company="Omiya Games">
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
		/// <strong>Version:</strong> 0.1.0-exp.1<br/>
		/// <strong>Date:</strong> 2/27/2022<br/>
		/// <strong>Author:</strong> Taro Omiya
		/// </term>
		/// <description>Initial draft.</description>
		/// </item><item>
		/// <term>
		/// <strong>Version:</strong> 1.0.0<br/>
		/// <strong>Date:</strong> 6/3/2022<br/>
		/// <strong>Author:</strong> Taro Omiya
		/// </term>
		/// <description>
		/// Adding properties and methods that utilizes
		/// <see cref="UnityEngine.Audio.AudioMixerGroup"/> to fade in and out
		/// <see cref="BackgroundAudio"/>s.  Also added a <see cref="History"/>
		/// tracker.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		///-----------------------------------------------------------------------
		/// <summary>
		/// Audio category applicable to ones predominately used in the background,
		/// i.e. music and ambience.
		/// </summary>
		[System.Serializable]
		public class Background : SubLayer
		{
			[SerializeField]
			[Range(2, AudioHistory.DEFAULT_HISTORY_SIZE)]
			[Tooltip("Max capacity of audio history.")]
			int maxHistoryCapacity = AudioHistory.DEFAULT_HISTORY_SIZE;
			[SerializeField]
			[Tooltip("List of Audio Mixer Groups and parameters changing the volumes of the group.")]
			MixerGroupManager.Layer[] fadeLayers;

			AudioHistory history = null;

			/// <summary>
			/// Gets the <seealso cref="MixerGroupManager.Layer"/> used to setup
			/// <seealso cref="MixerGroupManager"/>.  These values are set in the
			/// Unity Project Settings dialog.
			/// </summary>
			public MixerGroupManager.Layer[] FadeLayers
			{
				get => fadeLayers;
				internal set => fadeLayers = value;
			}

			/// <summary>
			/// Gets the manager of <seealso cref="BackgroundAudio"/> and the
			/// <seealso cref="BackgroundAudio.Player"/>s generated by them.
			/// </summary>
			public AudioPlayerManager PlayerManager
			{
				get;
				internal set;
			} = null;

			/// <summary>
			/// Gets the manager of <see cref="UnityEngine.Audio.AudioMixerGroup"/>s
			/// and handles which group <seealso cref="BackgroundAudio.Player"/>
			/// should play on.
			/// </summary>
			public MixerGroupManager MixerGroupManager
			{
				get;
				internal set;
			}

			/// <summary>
			/// Gets the history of <seealso cref="BackgroundAudio"/> played.
			/// </summary>
			/// <remarks>
			/// No properties adds <seealso cref="BackgroundAudio"/>
			/// to this history tracker automatically.  As such, if you're
			/// <em>not</em> using the helper methods within this class (e.g.
			/// <see cref="PlayNextCoroutine(AssetRef{BackgroundAudio}, FadeInArgs, FadeOutArgs, System.Action{BackgroundAudio.Player})"/>),
			/// don't forget to update this property as well.
			/// </remarks>
			public AudioHistory History
			{
				get
				{
					if (history == null)
					{
						history = new AudioHistory(maxHistoryCapacity);
					}
					return history;
				}
			}

			/// <summary>
			/// Grabs the currently-playing audio file from <see cref="History"/>.
			/// </summary>
			public AssetRef<BackgroundAudio> CurrentFile => History.Newest;

			/// <summary>
			/// Grabs the currently-playing audio file from <see cref="History"/>.
			/// </summary>
			public BackgroundAudio.Player CurrentPlayer
			{
				get
				{
					BackgroundAudio.Player returnPlayer = null;

					// Check if current file is valid
					AssetRef<BackgroundAudio> currentFile = CurrentFile;
					if (currentFile.CurrentState != AssetRef.State.Null)
					{
						// Grab the currently managed players
						BackgroundAudio.Player[] fadingPlayers = MixerGroupManager.GetManagedPlayers();
						foreach (var fadingPlayer in fadingPlayers)
						{
							// Check if player data matches the current file
							if (currentFile.Equals(fadingPlayer.Data))
							{
								// Check if the player is already player
								if (AudioPlayerManager.IsPlayerMatchingState(fadingPlayer, AudioPlayerManager.AudioState.Playing))
								{
									// Return this immediately
									return fadingPlayer;
								}

								// Otherwise, hold the player in the backlog, to later return
								returnPlayer = fadingPlayer;
							}
						}
					}
					return returnPlayer;
				}
			}

			/// <summary>
			/// Grab all the players associated with <see cref="CurrentFile"/>
			/// </summary>
			/// <param name="state">The state to filter players by.</param>
			/// <returns>
			/// All the currently active <see cref="BackgroundAudio.Player"/>s
			/// with the same state as <paramref name="state"/>.
			/// </returns>
			public List<BackgroundAudio.Player> GetCurrentPlayers(AudioPlayerManager.AudioState state = AudioPlayerManager.AudioState.All)
			{
				// Grab the currently managed players
				BackgroundAudio.Player[] fadingPlayers = MixerGroupManager.GetManagedPlayers();
				List<BackgroundAudio.Player> returnPlayers = new(fadingPlayers.Length);

				// Check if current file is valid
				AssetRef<BackgroundAudio> currentFile = CurrentFile;
				if (currentFile.CurrentState != AssetRef.State.Null)
				{
					foreach (var fadingPlayer in fadingPlayers)
					{
						if (currentFile.Equals(fadingPlayer.Data) && AudioPlayerManager.IsPlayerMatchingState(fadingPlayer, state))
						{
							returnPlayers.Add(fadingPlayer);
						}
					}
				}
				return returnPlayers;
			}

			/// <summary>
			/// Grab the first player associated with <see cref="CurrentFile"/>
			/// </summary>
			/// <param name="state">The state to filter players by.</param>
			/// <returns>
			/// The first <see cref="BackgroundAudio.Player"/>s with the same state
			/// as <paramref name="state"/>.
			/// </returns>
			/// <remarks>
			/// Order (and thus, what's determined as "the first player") is
			/// dependent upon <seealso cref="FadeLayers"/>, and which player
			/// is running on which layer.
			/// </remarks>
			public BackgroundAudio.Player GetCurrentPlayer(AudioPlayerManager.AudioState state = AudioPlayerManager.AudioState.All)
			{
				// Check if current file is valid
				AssetRef<BackgroundAudio> currentFile = CurrentFile;
				if (currentFile.CurrentState != AssetRef.State.Null)
				{
					// Grab the currently managed players
					BackgroundAudio.Player[] fadingPlayers = MixerGroupManager.GetManagedPlayers();
					foreach (var fadingPlayer in fadingPlayers)
					{
						if (currentFile.Equals(fadingPlayer.Data) && AudioPlayerManager.IsPlayerMatchingState(fadingPlayer, state))
						{
							return fadingPlayer;
						}
					}
				}
				return null;
			}

			/// <summary>
			/// Coroutine to fade in the next background audio. Also adds
			/// <paramref name="audioClip"/> into the history if played.
			/// </summary>
			/// <param name="audioClip">
			/// The clip to play
			/// </param>
			/// <param name="fadeInArgs">
			/// Details on how to fade-in, e.g. how long it should last.
			/// </param>
			/// <param name="fadeOutArgs">
			/// Details on how to fade-out, e.g. how long it should last.
			/// </param>
			/// <param name="onFinish">
			/// Triggers as soon as this coroutine finishes, with a reference
			/// to the player that will be faded in. The parameter will be
			/// <see langword="null"/> if <paramref name="audioClip"/> isn't
			/// played (e.g. when it's already playing in the background.)
			/// </param>
			/// <returns>
			/// A coroutine for <em>loading</em> <paramref name="audioClip"/>.
			/// </returns>
			/// <exception cref="System.ArgumentNullException">
			/// If <paramref name="audioClip"/>'s <see cref="AssetRef{BackgroundAudio}.CurrentState"/>
			/// is <see cref="AssetRef.State.Null"/>.
			/// </exception>
			/// <remarks>
			/// This coroutine needs to be yielded on regardless of whether
			/// <paramref name="audioClip"/> points to an addressable or not.
			/// </remarks>
			public IEnumerator PlayNextCoroutine(AssetRef<BackgroundAudio> audioClip,
				FadeInArgs fadeInArgs = null, FadeOutArgs fadeOutArgs = null,
				System.Action<BackgroundAudio.Player> onFinish = null)
			{
				// Null-check first
				if (audioClip.CurrentState == AssetRef.State.Null)
				{
					throw new System.ArgumentNullException(nameof(audioClip));
				}

				// Fade in the new clip
				BackgroundAudio.Player audioPlayer = null;
				yield return FadeInCoroutine(audioClip, fadeInArgs, player => audioPlayer = player);

				// Check if a player was provided
				if (audioPlayer)
				{
					// Fade out the current players except the new one just started playing
					FadeOutCurrentPlaying(fadeOutArgs, audioPlayer);

					// Add clip into the history
					History.Add(audioClip);
				}

				// Invoke end event
				onFinish?.Invoke(audioPlayer);
			}

			/// <summary>
			/// Coroutine to fade in the previous background audio in <see cref="History"/>,
			/// if any.  Also removes the newest clip from the history, if played.
			/// </summary>
			/// <param name="fadeInArgs">
			/// Details on how to fade-in, e.g. how long it should last.
			/// </param>
			/// <param name="fadeOutArgs">
			/// Details on how to fade-out, e.g. how long it should last.
			/// </param>
			/// <param name="onFinish">
			/// Triggers as soon as this coroutine finishes, with a reference
			/// to the player that will be faded in. The parameter will be
			/// <see langword="null"/> if <paramref name="audioClip"/> isn't
			/// played (e.g. when it's already playing in the background.)
			/// </param>
			/// <returns>
			/// A coroutine for <em>loading</em> the previous
			/// <see cref="BackgroundAudio"/>.
			/// </returns>
			/// <remarks>
			/// This coroutine needs to be yielded on regardless of whether
			/// the previous clip points to an addressable or not.
			/// </remarks>
			public IEnumerator PlayPreviousCoroutine(FadeInArgs fadeInArgs = null, FadeOutArgs fadeOutArgs = null,
				System.Action<BackgroundAudio.Player> onFinish = null)
			{
				// Check the size of history
				if (History.Count < 2)
				{
					// There's no clip to go back to, finish immediately
					onFinish?.Invoke(null);
					yield break;
				}

				// Pop the latest music off from the history
				history.RemoveNewest();

				// Fade out the current players
				FadeOutCurrentPlaying(fadeOutArgs);

				// Fade in the next newest music
				yield return FadeInCurrentPlayingCoroutine(fadeInArgs, onFinish);
			}

			/// <summary>
			/// Fades out the currently-playing audio in this layer.
			/// </summary>
			/// <param name="fadeOutArgs">
			/// Details on how to fade-out, e.g. how long it should last.
			/// </param>
			/// <param name="except">
			/// Players <em>not</em> to fade out.
			/// </param>
			public void FadeOutCurrentPlaying(FadeOutArgs fadeOutArgs = null, params BackgroundAudio.Player[] except)
			{
				// Create a filter
				HashSet<BackgroundAudio.Player> ignorePlayers = null;
				if ((except != null) && (except.Length > 0))
				{
					ignorePlayers = new(except);
				}

				// Fade the current players out
				BackgroundAudio.Player[] fadingPlayers = MixerGroupManager.GetManagedPlayers();
				foreach (var fadingPlayer in fadingPlayers)
				{
					// Check if we should ignore this player
					if ((ignorePlayers != null) && (ignorePlayers.Contains(fadingPlayer)))
					{
						// If so, skip
						continue;
					}

					// Otherwise, check state
					if (fadingPlayer.State == BackgroundAudio.PlayState.Playing)
					{
						MixerGroupManager.FadeOut(fadingPlayer, fadeOutArgs);
					}
					else if (fadingPlayer.State == BackgroundAudio.PlayState.Scheduled)
					{
						fadingPlayer.Stop();
					}
				}
			}

			/// <summary>
			/// Fades in the currently-playing audio in this layer.
			/// </summary>
			/// <param name="fadeOutArgs">
			/// Details on how to fade-in, e.g. how long it should last.
			/// </param>
			public IEnumerator FadeInCurrentPlayingCoroutine(FadeInArgs fadeInArgs = null, System.Action<BackgroundAudio.Player> onFinish = null)
			{
				// Make sure there's a file in the history to play
				AssetRef<BackgroundAudio> audioFile = CurrentFile;
				if (audioFile.CurrentState == AssetRef.State.Null)
				{
					// There's no clip to play, finish immediately
					onFinish?.Invoke(null);
					yield break;
				}

				// Fade in the newest file in the history
				yield return FadeInCoroutine(audioFile, fadeInArgs, onFinish);
			}

			#region Helper Methods
			IEnumerator FadeInCoroutine(AssetRef<BackgroundAudio> audioClip, FadeInArgs fadeInArgs = null,
				System.Action<BackgroundAudio.Player> onFinish = null)
			{
				// TODO: there seems to be a bug with this on rapid-replay of the same clip before fading finishes: the two fading clips gets reverted to max volume.

				// Attempt to grab an existing player
				BackgroundAudio.Player audioPlayer = null;
				List<BackgroundAudio.Player> allPlayers = PlayerManager.GetPlayers(audioClip);
				if (allPlayers != null)
				{
					// Search through all players
					foreach (var player in allPlayers)
					{
						// Check if the player is already playing
						if (player.State == BackgroundAudio.PlayState.Playing)
						{
							// Check if we want to force a restart
							if ((fadeInArgs != null) && fadeInArgs.ForceRestart)
							{
								// Skip this player, then
								continue;
							}

							// If not, and there's a player that's already playing,
							// fade the players in to max volume (if it isn't already.)
							MixerGroupManager.FadeIn(player, fadeInArgs);

							// Halt early, don't return this player
							onFinish?.Invoke(null);
							yield break;
						}
						else
						{
							if (player.State == BackgroundAudio.PlayState.Scheduled)
							{
								// Stop the scheduled player
								player.Stop();
							}

							// Grab the last stopped player
							audioPlayer = player;
						}
					}
				}

				// If we couldn't find an existing player, create a new one
				if (audioPlayer == null)
				{
					// Create a new player, and retrieve the new one
					yield return PlayerManager.CreatePlayerCoroutine(audioClip, player => audioPlayer = player);
				}

				// Fade the players in
				MixerGroupManager.FadeIn(audioPlayer, fadeInArgs);

				// Invoke end event
				onFinish?.Invoke(audioPlayer);
			}
			#endregion
		}
	}
}