using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="MixerGroupManager.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 5/23/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Manager for <seealso cref="AudioMixerGroup"/>, used to fade in and out
	/// various <seealso cref="BackgroundAudio.Player"/>s.
	/// </summary>
	public class MixerGroupManager
	{
		readonly AudioPlayerManager manager;
		readonly MixerGroupFader[] fadeLayers;

		/// <summary>
		/// Constructs a new manager.
		/// </summary>
		/// <param name="manager">
		/// The manager for <seealso cref="BackgroundAudio.Player"/>s.
		/// </param>
		/// <param name="percentToDbCurve">
		/// Curve used to convert a fraction from <c>0</c> to <c>1</c>
		/// to decibels.
		/// </param>
		/// <param name="fadeLayers">
		/// Pairs of <see cref="AudioMixerGroup"/> and parameter name for the group's volume.
		/// </param>
		public MixerGroupManager(AudioPlayerManager manager, AnimationCurve percentToDbCurve, MixerGroupFader[] fadeLayers)
		{
			// Null check
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}
			else if (percentToDbCurve == null)
			{
				throw new ArgumentNullException(nameof(percentToDbCurve));
			}
			else if (fadeLayers == null)
			{
				throw new ArgumentNullException(nameof(fadeLayers));
			}
			else if (fadeLayers.Length < 1)
			{
				throw new ArgumentException("There must be at least one valid fade layer", nameof(fadeLayers));
			}

			foreach (var layer in fadeLayers)
			{
				if (layer.Group == null)
				{
					throw new ArgumentNullException(nameof(fadeLayers), "All fade layers must have a group assigned");
				}
				if (string.IsNullOrEmpty(layer.ParamName))
				{
					throw new ArgumentException("All fade layers must have a parameter name assigned", nameof(fadeLayers));
				}

				// Setup all layers
				layer.Setup(percentToDbCurve);
			}

			this.manager = manager;
			this.fadeLayers = fadeLayers;
		}

		/// <summary>
		/// Gets the <see cref="AudioMixerGroup"/> at a specified index.
		/// </summary>
		/// <param name="layerIndex">
		/// Index corresponding to the list in Unity Project Settings
		/// dialog (starting at <c>0</c>.)
		/// </param>
		/// <returns>
		/// The corresponding <see cref="AudioMixerGroup"/>.
		/// </returns>
		public AudioMixerGroup GetMixerGroup(int layerIndex) => fadeLayers[layerIndex].Group;

		/// <summary>
		/// Gets the volume for a <see cref="AudioMixerGroup"/>.
		/// </summary>
		/// <param name="layerIndex">
		/// Index corresponding to the list in Unity Project Settings
		/// dialog (starting at <c>0</c>.)
		/// </param>
		/// <returns>
		/// The volume, as a fraction between <c>0</c> and <c>1</c>.
		/// </returns>
		public float GetVolume(int layerIndex) => fadeLayers[layerIndex].VolumePercent;

		/// <summary>
		/// Sets the volume for a <see cref="AudioMixerGroup"/>.
		/// </summary>
		/// <param name="layerIndex">
		/// Index corresponding to the list in Unity Project Settings
		/// dialog (starting at <c>0</c>.)
		/// </param>
		/// <param name="volumePercent">
		/// The volume, as a fraction between <c>0</c> and <c>1</c>.
		/// </param>
		public void SetVolume(int layerIndex, float volumePercent) => fadeLayers[layerIndex].VolumePercent = volumePercent;

		/// <summary>
		/// Starts playing the <paramref name="player"/>, and fading
		/// it in to full volume.
		/// </summary>
		/// <param name="player">
		/// The <see cref="BackgroundAudio.Player"/> to start playing,
		/// and/or fading in.
		/// </param>
		/// <param name="args">
		/// Details on how to fade, e.g. how long it should last, etc.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if fading has been performed; false otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="player"/> is <see langword="null"/>.
		/// </exception>
		public bool FadeIn(BackgroundAudio.Player player, FadeInArgs args) => FadeTo(player, args, 1);

		/// <summary>
		/// Starts playing the <paramref name="player"/>, and fading
		/// it in to the specified volume.
		/// </summary>
		/// <param name="player">
		/// The <see cref="BackgroundAudio.Player"/> to start playing,
		/// and/or fading in.
		/// </param>
		/// <param name="args">
		/// Details on how to fade, e.g. how long it should last, etc.
		/// </param>
		/// <param name="finalVolumePercent">
		/// The final volume at the end of the fade, as a fraction
		/// between <c>0</c> and <c>1</c>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if fading has been performed; false otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="player"/> is <see langword="null"/>.
		/// </exception>
		public bool FadeTo(BackgroundAudio.Player player, FadeInArgs args, float finalVolumePercent)
		{
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Clamp volume
			finalVolumePercent = Mathf.Clamp01(finalVolumePercent);
			if (Mathf.Approximately(finalVolumePercent, 0f))
			{
				return FadeOut(player, new FadeOutArgs()
				{
					DelaySeconds = args.DelaySeconds,
					DurationSeconds = args.DurationSeconds,
				});
			}

			// Get or create a new player metadata
			MixerGroupFader playerInfo = GetPlayerFadeInfo(player);
			if (playerInfo == null)
			{
				playerInfo = CreatePlayerFadeInfo(player);
				playerInfo.VolumePercent = 0;
			}

			// Check if volume is already at final volume value
			if (Mathf.Approximately(playerInfo.VolumePercent, finalVolumePercent))
			{
				// Don't bother fading in
				return false;
			}

			// Stop the fade-in coroutine, if one is running
			if (playerInfo.FadeRoutine != null)
			{
				manager.StopCoroutine(playerInfo.FadeRoutine);
			}

			// Calculate time
			double startTime = UnityEngine.AudioSettings.dspTime, fadeDuration = 0;
			if (args != null)
			{
				startTime += args.DelaySeconds;
				fadeDuration = args.DurationSeconds;
			}

			// Start playing the music
			playerInfo.Player.Play(args);

			// Start the fade-in coroutine
			playerInfo.FadeRoutine = manager.StartCoroutine(playerInfo.FadeVolumeCoroutine(startTime, fadeDuration, finalVolumePercent, EndFadeOut));
			return true;

			void EndFadeOut(MixerGroupFader playerInfo)
			{
				// Reset the coroutine only
				playerInfo.FadeRoutine = null;
			}
		}

		/// <summary>
		/// Fades out a <paramref name="player"/>, and optinally stop playing
		/// once it's silent.
		/// </summary>
		/// <param name="player">
		/// The <see cref="BackgroundAudio.Player"/> to fade out, and/or stop.
		/// </param>
		/// <param name="args">
		/// Details on how to fade out, e.g. how long it should last, etc.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if fading has been performed; false otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="player"/> is <see langword="null"/>.
		/// </exception>
		public bool FadeOut(BackgroundAudio.Player player, FadeOutArgs args)
		{
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Check if the current music is assigned
			MixerGroupFader playerInfo = GetPlayerFadeInfo(player);
			if (playerInfo == null)
			{
				return false;
			}

			// Stop the fade-in coroutine, if one is running
			if (playerInfo.FadeRoutine != null)
			{
				manager.StopCoroutine(playerInfo.FadeRoutine);
			}

			// Calculate time
			double startTime = UnityEngine.AudioSettings.dspTime, fadeDuration = 0;
			if (args != null)
			{
				startTime += args.DelaySeconds;
				fadeDuration = args.DurationSeconds;
			}

			// Start the fade-out coroutine
			playerInfo.FadeRoutine = manager.StartCoroutine(playerInfo.FadeVolumeCoroutine(startTime, fadeDuration, 0, EndFadeOut));
			return true;

			void EndFadeOut(MixerGroupFader playerInfo)
			{
				// Reset the info
				playerInfo.Player = null;
				playerInfo.FadeRoutine = null;
			}
		}

		/// <summary>
		/// Gets a list of <see cref="BackgroundAudio.Player"/>s that's
		/// playing on a <see cref="AudioMixerGroup"/> managed by this manager.
		/// </summary>
		/// <returns>
		/// The list of <see cref="BackgroundAudio.Player"/>s managed by
		/// this instance.
		/// </returns>
		public BackgroundAudio.Player[] GetManagedPlayers()
		{
			List<BackgroundAudio.Player> managedPlayers = new(fadeLayers.Length);
			foreach (var fadeLayer in fadeLayers)
			{
				if (fadeLayer.Player != null)
				{
					managedPlayers.Add(fadeLayer.Player);
				}
			}
			return managedPlayers.ToArray();
		}

		#region Helper Methods
		MixerGroupFader GetPlayerFadeInfo(BackgroundAudio.Player player)
		{
			// Go through each fader
			foreach (var fadeInfo in fadeLayers)
			{
				// Check if this fader has the same player as the argument
				if (fadeInfo.Player == player)
				{
					return fadeInfo;
				}
			}
			return null;
		}

		MixerGroupFader CreatePlayerFadeInfo(BackgroundAudio.Player player)
		{
			// By default, return the first layer
			MixerGroupFader returnInfo = fadeLayers[0];
			float largestProgressionPercent = returnInfo.GetFadeProgressionPercent();
			foreach (var fadeInfo in fadeLayers)
			{
				// Check if there's an info without player
				if (fadeInfo.Player == null)
				{
					// If so, return that
					fadeInfo.Player = player;
					return fadeInfo;
				}

				// Check if there's an info with a stopped or paused player
				switch (fadeInfo.Player.State)
				{
					case BackgroundAudio.PlayState.Stopped:

						// If so, return that
						fadeInfo.Player = player;
						return fadeInfo;
				}

				// Otherwise, check how far the fader has progressed
				float compareProgression = fadeInfo.GetFadeProgressionPercent();
				if (compareProgression > largestProgressionPercent)
				{
					// If this info has progressed farther than the return candidate,
					// switch to returning this info.
					returnInfo = fadeInfo;
					largestProgressionPercent = compareProgression;
				}
			}
			return returnInfo;
		}
		#endregion
	}
}
