using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OmiyaGames.Audio.Collections
{
	public class MusicFader
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

			/// <summary>
			/// TODO
			/// </summary>
			public AudioMixerGroup Group => group;
			/// <summary>
			/// TODO
			/// </summary>
			public string ParamName => paramName;
		}

		class PlayData
		{
			public BackgroundAudio.Player player;
			public Layer layer;
			public double startTime;
			public double fadeDuration;
			public float volumePercent;
			public Coroutine fadeRoutine;
			public bool pauseOnFadeOut;
		}

		int nextLayer = 0;
		MusicManager currentMusic = null;
		PlayData currentMetaData = null;
		readonly MonoBehaviour manager;
		readonly Layer[] fadeLayers;
		readonly AudioSource defaultAudioPrefab;
		readonly MusicDataCollection<PlayData> fadeOutQueue = new MusicDataCollection<PlayData>();
		readonly AnimationCurve percentToDbCurve;
		readonly Action afterFadeOut;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="defaultAudioPrefab"></param>
		/// <param name="percentToDbCurve"></param>
		/// <param name="fadeLayers"></param>
		public MusicFader(MonoBehaviour manager, AudioSource defaultAudioPrefab, AnimationCurve percentToDbCurve, params Layer[] fadeLayers)
		{
			// Null check
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}
			if (defaultAudioPrefab == null)
			{
				throw new ArgumentNullException(nameof(defaultAudioPrefab));
			}
			if (percentToDbCurve == null)
			{
				throw new ArgumentNullException(nameof(percentToDbCurve));
			}
			if (fadeLayers == null)
			{
				throw new ArgumentNullException(nameof(fadeLayers));
			}
			if (fadeLayers.Length < 1)
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
			}

			this.manager = manager;
			this.defaultAudioPrefab = defaultAudioPrefab;
			this.percentToDbCurve = percentToDbCurve;
			this.fadeLayers = fadeLayers;
			this.afterFadeOut = new Action(() => PruneFadeOutQueue(false));
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="player"></param>
		/// <param name="args"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void FadeIn(BackgroundAudio music, GameObject player, FadeInArgs args)
		{
			// Check if arguments are valid
			if (music == null)
			{
				throw new ArgumentNullException(nameof(music));
			}
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Perform fade-in
			MusicManager instance = MusicManager.Create(music, e =>
			{
				UnityEngine.Object.Destroy(player);
			});
			FadeIn(instance, player, args);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="player"></param>
		/// <param name="args"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void FadeIn(AsyncOperationHandle<BackgroundAudio> music, GameObject player, FadeInArgs args)
		{
			// Check if arguments are valid
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Perform fade-in
			MusicManager instance = MusicManager.Create(music, e =>
			{
				UnityEngine.Object.Destroy(player);
			});
			FadeIn(instance, player, args);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="args"></param>
		public bool FadeOut(FadeOutArgs args)
		{
			// Check if the current music is assigned
			bool isFadingOut = false;
			if ((currentMusic == null) || (currentMetaData == null))
			{
				return isFadingOut;
			}

			// Stop the fade-in coroutine, if one is running
			if (currentMetaData.fadeRoutine != null)
			{
				manager.StopCoroutine(currentMetaData.fadeRoutine);
				currentMetaData.fadeRoutine = null;
			}

			// Check if the last music even played
			isFadingOut = currentMetaData.startTime < UnityEngine.AudioSettings.dspTime;
			if (isFadingOut)
			{
				// Update the metadata
				currentMetaData.startTime = UnityEngine.AudioSettings.dspTime;
				currentMetaData.fadeDuration = 0;
				currentMetaData.pauseOnFadeOut = false;
				if (args != null)
				{
					currentMetaData.startTime += args.DelaySeconds;
					currentMetaData.fadeDuration = args.DurationSeconds;
					currentMetaData.pauseOnFadeOut = args.Pause;
				}

				// Add the current music into the fade-out queue
				fadeOutQueue.AddLast(currentMusic.Music, currentMetaData);

				// Check if the queue exceeds the number of layers
				PruneFadeOutQueue(false);

				// Start the fade-out coroutine
				currentMetaData.fadeRoutine = manager.StartCoroutine(FadeRoutine(currentMetaData, true, afterFadeOut));
			}
			else
			{
				// If not, stop or pause the last music
				if ((args != null) && args.Pause)
				{
					currentMetaData.player.Pause();
				}
				else
				{
					currentMetaData.player.Stop();
				}

				// Silence the layer the music was playing on
				SetVolume(in currentMetaData.layer, 0);
			}

			// Reset the variables
			SetCurrentMusicData(null, null);
			return isFadingOut;
		}

		void FadeIn(MusicManager music, GameObject attach, FadeInArgs args)
		{
			// Prune the fade-out queue as well
			PruneFadeOutQueue(true);

			// Check if there are any music being faded out
			if (fadeOutQueue.Count > 0)
			{
				// Increment layer index
				nextLayer = (nextLayer + 1) % fadeLayers.Length;
			}
			Layer layer = fadeLayers[nextLayer];

			// Calculate time
			double startTime = UnityEngine.AudioSettings.dspTime;
			double fadeDuration = 0;
			if (args != null)
			{
				startTime += args.DelaySeconds;
				fadeDuration = args.DurationSeconds;
			}

			// Generate a new player
			BackgroundAudio.Player player = music.Music.GeneratePlayer(attach);
			player.MixerGroup = layer.Group;

			// Setup the member variables
			SetCurrentMusicData(music, new PlayData()
			{
				player = player,
				layer = layer,
				volumePercent = 0f,
				startTime = startTime,
				fadeDuration = fadeDuration,
			});

			// Start playing the music
			player.Play(args);

			// Start the fade-in coroutine
			currentMetaData.fadeRoutine = manager.StartCoroutine(FadeRoutine(currentMetaData, false));
		}

		void SetCurrentMusicData(MusicManager music, PlayData metaData)
		{
			if (currentMusic != null)
			{
				currentMusic.DecrementCounter();
			}

			currentMusic = music;
			currentMetaData = metaData;

			if (currentMusic != null)
			{
				currentMusic.IncrementCounter();
			}
		}

		void SetVolume(in Layer layer, float volumePercent)
		{
			// Grab all the necessary parameters
			AudioMixer mixer = layer.Group.audioMixer;
			string paramName = layer.ParamName;

			// Compute the volume
			float volumeDb = percentToDbCurve.Evaluate(volumePercent);

			// Update the mixer
			mixer.SetFloat(paramName, volumeDb);
		}

		IEnumerator FadeRoutine(PlayData metaData, bool fadeOut, Action afterFadeFinished = null)
		{
			// Set starting volume
			float startingVolumePercent = metaData.volumePercent;
			SetVolume(in metaData.layer, startingVolumePercent);

			// Wait until start time is met
			if (metaData.startTime > UnityEngine.AudioSettings.dspTime)
			{
				yield return new WaitUntil(() => UnityEngine.AudioSettings.dspTime >= metaData.startTime);
			}

			// Start the fade
			double currentDuration = 0;
			while (currentDuration < metaData.fadeDuration)
			{
				// Set the volume
				float volumePercent = (float)(currentDuration / metaData.fadeDuration);
				volumePercent = Mathf.Clamp01(volumePercent);
				if (fadeOut)
				{
					// For fade out, flip the fade direction, and scale it by starting volume
					volumePercent = 1f - volumePercent;
					volumePercent *= startingVolumePercent;
				}
				metaData.volumePercent = volumePercent;
				SetVolume(in metaData.layer, metaData.volumePercent);

				// Wait for a frame
				yield return null;

				// Calculate how much time has passed
				currentDuration = UnityEngine.AudioSettings.dspTime - metaData.startTime;
			}

			// Set ending volume
			metaData.volumePercent = fadeOut ? 0 : 1;
			SetVolume(in metaData.layer, metaData.volumePercent);

			// Run the action indicating fade has completed
			afterFadeFinished?.Invoke();
		}

		void PruneFadeOutQueue(bool includeCurrentMusic)
		{
			// Calculate how many elements should be in the fade queue
			int finalQueueSize = fadeLayers.Length;
			if (includeCurrentMusic && (finalQueueSize > 0))
			{
				--finalQueueSize;
			}

			// Check if queue is not empty
			while (fadeOutQueue.Count > 0)
			{
				// Grab the first element in the queue
				var dequeue = fadeOutQueue.First;

				// Break if queue is within expected size limit, AND the first element has not finished fading
				if ((fadeOutQueue.Count <= finalQueueSize) &&
					((dequeue.MetaData.startTime + dequeue.MetaData.fadeDuration) > UnityEngine.AudioSettings.dspTime))
				{
					break;
				}

				// Stop the corresponding coroutine
				if (dequeue.MetaData.fadeRoutine != null)
				{
					manager.StopCoroutine(dequeue.MetaData.fadeRoutine);
				}

				// Pause or stop the music
				if (dequeue.MetaData.pauseOnFadeOut)
				{
					dequeue.MetaData.player.Pause();
				}
				else
				{
					dequeue.MetaData.player.Stop();
				}

				// Remove an element from the queue
				fadeOutQueue.RemoveFirst();
			}
		}
	}
}
