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
	public class SingleLoopingMusic : BackgroundAudio
	{
		[SerializeField]
		AudioClip loop;

		[Header("Intro Stings")]
		[SerializeField]
		AudioClip introSting;
		[SerializeField]
		double playLoopAfterSeconds;

		/// <summary>
		/// TODO
		/// </summary>
		public double PlayLoopAfterSeconds => playLoopAfterSeconds;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioClip IntroSting => introSting;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioClip Loop => loop;

		/// <inheritdoc/>
		public override Player GeneratePlayer(GameObject attach)
		{
			var returnScript = attach.AddComponent<SingleLoopingMusicPlayer>();
			returnScript.Setup(this);
			return returnScript;
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();

			SetLoopDelayToIntroStingDuration();
		}

		/// <summary>
		/// Calculates how long <see cref="IntroSting"/> is,
		/// and sets <see cref="PlayLoopAfterSeconds"/>.
		/// This produces a more accurate value than
		/// <see cref="AudioClip.length"/>.
		/// </summary>
		public void SetLoopDelayToIntroStingDuration()
		{
			// Make sure we have a sting to play
			if (IntroSting != null)
			{
				// Calculate the duration based on samples and frequency
				playLoopAfterSeconds = AudioManager.CalculateClipLengthSeconds(IntroSting);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		class SingleLoopingMusicPlayer : Player
		{
			[SerializeField]
			SingleLoopingMusic data;
			[SerializeField]
			AudioSource intro;
			[SerializeField]
			AudioSource loop;
			[SerializeField]
			double playTimeStamp = 0;

			/// <inheritdoc/>
			public override event ITrackable<PlayState>.ChangeEvent OnBeforeChangeState;
			/// <inheritdoc/>
			public override event ITrackable<PlayState>.ChangeEvent OnAfterChangeState;

			/// <inheritdoc/>
			public override BackgroundAudio Data => data;
			/// <inheritdoc/>
			public override PlayState State
			{
				get
				{
					// Check if either Audio sources are playing
					if (((loop != null) && (loop.isPlaying)) ||
						((intro != null) && (intro.isPlaying)))
					{
						// Check if it's before the scheduled time of play
						if (playTimeStamp > UnityEngine.AudioSettings.dspTime)
						{
							return PlayState.Scheduled;
						}
						return PlayState.Playing;
					}
					return PlayState.Stopped;
				}
			}
			/// <inheritdoc/>
			public override AudioMixerGroup MixerGroup
			{
				set
				{
					// Setup the audio groups
					loop.outputAudioMixerGroup = value;
					if (intro != null)
					{
						intro.outputAudioMixerGroup = value;
					}
				}
			}

			/// <inheritdoc/>
			public override void Play(PlaybackArgs args)
			{
				// Grab the state changes
				PlayState startState = State;
				PlayState endState = PlayState.Playing;

				// Determine when to start playing the audio
				playTimeStamp = UnityEngine.AudioSettings.dspTime;
				double skipForwardToSeconds = 0;
				if (args != null)
				{
					skipForwardToSeconds = args.SkipForwardToSeconds;

					if (args.DelaySeconds > 0)
					{
						endState = PlayState.Scheduled;
						playTimeStamp += args.DelaySeconds;
					}
				}

				// Check the state
				if (startState == PlayState.Stopped)
				{
					// Invoke state change event
					OnBeforeChangeState?.Invoke(startState, endState);

					PlayFromStop(skipForwardToSeconds);

					// Invoke state change event
					OnAfterChangeState?.Invoke(startState, endState);
				}
			}

			/// <inheritdoc/>
			public override void Stop()
			{
				PlayState startState = State;
				if (startState != PlayState.Stopped)
				{
					// Invoke state change event
					OnBeforeChangeState?.Invoke(startState, PlayState.Stopped);

					// Stop both audio sources immediately
					if (loop != null)
					{
						loop.Stop();
					}
					if (intro != null)
					{
						intro.Stop();
					}

					// Invoke state change event
					OnAfterChangeState?.Invoke(startState, PlayState.Stopped);
				}
			}

			/// <inheritdoc/>
			//public override void Pause()
			//{
			//	PlayState startState = State;
			//	if ((startState == PlayState.Playing) || (startState == PlayState.Scheduled))
			//	{
			//		// Invoke state change event
			//		OnBeforeChangeState?.Invoke(startState, PlayState.Paused);

			//		// Pause main audio source
			//		if (intro != null)
			//		{
			//			// TODO: figure out the times and scheduled delays so resuming is easier
			//			// don't forget to reset these stats on Stop()
			//			throw new System.NotImplementedException();
			//			intro.Pause();
			//			loop.Pause();
			//		}
			//		else
			//		{
			//			loop.Pause();
			//		}

			//		// Invoke state change event
			//		OnAfterChangeState?.Invoke(startState, PlayState.Paused);
			//	}
			//}

			/// <inheritdoc/>
			internal void Setup(SingleLoopingMusic data)
			{
				// Make sure audio prefab is not null
				if (data.MainAudioSourcePrefab == null)
				{
					throw new System.ArgumentNullException(nameof(data), "Cannot create a new audio source without a prefab.");
				}

				// Setup the data
				this.data = data;

				// Setup the looping audio player
				loop = null;
				if (data.Loop)
				{
					loop = Instantiate(Data.MainAudioSourcePrefab, Vector3.zero, Quaternion.identity, transform);
					loop.gameObject.name = "Looping Audio Player";
					loop.loop = true;
					loop.clip = data.Loop;
				}

				// Check if we need to generate the intro sting player
				intro = null;
				if (data.IntroSting)
				{
					intro = Instantiate(data.MainAudioSourcePrefab, Vector3.zero, Quaternion.identity, transform);
					intro.gameObject.name = "Intro Sting Player";
					intro.loop = false;
					intro.clip = data.IntroSting;
				}
			}

			void PlayFromStop(double skipForwardToSeconds)
			{
				// Check if there's an intro sting to play
				if (intro != null)
				{
					// Confirm skipping forward timestamp doesn't exceed intro sting length
					double introLength = AudioManager.CalculateClipLengthSeconds(intro.clip);
					if (skipForwardToSeconds < introLength)
					{
						// If not, schedule playing the intro
						intro.PlayScheduled(playTimeStamp);
						intro.timeSamples = AudioManager.CalculateTimeSample(intro.clip, skipForwardToSeconds);

						// Delay playing the main tune
						playTimeStamp += data.PlayLoopAfterSeconds;

						// Update skip forward timestamp for main loop
						skipForwardToSeconds -= data.PlayLoopAfterSeconds;
					}
					else
					{
						// Don't play intro sting, and skip main-loop forward by a certain amount
						skipForwardToSeconds -= introLength;
					}
				}

				// Check if there's a loop to play
				if (loop != null)
				{
					// Schedule when to play the main loop
					loop.PlayScheduled(playTimeStamp);

					// Correct skip forward time for main loop
					if (skipForwardToSeconds < 0)
					{
						skipForwardToSeconds = 0;
					}
					else
					{
						// Calculate how long the main loop is
						double mainLength = AudioManager.CalculateClipLengthSeconds(loop.clip);

						// Reduce skip forward duration by this number until it's small enough
						while (skipForwardToSeconds > mainLength)
						{
							skipForwardToSeconds -= mainLength;
						}
					}

					// Skip main forward in time
					loop.timeSamples = AudioManager.CalculateTimeSample(loop.clip, skipForwardToSeconds);
				}
			}
		}
	}
}
