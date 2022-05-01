using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{

	// FIXME: move this class as a private, nested class to SingleLoopingMusic
	// It's currently separated in a file for testing purposes.
	/// <summary>
	/// TODO
	/// </summary>
	public class SingleLoopingMusicPlayer : BackgroundAudio.Player
	{
		[SerializeField]
		SingleLoopingMusic data;
		[SerializeField]
		AudioSource intro;
		[SerializeField]
		AudioSource loop;

		/// <inheritdoc/>
		public override BackgroundAudio Data => data;
		/// <inheritdoc/>
		public override BackgroundAudio.PlayState State
		{
			get
			{
				// FIXME: this isn't sophisticated enough
				if (loop.isPlaying)
				{
					return BackgroundAudio.PlayState.Playing;
				}
				else if (loop.timeSamples > 0)
				{
					return BackgroundAudio.PlayState.Paused;
				}
				else if (intro != null)
				{
					if (intro.isPlaying)
					{
						return BackgroundAudio.PlayState.Playing;
					}
					else if (intro.timeSamples > 0)
					{
						return BackgroundAudio.PlayState.Paused;
					}
				}
				return BackgroundAudio.PlayState.Stopped;
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

		[ContextMenu("Setup")]
		[System.Obsolete("Will be removed")]
		void Setup() => Setup(data);

		[ContextMenu("Play Immediately")]
		[System.Obsolete("Will be removed")]
		void Play() => Play(null);

		[ContextMenu("Play Delayed")]
		[System.Obsolete("Will be removed")]
		void PlayDelayed() => Play(new() { DelaySeconds = 1f });

		/// <inheritdoc/>
		public override void Play(PlaybackArgs args)
		{
			// Determine when to start playing the audio
			double playTimeStamp = UnityEngine.AudioSettings.dspTime;
			double skipForwardToSeconds = 0;
			if (args != null)
			{
				playTimeStamp += args.DelaySeconds;
				skipForwardToSeconds = args.SkipForwardToSeconds;
			}

			// Check the state
			if (State == BackgroundAudio.PlayState.Stopped)
			{
				PlayFromStop(playTimeStamp, skipForwardToSeconds);
			}
			else if (State == BackgroundAudio.PlayState.Paused)
			{
				if (intro != null)
				{
					// FIXME: actually use DSP into account
					throw new System.NotImplementedException();
				}
				else
				{
					loop.PlayScheduled(playTimeStamp);
				}
			}
		}

		/// <inheritdoc/>
		[ContextMenu("Stop Immediately")]
		public override void Stop()
		{
			// Stop both audio sources immediately
			loop.Stop();
			if (intro != null)
			{
				intro.Stop();
			}
		}

		/// <inheritdoc/>
		[ContextMenu("Pause Immediately")]
		public override void Pause()
		{
			// Pause main audio source
			if (intro != null)
			{
				// FIXME: figure out the times and scheduled delays so resuming is easier
				// FIXME: don't forget to reset these stats on Stop()
				throw new System.NotImplementedException();
				intro.Pause();
				loop.Pause();
			}
			else
			{
				loop.Pause();
			}
		}

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
			loop = Instantiate(Data.MainAudioSourcePrefab, Vector3.zero, Quaternion.identity, transform);
			loop.gameObject.name = "Looping Audio Player";
			loop.loop = true;
			loop.clip = data.loop;

			// Check if we need to generate the intro sting player
			intro = null;
			if (data.addIntroSting)
			{
				intro = Instantiate(data.MainAudioSourcePrefab, Vector3.zero, Quaternion.identity, transform);
				intro.gameObject.name = "Intro Sting Player";
				intro.loop = false;
				intro.clip = data.introSting;
			}
		}

		void PlayFromStop(double playTimeStamp, double skipForwardToSeconds)
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
					playTimeStamp += data.playLoopAfterSeconds;

					// Update skip forward timestamp for main loop
					skipForwardToSeconds -= data.playLoopAfterSeconds;
				}
				else
				{
					// Don't play intro sting, and skip main-loop forward by a certin amount
					skipForwardToSeconds -= introLength;
				}
			}

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
