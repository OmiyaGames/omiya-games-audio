using UnityEngine;
using System.Collections.Generic;

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
	/// <strong>Version:</strong> 0.1.0-exp.1<br/>
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
	public class SoundEffect : MonoBehaviour
	{
		public enum Layer
		{
			All,
			Latest
		}

		public const float MIN_PITCH = -3, MAX_PITCH = 3;
		public const float MIN_VOLUME = 0, MAX_VOLUME = 1;
		public const int MIN_LAYERS = 1, MAX_LAYERS = 10;

		[HideInInspector]
		AudioSource audioCache = null;
		[Tooltip("A randomized list of clips to play. Note that the clip set on the AudioSource on start will be added to this list automatically.")]
		[SerializeField]
		RandomList<AudioClip> clipVariations = new();

		[Header("Mutate")]
		[SerializeField]
		bool mutatePitch = false;
		[SerializeField]
		Vector2 pitchMutationRange = new Vector2(0.6f, 1.4f);
		[SerializeField]
		bool mutateVolume = false;
		[SerializeField]
		Vector2 volumeMutationRange = new Vector2(0.8f, 1f);

		[Header("Other Settings")]
		[SerializeField]
		int maxNumLayers = 3;
		[SerializeField]
		bool isPausedOnTimeStop = true;

		LinkedList<AudioSource> allAudioLayers = null;

		/// <inheritdoc/>
		public bool IsPausedOnTimeStop
		{
			get => isPausedOnTimeStop;
			set
			{
				if (isPausedOnTimeStop != value)
				{
					isPausedOnTimeStop = value;
					foreach (AudioSource layer in AllAudioLayers)
					{
						layer.ignoreListenerPause = !isPausedOnTimeStop;
					}
				}
			}
		}
		/// <summary>
		/// The attached audio source that played the latest SFX.
		/// </summary>
		public AudioSource LatestAudio => AllAudioLayers.Last.Value;
		/// <summary>
		/// All attached audio sources, sorted with
		/// oldest source that played a SFX as first node.
		/// </summary>
		public IReadOnlyCollection<AudioSource> AllAudios => AllAudioLayers;
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
			set
			{
				if (mutatePitch != value)
				{
					mutatePitch = value;
					if (mutatePitch)
					{
						OriginalPitch = LatestAudio.pitch;
					}
					else
					{
						foreach (AudioSource layer in AllAudioLayers)
						{
							layer.pitch = OriginalPitch;
						}
					}
				}
			}
		}
		/// <summary>
		/// Whether this sound effect's volume should be mutated
		/// </summary>
		public bool IsMutatingVolume
		{
			get => mutateVolume;
			set
			{
				if (mutateVolume != value)
				{
					mutateVolume = value;
					if (mutateVolume)
					{
						OriginalVolume = LatestAudio.volume;
					}
					else
					{
						foreach (AudioSource layer in AllAudioLayers)
						{
							layer.volume = OriginalVolume;
						}
					}
				}
			}
		}
		/// <summary>
		/// The allowed range the pitch can mutate from the center pitch
		/// </summary>
		public Vector2 PitchMutationRange
		{
			get => pitchMutationRange;
			set => pitchMutationRange = ClampRange(value, MIN_PITCH, MAX_PITCH);
		}
		/// <summary>
		/// The allowed range the volume can mutate from the center pitch
		/// </summary>
		public Vector2 VolumeMutationRange
		{
			get => volumeMutationRange;
			set => volumeMutationRange = ClampRange(value, MIN_VOLUME, MAX_VOLUME);
		}
		/// <summary>
		/// The number of one-shot sounds this component will allow to overlap.
		/// </summary>
		public int NumberOfLayers
		{
			get => maxNumLayers;
			set
			{
				value = Mathf.Clamp(value, MIN_LAYERS, MAX_LAYERS);
				if (maxNumLayers != value)
				{
					// Set the value
					maxNumLayers = value;

					// Check if we need to prune the layers
					LinkedListNode<AudioSource> seekStoppedLayer = AllAudioLayers.First;
					while (AllAudioLayers.Count > maxNumLayers)
					{
						// Look for an audio layer that is stopped
						LinkedListNode<AudioSource> removeNode = FindFirstStoppedLayer(seekStoppedLayer);
						if (removeNode != null)
						{
							// If one is found, move the seek node to the next one
							// (for the next loop.)
							seekStoppedLayer = removeNode.Next;
						}
						else
						{
							// If there isn't any, mark the oldest audio source that
							// isn't attached to this script for removal.
							removeNode = AllAudioLayers.First;
						}

						// Destroy the layer
						Destroy(removeNode.Value);
						AllAudioLayers.Remove(removeNode);
					}
				}
			}
		}
		/// <summary>
		/// The original audio source's pitch,
		/// before mutation was applied.
		/// </summary>
		public float OriginalPitch
		{
			get;
			private set;
		} = 1;
		/// <summary>
		/// The original audio source's volume,
		/// before mutation was applied.
		/// </summary>
		public float OriginalVolume
		{
			get;
			private set;
		} = 1;

		/// <summary>
		/// All attached audio sources, sorted with
		/// oldest source that played a SFX as first node.
		/// </summary>
		/// <remarks>
		/// This is a write-able <see cref="LinkedList{T}"/> version of
		/// <see cref="AllAudios"/>.
		/// </remarks>
		protected LinkedList<AudioSource> AllAudioLayers
		{
			get
			{
				// Check to see if the audio layers are setup
				if (allAudioLayers == null)
				{
					// Grab the attached audio source, and perform a setup
					Setup(AttachedSource);

					// Create the list, and add the first element
					allAudioLayers = new LinkedList<AudioSource>();
					allAudioLayers.AddLast(AttachedSource);
				}
				return allAudioLayers;
			}
		}
		protected AudioSource AttachedSource => Helpers.GetComponentCached(this, ref audioCache);

		/// <summary>
		/// <para>
		/// Plays an audio on the latest layer, based on whether
		/// <see cref="LatestAudio"/>'s <see cref="AudioSource.loop"/> flag is
		/// set to <c>true</c>.
		/// </para><para>
		/// Note: this method does <em>not</em> resume a previously
		/// paused sound effect.
		/// </para>
		/// </summary>
		/// <remarks>
		/// <para>
		/// If <see cref="LatestAudio"/>'s <see cref="AudioSource.loop"/> flag is set to
		/// <c>true</c>, then this method checks to see the audio's state.
		/// If stopped, a random clip will play on loop, with mutation applied to
		/// pitch and volume if flagged to do so.
		/// If paused, it resumes the paused clip, with pitch and volume unchanged.
		/// Otherwise, this method does nothing.
		/// </para><para>
		/// If <see cref="LatestAudio"/>'s <see cref="AudioSource.loop"/> flag is <em>not</em>
		/// set to <c>true</c>, then this method acts like
		/// <see cref="AudioSource.PlayOneShot(AudioClip)"/>.
		/// While this method attempts to play a sound effect on an audio layer that
		/// isn't playing anything, if all of them are playing or paused,
		/// this method will stop the audio layer that played the oldest clip,
		/// and play a new random clip instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Stop(Layer)"/>
		/// <seealso cref="Pause(Layer)"/>
		public void Play()
		{
			// Check to see if we're supposed to be looping.
			AudioSource playAudio = LatestAudio;
			if (playAudio.loop)
			{
				// First, attempt to unpause the latest audio.
				// If paused, this updates the isPlaying flag to true.
				playAudio.UnPause();

				// Check if the audio is already playing
				// or resuming from a previous pause.
				if (playAudio.isPlaying == true)
				{
					// If so, halt
					return;
				}
			}
			else
			{
				// Look through all the audio layers first, and find one that has stopped.
				LinkedListNode<AudioSource> checkLayer = FindFirstStoppedLayer(AllAudioLayers.First);

				// Check if there's an audio layer that has stopped
				if (checkLayer != null)
				{
					// Set the audio to play from
					playAudio = checkLayer.Value;

					// Shuffle the node to the end (making it the latest audio source)
					AllAudioLayers.Remove(checkLayer);
					AllAudioLayers.AddLast(checkLayer);
				}
				else if (AllAudioLayers.Count < NumberOfLayers)
				{
					// If all current layers are still playing/paused,
					// BUT the current number of layers is below the max limit of layers,
					// create a new audio layer
					playAudio = Helpers.CloneComponent(AttachedSource);
					AllAudioLayers.AddLast(playAudio);
				}
				else
				{
					// If ALL layers are still playing/paused,
					// and we can't make new audio layers,
					// grab the first (i.e. oldest) layer
					checkLayer = AllAudioLayers.First;
					playAudio = checkLayer.Value;

					// Shuffle the node to the end (making it the latest audio source)
					AllAudioLayers.RemoveFirst();
					AllAudioLayers.AddLast(checkLayer);
				}
			}

			// Stop the audio before changing the clip
			playAudio.Stop();

			// Pick a random clip
			if (ClipVariations.Count > 0)
			{
				playAudio.clip = ClipVariations.NextRandomElement;
			}

			// Apply pitch mutation
			if (IsMutatingPitch == true)
			{
				// Change the audio's pitch
				playAudio.pitch = Random.Range(PitchMutationRange.x, PitchMutationRange.y);
			}

			// Update the volume
			if (IsMutatingVolume == true)
			{
				// Change the audio's volume
				playAudio.volume = Random.Range(VolumeMutationRange.x, VolumeMutationRange.y);
			}

			// Play the audio back from the start
			playAudio.Play();
		}

		/// <summary>
		/// Stops the audio, and rewind to the beginning.
		/// </summary>
		/// <param name="layerToStop">
		/// Which layer(s) to stop.
		/// </param>
		/// <seealso cref="Play()"/>
		public void Stop(Layer layerToStop = Layer.All)
		{
			if (layerToStop == Layer.Latest)
			{
				LatestAudio.Stop();
				return;
			}

			foreach (AudioSource source in AllAudioLayers)
			{
				source.Stop();
			}
		}

		/// <summary>
		/// Pauses the audio, which can be resumed later.
		/// </summary>
		/// <param name="layerToStop">
		/// Which layer(s) to pause.
		/// </param>
		/// <seealso cref="Play()"/>
		/// <seealso cref="UnPause(Layer)"/>
		public void Pause(Layer layerToPause = Layer.All)
		{
			if (layerToPause == Layer.Latest)
			{
				LatestAudio.Pause();
				return;
			}

			foreach (AudioSource source in AllAudioLayers)
			{
				source.Pause();
			}
		}

		/// <summary>
		/// Resumes the audio <em>if</em> it was paused earlier.
		/// </summary>
		/// <param name="layerToStop">
		/// Which layer(s) to un-pause.
		/// </param>
		/// <seealso cref="Pause(Layer)"/>
		public void UnPause(Layer layerToUnPause = Layer.All)
		{
			if (layerToUnPause == Layer.Latest)
			{
				LatestAudio.UnPause();
				return;
			}

			foreach (AudioSource source in AllAudioLayers)
			{
				source.UnPause();
			}
		}

		/// <summary>
		/// Resumes the audio <em>if</em> it was paused earlier.
		/// </summary>
		/// <remarks>
		/// This method is an alias to <seealso cref="UnPause(Layer)"/>.
		/// </remarks>
		/// <param name="layerToStop">
		/// Which layer(s) to un-pause.
		/// </param>
		/// <seealso cref="Pause(Layer)"/>
		public void Resume(Layer layerToResume = Layer.All) => UnPause(layerToResume);

		void Awake()
		{
			// Check if this audio *should* be playing on awake
			if (LatestAudio.playOnAwake)
			{
				// Override the audio-source behavior
				Play();
			}
		}

		void Setup(AudioSource attachedSource)
		{
			// Update attached audio source with listener settings
			attachedSource.ignoreListenerPause = !IsPausedOnTimeStop;

			// Grab the pitch and volume before doing any setup
			OriginalPitch = attachedSource.pitch;
			OriginalVolume = attachedSource.volume;

			// Check to see if audio already has a clip
			if (attachedSource.clip != null)
			{
				// If so, check to see if it's already in the list
				int frequency = ClipVariations.GetFrequency(attachedSource.clip);
				if (frequency == 0)
				{
					// If not, add it in with default frequency
					ClipVariations.Add(attachedSource.clip);
				}
			}
		}

		static LinkedListNode<AudioSource> FindFirstStoppedLayer(LinkedListNode<AudioSource> checkNode)
		{
			while (checkNode != null)
			{
				// Halt if this audio has stopped
				if ((checkNode.Value.isPlaying == false) && (Mathf.Approximately(checkNode.Value.time, 0)))
				{
					break;
				}

				// Otherwise, check the next node
				checkNode = checkNode.Next;
			}
			return checkNode;
		}

		static Vector2 ClampRange(Vector2 value, float min, float max)
		{
			if (value.x < min)
			{
				value.x = min;
			}

			if (value.y > max)
			{
				value.y = max;
			}

			return value;
		}
	}
}
