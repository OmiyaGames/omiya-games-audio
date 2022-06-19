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
		/// <strong>Version:</strong> 1.0.0-pre.1<br/>
		/// <strong>Date:</strong> 2/27/2022<br/>
		/// <strong>Author:</strong> Taro Omiya
		/// </term>
		/// <description>Initial draft.</description>
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
			/// TODO
			/// </summary>
			public MixerGroupManager.Layer[] FadeLayers
			{
				get => fadeLayers;
				internal set => fadeLayers = value;
			}

			/// <summary>
			/// TODO
			/// </summary>
			public AudioPlayerManager PlayerManager
			{
				get;
				internal set;
			} = null;

			/// <summary>
			/// TODO
			/// </summary>
			public MixerGroupManager GroupManager
			{
				get;
				internal set;
			}

			/// <summary>
			/// TODO
			/// </summary>
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
			public AssetRef<BackgroundAudio> CurrentFile
			{
				get
				{
					AssetRef<BackgroundAudio>? latestFile = History.Newest;
					if (latestFile.HasValue)
					{
						return latestFile.Value;
					}
					else
					{
						return AssetRef<BackgroundAudio>.NULL;
					}
				}
			}

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
						BackgroundAudio.Player[] fadingPlayers = GroupManager.GetManagedPlayers();
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
			public List<BackgroundAudio.Player> GetCurrentPlayers(AudioPlayerManager.AudioState state = AudioPlayerManager.AudioState.All)
			{
				// Grab the currently managed players
				BackgroundAudio.Player[] fadingPlayers = GroupManager.GetManagedPlayers();
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
			public BackgroundAudio.Player GetCurrentPlayer(AudioPlayerManager.AudioState state = AudioPlayerManager.AudioState.All)
			{
				// Check if current file is valid
				AssetRef<BackgroundAudio> currentFile = CurrentFile;
				if (currentFile.CurrentState != AssetRef.State.Null)
				{
					// Grab the currently managed players
					BackgroundAudio.Player[] fadingPlayers = GroupManager.GetManagedPlayers();
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
			/// <param name="audioClip"></param>
			/// <param name="fadeInArgs"></param>
			/// <param name="fadeOutArgs"></param>
			/// <param name="onFinish">
			/// Triggers as soon as this coroutine finishes, with a reference
			/// to the player that will be faded in. The parameter will be null
			/// if <paramref name="audioClip"/> isn't played (e.g. when it's
			/// already playing in the background.)
			/// </param>
			/// <returns></returns>
			/// <exception cref="System.ArgumentNullException"></exception>
			public IEnumerator PlayNextCoroutine(AssetRef<BackgroundAudio> audioClip,
				FadeInArgs fadeInArgs = null, FadeOutArgs fadeOutArgs = null,
				System.Action<BackgroundAudio.Player> onFinish = null)
			{
				// Null-check first
				if (audioClip.CurrentState == AssetRef.State.Null)
				{
					throw new System.ArgumentNullException(nameof(audioClip));
				}

				// Attempt to grab an existing player
				List<BackgroundAudio.Player> allPlayers = PlayerManager.GetPlayers(audioClip);
				BackgroundAudio.Player newPlayer = null;
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
							// don't bother playing this clip.
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
							newPlayer = player;
						}
					}
				}

				// If we couldn't find an existing player, create a new one
				if (newPlayer == null)
				{
					// Create a new player, and retrieve the new one
					yield return PlayerManager.CreatePlayerCoroutine(audioClip, player => newPlayer = player);
				}

				// Fade the currently playing players out
				FadeOutCurrentPlaying(fadeOutArgs);

				// Fade the players in
				GroupManager.FadeIn(newPlayer, fadeInArgs);

				// Add to history
				History.Add(audioClip);

				// Invoke end event
				onFinish?.Invoke(newPlayer);
			}

			/// <summary>
			/// Coroutine to fade in the previous background audio in <see cref="History"/>,
			/// if any.  Also removes the newest clip from the history, if played.
			/// </summary>
			/// <param name="fadeInArgs"></param>
			/// <param name="fadeOutArgs"></param>
			/// <param name="onFinish"></param>
			/// <returns></returns>
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

				// Make sure the next music is not null
				AssetRef<BackgroundAudio> poppedMusic = history.Newest;
				if (poppedMusic.CurrentState == AssetRef.State.Null)
				{
					// Clip is null, finish immediately
					onFinish?.Invoke(null);
					yield break;
				}

				// Fade in the next newest music
				yield return PlayNextCoroutine(poppedMusic, fadeInArgs, fadeOutArgs, onFinish);
			}

			/// <summary>
			/// Fades out the currently-playing audio in this layer.
			/// </summary>
			/// <param name="fadeOutArgs">
			/// Arguments for fade-out, e.g. duration.
			/// </param>
			public void FadeOutCurrentPlaying(FadeOutArgs fadeOutArgs = null)
			{
				// Fade the current players out
				BackgroundAudio.Player[] fadingPlayers = GroupManager.GetManagedPlayers();
				foreach (var fadingPlayer in fadingPlayers)
				{
					if (fadingPlayer.State == BackgroundAudio.PlayState.Playing)
					{
						GroupManager.FadeOut(fadingPlayer, fadeOutArgs);
					}
					else if (fadingPlayer.State == BackgroundAudio.PlayState.Scheduled)
					{
						fadingPlayer.Stop();
					}
				}
			}
		}
	}
}
