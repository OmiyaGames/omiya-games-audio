using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="MixerGroupFader.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/26/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Unity inspector class for pairing a
	/// <seealso cref="AudioMixerGroup"/> with a parameter name
	/// for changing its volume.
	/// </summary>
	[Serializable]
	public class MixerGroupFader
	{
		[SerializeField]
		[Tooltip("The group to pipe the Audio Source for this layer.")]
		AudioMixerGroup group;
		[SerializeField]
		[Tooltip("The parameter name that adjusts the group's volume.")]
		string paramName;

		float volumePercent = 0;
		double lastStartTime = 0, lastFadeDuration = 0;
		AnimationCurve percentToDbCurve = null;
		BackgroundAudio.Player player = null;
		Action<BackgroundAudio.Player> beforePlayerDestroy = null;

		/// <summary>
		/// Gets this layer's <see cref="AudioMixerGroup"/>.
		/// </summary>
		public AudioMixerGroup Group => group;
		/// <summary>
		/// Gets the <seealso cref="Group"/>'s parameter name
		/// to adjust its volume.
		/// </summary>
		public string ParamName => paramName;
		/// <summary>
		/// Sets the volume of <seealso cref="Group"/>.
		/// </summary>
		public float VolumePercent
		{
			get => volumePercent;
			set
			{
				// Compute the volume
				volumePercent = Mathf.Clamp01(value);
				if (percentToDbCurve != null)
				{
					float volumeDb = percentToDbCurve.Evaluate(volumePercent);

					// Update the mixer
					Group.audioMixer.SetFloat(ParamName, volumeDb);
				}
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public BackgroundAudio.Player Player
		{
			get => player;
			set
			{
				// Make sure player is different
				if (player == value)
				{
					// If not, don't do anything
					return;
				}

				// Check if there were a player set here before
				if (player != null)
				{
					// Stop the player
					player.Stop();

					// Clean the events
					if (beforePlayerDestroy != null)
					{
						Player.OnBeforeDestroy -= beforePlayerDestroy;
						beforePlayerDestroy = null;
					}
				}

				// Update the member variable
				player = value;

				// Check if we need to setup the player
				if (player != null)
				{
					// Setup the player's mixer group
					player.MixerGroup = Group;

					// Setup the clean-up action if player gets destroyed externally
					beforePlayerDestroy = new Action<BackgroundAudio.Player>(player =>
					{
						// Make sure the stored player is the same one being destroyed
						if (Player == player)
						{
							Player = null;
						}
					});
					player.OnBeforeDestroy += beforePlayerDestroy;
				}
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public Coroutine FadeRoutine
		{
			get;
			set;
		} = null;

		/// <summary>
		/// Setup the member variables of this layer
		/// </summary>
		/// <param name="percentToDbCurve">
		/// The curve used to convert a fraction from <c>0</c> to <c>1</c>
		/// to decibels.
		/// </param>
		public void Setup(AnimationCurve percentToDbCurve)
		{
			this.percentToDbCurve = percentToDbCurve;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="startTime"></param>
		/// <param name="fadeDuration"></param>
		/// <param name="finalVolumePercent"></param>
		/// <param name="afterFadeFinished"></param>
		/// <returns>
		/// The volume fading coroutine
		/// </returns>
		public IEnumerator FadeVolumeCoroutine(double startTime, double fadeDuration, float finalVolumePercent, Action<MixerGroupFader> afterFadeFinished = null)
		{
			// See if the final volume is different from starting volume
			float startingVolumePercent = VolumePercent;
			if (Mathf.Approximately(startingVolumePercent, finalVolumePercent))
			{
				// If not, halt early
				afterFadeFinished?.Invoke(this);
				yield break;
			}

			// Setup member variables
			lastStartTime = startTime;
			lastFadeDuration = fadeDuration;

			// Wait until start time is met
			if (startTime > UnityEngine.AudioSettings.dspTime)
			{
				yield return new WaitUntil(() => UnityEngine.AudioSettings.dspTime >= startTime);
			}

			// Start the fade
			double currentDuration = 0;
			while (currentDuration < fadeDuration)
			{
				// Set the volume
				float timeProgressionPercent = (float)(currentDuration / fadeDuration);
				VolumePercent = Mathf.Lerp(startingVolumePercent, finalVolumePercent, timeProgressionPercent);

				// Wait for a frame
				yield return null;

				// Calculate how much time has passed
				currentDuration = UnityEngine.AudioSettings.dspTime - startTime;
			}

			// Set ending volume
			VolumePercent = finalVolumePercent;

			// Run the action indicating fade has completed
			afterFadeFinished?.Invoke(this);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="returnInfo"></param>
		/// <returns></returns>
		public float GetFadeProgressionPercent()
		{
			if (lastStartTime > UnityEngine.AudioSettings.dspTime)
			{
				// If fade hasn't actually started yet, return 0 percent
				return 0;
			}
			else if (lastFadeDuration.CompareTo(0) <= 0)
			{
				// If fade has started, but duration is 0 or less, return 100 percent
				return 1;
			}

			// Check duration passed
			double progressionPercent = UnityEngine.AudioSettings.dspTime - lastStartTime;
			progressionPercent /= lastFadeDuration;

			// Clamp percentage to 1
			if (progressionPercent > 1)
			{
				progressionPercent = 1;
			}
			return (float)progressionPercent;
		}
	}
}
