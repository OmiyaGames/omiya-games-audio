using UnityEngine;
using System.Collections.Generic;
using OmiyaGames.Global;
using OmiyaGames.Saves;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="SoundEffect.cs" company="Omiya Games">
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2014-2018 Omiya Games
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
	/// <strong>Date:</strong> 8/18/2015<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// <item>
	/// </item><term>
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 2/18/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Moved to new package.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// A script for playing sound effects, with extra options such as clip, pitch,
	/// and volume mutation. Also allows configuring sound effects' volume.
	/// </summary>
	/// <seealso cref="AudioSource"/>
	[RequireComponent(typeof(AudioSource))]
	public class SoundEffect : IAudio
	{
		public const float MinPitch = -3, MaxPitch = 3;
		public const float MinVolume = 0, MaxVolume = 1;

		/// <summary>
		/// A series of clips to play at random
		/// </summary>
		[Tooltip("A randomized list of clips to play. Note that the clip set on the AudioSource on start will be added to this list automatically.")]
		[SerializeField]
		RandomList<AudioClip> clipVariations = new();
		/// <summary>
		/// Whether this sound effect's pitch should be mutated
		/// </summary>
		[SerializeField]
		bool mutatePitch = false;
		/// <summary>
		/// The allowed range the pitch can mutate from the center pitch
		/// </summary>
		[SerializeField]
		Vector2 pitchMutationRange = new Vector2(0.5f, 1.5f);
		/// <summary>
		/// Whether this sound effect's volume should be mutated
		/// </summary>
		[SerializeField]
		bool mutateVolume = false;
		/// <summary>
		/// The allowed range the volume can mutate from the center pitch
		/// </summary>
		[SerializeField]
		Vector2 volumeMutationRange = new Vector2(0.5f, 1f);

		#region Local Properties
		/// <summary>
		/// TODO
		/// </summary>
		public float CenterVolume
		{
			get
			{
				float returnVolume = Audio.volume;
				if (mutateVolume == true)
				{
					returnVolume = (volumeMutationRange.x + volumeMutationRange.y) / 2f;
				}
				return returnVolume;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public float CenterPitch
		{
			get
			{
				float returnPitch = Audio.pitch;
				if (mutateVolume == true)
				{
					returnPitch = (pitchMutationRange.x + pitchMutationRange.y) / 2f;
				}
				return returnPitch;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public RandomList<AudioClip> ClipVariations => clipVariations;
		#endregion

		#region Unity Events
		protected override void Awake()
		{
			base.Awake();

			// Check to see if audio already has a clip
			if (Audio.clip != null)
			{
				// If so, check to see if it's already in the list
				int frequency = ClipVariations.GetFrequency(Audio.clip);
				if (frequency == 0)
				{
					// If not, add it in with default frequency
					ClipVariations.Add(Audio.clip);
				}
			}
			else if (ClipVariations.Count > 0)
			{
				// Add a random clip into the audio, if there aren't any
				Audio.clip = ClipVariations.NextRandomElement;
			}
		}
		#endregion

		protected override bool ChangeAudioSourceState(State before, State after)
		{
			// Check if we're playing this sound effect from Playing or Stopped state
			if ((after == State.Playing) && (before != State.Paused))
			{
				// Stop the audio
				Audio.Stop();

				// Pick a random clip
				if (ClipVariations.Count > 1)
				{
					Audio.clip = ClipVariations.NextRandomElement;
				}

				// Apply pitch mutation
				if (mutatePitch == true)
				{
					// Change the audio's pitch
					Audio.pitch = Random.Range(pitchMutationRange.x, pitchMutationRange.y);
				}

				// Update the volume
				if (mutateVolume == true)
				{
					// Change the audio's volume
					Audio.volume = Random.Range(volumeMutationRange.x, volumeMutationRange.y);
				}

				// Play the audio
				Audio.Play();
				return true;
			}

			// Otherwise, call base method
			return base.ChangeAudioSourceState(before, after);
		}
	}
}
