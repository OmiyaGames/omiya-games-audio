using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="SingleLoopingMusic.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 4/16/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Sets up the background audio to a single looping music
	/// </summary>
	[CreateAssetMenu(menuName = "Omiya Games/Audio/Looping Music", fileName = "Looping Music", order = (MENU_ORDER))]
	public class SingleLoopingMusic : MusicData
	{
		[SerializeField]
		AudioClip loop;

		[Header("Intro Stings")]
		[SerializeField]
		bool addIntroSting = false;
		[SerializeField]
		AudioClip introSting;
		[SerializeField]
		double playLoopAfterSeconds;

		class AudioPlayers
		{
			public AudioPlayers(AudioSource main, AudioSource introSting)
			{
				Main = main;
				IntroSting = introSting;
			}

			public AudioSource Main
			{
				get;
			}

			public AudioSource IntroSting
			{
				get;
			}

			public bool IsPlaying
			{
				get;
				set;
			} = false;
		}

		readonly Dictionary<GameObject, AudioPlayers> cache = new Dictionary<GameObject, AudioPlayers>();

		/// <inheritdoc/>
		public override void Setup(GameObject attach, AudioMixerGroup group, AudioSource audioPrefab)
		{
			// Grab the components from the cache
			if (cache.TryGetValue(attach, out AudioPlayers players) == false)
			{
				// Setup the looping audio player
				AudioSource main = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity, attach.transform);
				main.loop = true;
				main.clip = loop;

				// Check if we need to generate the intro sting player
				AudioSource intro = null;
				if (addIntroSting)
				{
					intro = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity, attach.transform);
					intro.loop = false;
					intro.clip = introSting;
				}

				// Add this to the cache
				players = new AudioPlayers(main, intro);
				cache.Add(attach, players);
			}

			// Setup the audio groups
			players.Main.outputAudioMixerGroup = group;
			players.IntroSting.outputAudioMixerGroup = group;
		}

		/// <inheritdoc/>
		public override void CleanUp(GameObject attach)
		{
			// Check if this game object has been cached
			if (cache.TryGetValue(attach, out AudioPlayers players) == false)
			{
				// If not, skip cleaning up this object
				return;
			}

			// Destroy the main audio player
			Destroy(players.Main);

			// Destroy the intro sting audio player
			if (players.IntroSting != null)
			{
				Destroy(players.IntroSting);
			}

			// Remove this entry from the cache
			cache.Remove(attach);

			// Destroy the attached object itself
			Destroy(attach);
		}

		/// <inheritdoc/>
		public override void Play(GameObject attach)
		{
			// Check if this game object has been cached
			if (cache.TryGetValue(attach, out AudioPlayers players) == false)
			{
				// If not, skip cleaning up this object
				return;
			}

			// Check if the player is not playing
			if (players.IsPlaying == false)
			{
				if (players.IntroSting)
				{
					// TODO: is .Play() accurate enough?  Do we need to use PlayScheduled instead?
					players.IntroSting.Play();
					players.Main.PlayScheduled(UnityEngine.AudioSettings.dspTime + playLoopAfterSeconds);
				}
				else
				{
					// Only play the main loop
					players.Main.Play();
				}

				// Indicate we're playing
				players.IsPlaying = true;
			}
		}

		/// <inheritdoc/>
		public override void Stop(GameObject attach)
		{
			// Check if this game object has been cached
			if (cache.TryGetValue(attach, out AudioPlayers players) == false)
			{
				// If not, skip cleaning up this object
				return;
			}

			// Check if the player is playing
			if (players.IsPlaying)
			{
				// Stop both audio sources
				players.IntroSting.Stop();
				players.Main.Stop();
				players.IsPlaying = false;
			}
		}

		/// <inheritdoc/>
		public override PlayState IsPlaying(GameObject attach)
		{
			// Check if this game object has been cached
			if (cache.TryGetValue(attach, out AudioPlayers players))
			{
				// Return its play state
				return players.IsPlaying ? PlayState.Playing : PlayState.Stopped;
			}

			// Otherwise, return invalid
			return PlayState.Invalid;
		}

		/// <summary>
		/// Calculates how long <see cref="introSting"/> is,
		/// and sets <see cref="playLoopAfterSeconds"/>.
		/// This produces a more accurate value than
		/// <see cref="AudioClip.length"/>.
		/// </summary>
		[ContextMenu("Calculate Intro Sting Duration")]
		public void SetLoopDelayToIntroStingDuration()
		{
			// Make sure we have a sting to play
			if (addIntroSting && introSting)
			{
				// Calculate the duration based on samples and frequency
				playLoopAfterSeconds = introSting.samples;
				playLoopAfterSeconds /= introSting.frequency;
			}
		}
	}
}
