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
	[AddComponentMenu("Audio/Sound Effect", 100)]
	public class SoundEffect : IAudio
	{
		public const float MinPitch = -3, MaxPitch = 3;
		public const float MinVolume = 0, MaxVolume = 1;

		[Tooltip("A randomized list of clips to play. Note that the clip set on the AudioSource on start will be added to this list automatically.")]
		[SerializeField]
		RandomList<AudioClip> clipVariations = new();
		[SerializeField]
		bool mutatePitch = false;
		[SerializeField]
		Vector2 pitchMutationRange = new Vector2(0.6f, 1.4f);
		[SerializeField]
		bool mutateVolume = false;
		[SerializeField]
		Vector2 volumeMutationRange = new Vector2(0.8f, 1f);
		[SerializeField]
		bool isPausedOnTimeStop = true;

		/// <summary>
		/// TODO
		/// </summary>
		public float CenterVolume
		{
			get
			{
				float returnVolume = CurrentAudio.volume;
				if (IsMutatingVolume == true)
				{
					returnVolume = (VolumeMutationRange.x + VolumeMutationRange.y) / 2f;
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
				float returnPitch = CurrentAudio.pitch;
				if (IsMutatingVolume == true)
				{
					returnPitch = (PitchMutationRange.x + PitchMutationRange.y) / 2f;
				}
				return returnPitch;
			}
		}
		/// <summary>
		/// A series of clips to play at random
		/// </summary>
		public RandomList<AudioClip> ClipVariations => clipVariations;
		/// <summary>
		/// Whether this sound effect's pitch should be mutated
		/// </summary>
		public bool IsMutatingPitch
		{
			get => mutatePitch;
			set => mutatePitch = value;
		}
		/// <summary>
		/// Whether this sound effect's volume should be mutated
		/// </summary>
		public bool IsMutatingVolume
		{
			get => mutateVolume;
			set => mutateVolume = value;
		}
		/// <summary>
		/// The allowed range the pitch can mutate from the center pitch
		/// </summary>
		public Vector2 PitchMutationRange
		{
			get => pitchMutationRange;
			set => pitchMutationRange = value;
		}
		/// <summary>
		/// The allowed range the volume can mutate from the center pitch
		/// </summary>
		public Vector2 VolumeMutationRange
		{
			get => volumeMutationRange;
			set => volumeMutationRange = value;
		}
		/// <inheritdoc/>
		public override bool IsPausedOnTimeStop
		{
			get => isPausedOnTimeStop;
			set
			{
				isPausedOnTimeStop = value;
				CurrentAudio.ignoreListenerPause = !isPausedOnTimeStop;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			// Check to see if audio already has a clip
			if (CurrentAudio.clip != null)
			{
				// If so, check to see if it's already in the list
				int frequency = ClipVariations.GetFrequency(CurrentAudio.clip);
				if (frequency == 0)
				{
					// If not, add it in with default frequency
					ClipVariations.Add(CurrentAudio.clip);
				}
			}
			else if (ClipVariations.Count > 0)
			{
				// Add a random clip into the audio, if there aren't any
				CurrentAudio.clip = ClipVariations.NextRandomElement;
			}
		}

		protected override bool ChangeAudioSourceState(State before, State after)
		{
			// Check if we're playing this sound effect from Playing or Stopped state
			if ((after == State.Playing) && (before != State.Paused))
			{
				// Stop the audio
				CurrentAudio.Stop();

				// Pick a random clip
				if (ClipVariations.Count > 1)
				{
					CurrentAudio.clip = ClipVariations.NextRandomElement;
				}

				// Apply pitch mutation
				if (IsMutatingPitch == true)
				{
					// Change the audio's pitch
					CurrentAudio.pitch = Random.Range(PitchMutationRange.x, PitchMutationRange.y);
				}

				// Update the volume
				if (IsMutatingVolume == true)
				{
					// Change the audio's volume
					CurrentAudio.volume = Random.Range(VolumeMutationRange.x, VolumeMutationRange.y);
				}

				// Play the audio
				CurrentAudio.Play();
				return true;
			}

			// Otherwise, call base method
			return base.ChangeAudioSourceState(before, after);
		}
	}
}
