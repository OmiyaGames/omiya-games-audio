using System;
using System.Collections;
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
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 5/23/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// TODO.
	/// </summary>
	public class MixerGroupManager
	{
		/// <summary>
		/// TODO
		/// </summary>
		[Serializable]
		public struct Layer
		{
			[SerializeField]
			[Tooltip("The group to pipe the Audio Source for this layer.")]
			AudioMixerGroup group;
			[SerializeField]
			[Tooltip("The parameter name that adjusts the group's volume.")]
			string paramName;

			// FIXME: remove this constructor when MusicFader is deleted
			public Layer(in Collections.MusicFader.Layer layer)
			{
				group = layer.Group;
				paramName = layer.ParamName;
			}

			/// <summary>
			/// TODO
			/// </summary>
			public AudioMixerGroup Group => group;
			/// <summary>
			/// TODO
			/// </summary>
			public string ParamName => paramName;
		}

		class FadeSet
		{
			public FadeSet(in Layer layer)
			{
				Layer = layer;
			}

			public Layer Layer
			{
				get;
			}

			public BackgroundAudio.Player Player
			{
				get;
				set;
			} = null;

			public double StartTime
			{
				get;
				set;
			} = 0;

			public double FadeDuration
			{
				get;
				set;
			} = 0;

			public float VolumePercent
			{
				get;
				set;
			} = 0;

			public Coroutine FadeRoutine
			{
				get;
				set;
			} = null;
		}

		readonly AudioPlayerManager manager;
		readonly AnimationCurve percentToDbCurve;
		readonly FadeSet[] fader;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="percentToDbCurve"></param>
		/// <param name="fadeLayers"></param>
		public MixerGroupManager(AudioPlayerManager manager, AnimationCurve percentToDbCurve, params Layer[] fadeLayers)
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

			// FIXME: comment back this arg check in
			//foreach (var layer in fadeLayers)
			//{
			//	if (layer.Group == null)
			//	{
			//		throw new ArgumentNullException(nameof(fadeLayers), "All fade layers must have a group assigned");
			//	}
			//	if (string.IsNullOrEmpty(layer.ParamName))
			//	{
			//		throw new ArgumentException("All fade layers must have a parameter name assigned", nameof(fadeLayers));
			//	}
			//}

			this.manager = manager;
			this.percentToDbCurve = percentToDbCurve;

			fader = new FadeSet[fadeLayers.Length];
			for (int i = 0; i < fadeLayers.Length; i++)
			{
				fader[i] = new FadeSet(in fadeLayers[i]);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attach"></param>
		/// <param name="args"></param>
		public bool FadeIn(BackgroundAudio.Player player, FadeInArgs args) => FadeTo(player, args, 1);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="args"></param>
		/// <param name="finalVolumePercent"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
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
					Pause = true
				});
			}

			// Get or create a new player metadata
			FadeSet playerInfo = GetPlayerFadeInfo(player);
			if (playerInfo == null)
			{
				playerInfo = CreatePlayerFadeInfo(player);
				playerInfo.VolumePercent = 0;
			}

			// Check if volume is already at maximum
			if (Mathf.Approximately(playerInfo.VolumePercent, 1) || (playerInfo.VolumePercent > 1))
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
			playerInfo.StartTime = UnityEngine.AudioSettings.dspTime;
			playerInfo.FadeDuration = 0;

			// Apply args to info
			if (args != null)
			{
				playerInfo.StartTime += args.DelaySeconds;
				playerInfo.FadeDuration = args.DurationSeconds;
			}

			// Start playing the music
			player.Play(args);

			// Start the fade-in coroutine
			playerInfo.FadeRoutine = manager.StartCoroutine(FadeRoutine(playerInfo, 1, EndFadeOut));
			return true;

			void EndFadeOut()
			{
				// Reset the coroutine only
				playerInfo.FadeRoutine = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="args"></param>
		public bool FadeOut(BackgroundAudio.Player player, FadeOutArgs args)
		{
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Check if the current music is assigned
			FadeSet playerInfo = GetPlayerFadeInfo(player);
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
			playerInfo.StartTime = UnityEngine.AudioSettings.dspTime;
			playerInfo.FadeDuration = 0;

			// Apply args to info
			if (args != null)
			{
				playerInfo.StartTime += args.DelaySeconds;
				playerInfo.FadeDuration = args.DurationSeconds;
			}

			// Start the fade-out coroutine
			playerInfo.FadeRoutine = manager.StartCoroutine(FadeRoutine(playerInfo, 0, EndFadeOut));
			return true;

			void EndFadeOut()
			{
				// Pause or stop the player
				if (args.Pause)
				{
					playerInfo.Player.Pause();
				}
				else
				{
					playerInfo.Player.Stop();
				}

				// Reset the info
				playerInfo.Player = null;
				playerInfo.FadeRoutine = null;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public BackgroundAudio.Player[] GetManagedPlayers()
		{
			List<BackgroundAudio.Player> managedPlayers = new(fader.Length);
			foreach (var fadeLayer in fader)
			{
				if (fadeLayer.Player != null)
				{
					managedPlayers.Add(fadeLayer.Player);
				}
			}
			return managedPlayers.ToArray();
		}

		#region Helper Methods
		void SetVolume(Layer layer, float volumePercent)
		{
			// Grab all the necessary parameters
			AudioMixer mixer = layer.Group.audioMixer;
			string paramName = layer.ParamName;

			// Compute the volume
			float volumeDb = percentToDbCurve.Evaluate(volumePercent);

			// Update the mixer
			mixer.SetFloat(paramName, volumeDb);
		}

		FadeSet GetPlayerFadeInfo(BackgroundAudio.Player player)
		{
			// Go through each fader
			foreach (var fadeInfo in fader)
			{
				// Check if this fader has the same player as the argument
				if (fadeInfo.Player == player)
				{
					return fadeInfo;
				}
			}
			return null;
		}

		FadeSet CreatePlayerFadeInfo(BackgroundAudio.Player player)
		{
			// By default, return the first layer
			FadeSet returnInfo = fader[0];
			float largestProgressionPercent = PercentProgression(returnInfo);
			foreach (var fadeInfo in fader)
			{
				// Check if there's an info without player
				if (fadeInfo.Player == null)
				{
					// If so, return that
					return ConfigureFadeInfo(fadeInfo, player);
				}

				// Check if there's an info with a stopped or paused player
				switch (fadeInfo.Player.State)
				{
					case BackgroundAudio.PlayState.Stopped:
					case BackgroundAudio.PlayState.Paused:

						// If so, return that
						return ConfigureFadeInfo(fadeInfo, player);
				}

				// Otherwise, check how far the fader has progressed
				float compareProgression = PercentProgression(fadeInfo);
				if (compareProgression > largestProgressionPercent)
				{
					// If this info has progressed farther than the return candidate,
					// switch to returning this info.
					returnInfo = fadeInfo;
					largestProgressionPercent = compareProgression;
				}
			}
			return returnInfo;

			FadeSet ConfigureFadeInfo(FadeSet setupInfo, BackgroundAudio.Player player)
			{
				setupInfo.Player = player;
				player.MixerGroup = setupInfo.Layer.Group;
				return setupInfo;
			}

			float PercentProgression(FadeSet returnInfo)
			{
				// Check if fade has actually started
				if (returnInfo.StartTime > UnityEngine.AudioSettings.dspTime)
				{
					// If not, return 0 percent
					return 0;
				}

				// Check duration passed
				double progressionPercent = UnityEngine.AudioSettings.dspTime - returnInfo.StartTime;
				progressionPercent /= returnInfo.FadeDuration;

				// Clamp percentage to 1
				if (progressionPercent > 1)
				{
					progressionPercent = 1;
				}
				return (float)progressionPercent;
			}
		}

		IEnumerator FadeRoutine(FadeSet metaData, float finalVolumePercent, Action afterFadeFinished = null)
		{
			// See if the final volume is different from starting volume
			float startingVolumePercent = metaData.VolumePercent;
			if (Mathf.Approximately(startingVolumePercent, finalVolumePercent))
			{
				// If not, halt early
				afterFadeFinished?.Invoke();
				yield break;
			}

			// Set starting volume
			SetVolume(metaData.Layer, startingVolumePercent);

			// Wait until start time is met
			if (metaData.StartTime > UnityEngine.AudioSettings.dspTime)
			{
				yield return new WaitUntil(() => UnityEngine.AudioSettings.dspTime >= metaData.StartTime);
			}

			// Start the fade
			double currentDuration = 0;
			while (currentDuration < metaData.FadeDuration)
			{
				// Set the volume
				float timeProgressionPercent = (float)(currentDuration / metaData.FadeDuration);
				metaData.VolumePercent = Mathf.Lerp(startingVolumePercent, finalVolumePercent, timeProgressionPercent);
				SetVolume(metaData.Layer, metaData.VolumePercent);

				// Wait for a frame
				yield return null;

				// Calculate how much time has passed
				currentDuration = UnityEngine.AudioSettings.dspTime - metaData.StartTime;
			}

			// Set ending volume
			metaData.VolumePercent = finalVolumePercent;
			SetVolume(metaData.Layer, metaData.VolumePercent);

			// Run the action indicating fade has completed
			afterFadeFinished?.Invoke();
		}
		#endregion
	}
}
