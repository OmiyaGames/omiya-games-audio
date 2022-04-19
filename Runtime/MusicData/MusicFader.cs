using System;
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
			public GameObject player;
			public Layer layer;
			public double startTime;
			public double fadeDuration;
		}

		int nextLayer = 0;
		MusicManager currentMusic = null;
		PlayData currentMetaData = null;
		readonly MonoBehaviour manager;
		readonly Layer[] fadeLayers;
		readonly AudioSource defaultAudioPrefab;
		readonly MusicDataCollection<PlayData> fadeOutQueue = new MusicDataCollection<PlayData>();
		readonly AnimationCurve percentToDbCurve;

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
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="player"></param>
		/// <param name="args"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void FadeIn(MusicData music, GameObject player, FadeInArgs args = null)
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
				e.Music.CleanUp(player);
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
		public void FadeIn(AsyncOperationHandle<MusicData> music, GameObject player, FadeInArgs args = null)
		{
			// Check if arguments are valid
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Perform fade-in
			MusicManager instance = MusicManager.Create(music, e =>
			{
				e.Music.CleanUp(player);
				UnityEngine.Object.Destroy(player);
			});
			FadeIn(instance, player, args);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="args"></param>
		public bool FadeOut(FadeOutArgs args = null)
		{
			// Check if the current music is assigned
			bool isFadingOut = false;
			if ((currentMusic == null) || (currentMetaData == null))
			{
				return isFadingOut;
			}

			// Check if the last music even played
			if (currentMetaData.startTime < UnityEngine.AudioSettings.dspTime)
			{
				// Update the metadata
				currentMetaData.startTime = UnityEngine.AudioSettings.dspTime;
				currentMetaData.fadeDuration = 0;
				if (args != null)
				{
					currentMetaData.startTime += args.Delay;
					currentMetaData.fadeDuration = args.Duration;
				}

				// Add the current music into the fade-out queue
				fadeOutQueue.AddLast(currentMusic.Music, currentMetaData);
				isFadingOut = true;
			}

			// FIXME: perform fade out
			// For now, we just make the current music stop immediately
			currentMusic.Music.Stop(currentMetaData.player);
			SetVolume(in currentMetaData.layer, 0);

			// Reset the variables
			SetCurrentMusicData(null, null);
			return isFadingOut;
		}

		void FadeIn(MusicManager music, GameObject player, FadeInArgs args = null)
		{
			// Fade out the current music
			if (FadeOut(args?.FadeOut))
			{
				// If fade-out was performed, increment layer index
				nextLayer = (nextLayer + 1) % fadeLayers.Length;
			}

			// Calculate time
			double startTime = UnityEngine.AudioSettings.dspTime;
			double fadeDuration = 0;
			if (args != null)
			{
				startTime += args.StartTime;
				fadeDuration = args.Duration;
			}

			// Setup the member variables
			Layer layer = fadeLayers[nextLayer];
			SetCurrentMusicData(music, new PlayData()
			{
				player = player,
				layer = layer,
				startTime = startTime,
				fadeDuration = fadeDuration,
			});

			// Start playing the music
			currentMusic.IncrementCounter();
			currentMusic.Music.Setup(player, layer.Group, defaultAudioPrefab);
			currentMusic.Music.Play(player, args);

			// FIXME: perform the fade-in
			// For now, basically just play instantly
			SetVolume(in layer, 1);
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
	}
}
