using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
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

			// FIXME: turn into properties at some point
			public double startTime;
			public double fadeDuration;
			public float volumePercent;
			public Coroutine fadeRoutine;
			public bool pauseOnFadeOut;
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

		// FIXME: get rid of this constructor when MusicFader is removed
		public MixerGroupManager(AudioPlayerManager manager, AnimationCurve percentToDbCurve, params Collections.MusicFader.Layer[] fadeLayers)
			: this(manager, percentToDbCurve, new Layer[fadeLayers.Length])
		{
			for (int i = 0; i < fadeLayers.Length; i++)
			{
				fader[i] = new FadeSet(new Layer(fadeLayers[i]));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attach"></param>
		/// <param name="args"></param>
		public void FadeIn(BackgroundAudio.Player player, FadeInArgs args)
		{
			if (player == null)
			{
				throw new ArgumentNullException(nameof(player));
			}

			// Get or create a new player metadata
			FadeSet playerInfo = GetPlayerFadeInfo(player);
			if (playerInfo == null)
			{
				playerInfo = CreatePlayerFadeInfo(player);
			}

			// Stop the fade-in coroutine, if one is running
			if (playerInfo.fadeRoutine != null)
			{
				manager.StopCoroutine(playerInfo.fadeRoutine);
			}

			// FIXME: actually implement
			throw new NotImplementedException();
			//// Calculate time
			//double startTime = UnityEngine.AudioSettings.dspTime;
			//double fadeDuration = 0;
			//if (args != null)
			//{
			//	startTime += args.DelaySeconds;
			//	fadeDuration = args.DurationSeconds;
			//}

			//// Setup the member variables
			//SetCurrentMusicData(music, new PlayData()
			//{
			//	player = player,
			//	layer = layer,
			//	volumePercent = 0f,
			//	startTime = startTime,
			//	fadeDuration = fadeDuration,
			//});

			// Start playing the music
			player.Play(args);

			// Start the fade-in coroutine
			playerInfo.fadeRoutine = manager.StartCoroutine(FadeRoutine(playerInfo, false));
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

			// FIXME: actually implement
			bool isFadingOut = false;

			// Check if the current music is assigned
			FadeSet playerInfo = GetPlayerFadeInfo(player);
			if (playerInfo == null)
			{
				return isFadingOut;
			}

			// Stop the fade-in coroutine, if one is running
			if (playerInfo.fadeRoutine != null)
			{
				manager.StopCoroutine(playerInfo.fadeRoutine);
				playerInfo.fadeRoutine = null;
			}

			//// Check if the last music even played
			//isFadingOut = currentMetaData.startTime < UnityEngine.AudioSettings.dspTime;
			//if (isFadingOut)
			//{
			//	// Update the metadata
			//	currentMetaData.startTime = UnityEngine.AudioSettings.dspTime;
			//	currentMetaData.fadeDuration = 0;
			//	currentMetaData.pauseOnFadeOut = false;
			//	if (args != null)
			//	{
			//		currentMetaData.startTime += args.DelaySeconds;
			//		currentMetaData.fadeDuration = args.DurationSeconds;
			//		currentMetaData.pauseOnFadeOut = args.Pause;
			//	}

			//	// Add the current music into the fade-out queue
			//	fadeOutQueue.AddLast(currentMusic.Music, currentMetaData);

			//	// Check if the queue exceeds the number of layers
			//	PruneFadeOutQueue(false);

			//	// Start the fade-out coroutine
			//	currentMetaData.fadeRoutine = manager.StartCoroutine(FadeRoutine(currentMetaData, true, afterFadeOut));
			//}
			//else
			//{
			//	// If not, stop or pause the last music
			//	if ((args != null) && args.Pause)
			//	{
			//		currentMetaData.player.Pause();
			//	}
			//	else
			//	{
			//		currentMetaData.player.Stop();
			//	}

			//	// Silence the layer the music was playing on
			//	SetVolume(in currentMetaData.layer, 0);
			//}

			//// Reset the variables
			//SetCurrentMusicData(null, null);
			return isFadingOut;
		}

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
			// FIXME: implement
			throw new System.NotImplementedException();

			//// Generate a new player
			//BackgroundAudio.Player player = music.Music.GeneratePlayer(attach);
			//player.MixerGroup = layer.Group;
		}

		FadeSet CreatePlayerFadeInfo(BackgroundAudio.Player player)
		{
			// FIXME: implement
			throw new System.NotImplementedException();

			//// Generate a new player
			//BackgroundAudio.Player player = music.Music.GeneratePlayer(attach);
			//player.MixerGroup = layer.Group;
		}

		IEnumerator FadeRoutine(FadeSet metaData, bool fadeOut, Action afterFadeFinished = null)
		{
			// Set starting volume
			float startingVolumePercent = metaData.volumePercent;
			SetVolume(metaData.Layer, startingVolumePercent);

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
				SetVolume(metaData.Layer, metaData.volumePercent);

				// Wait for a frame
				yield return null;

				// Calculate how much time has passed
				currentDuration = UnityEngine.AudioSettings.dspTime - metaData.startTime;
			}

			// Set ending volume
			metaData.volumePercent = fadeOut ? 0 : 1;
			SetVolume(metaData.Layer, metaData.volumePercent);

			// Run the action indicating fade has completed
			afterFadeFinished?.Invoke();
		}
	}
}
