using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using OmiyaGames.Managers;
using OmiyaGames.Global;
using OmiyaGames.Global.Settings;
using OmiyaGames.Saves;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AudioManager.cs" company="Omiya Games">
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
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 2/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// A manager file that allows adjusting an <see cref="AudioMixer"/>
	/// from settings.
	/// </summary>
	public static class AudioManager
	{
		/// <summary>
		/// The configuration name stored in Editor Settings.
		/// </summary>
		public const string CONFIG_NAME = "com.omiyagames.audio";
		/// <summary>
		/// The name this settings will appear in the
		/// Project Setting's left-sidebar.
		/// </summary>
		public const string SIDEBAR_PATH = "Project/Omiya Games/Audio";
		/// <summary>
		/// Name of the addressable.
		/// </summary>
		public const string ADDRESSABLE_NAME = "AudioSettings";
		/// <summary>
		/// Path to UXML file.
		/// </summary>
		public const string UXML_PATH = "Packages/com.omiyagames.audio/Editor/Audio.uxml";

		class AudioFilePlayerPair
		{
			public AssetRef<BackgroundAudio> File
			{
				get;
				set;
			}
			public BackgroundAudio.Player Player
			{
				get;
				set;
			}
		}

		/// <summary>
		/// Indicates whether the manager is either still
		/// in the middle of setting up, or is already setup.
		/// </summary>
		public static Data.Status Status => AudioSettingsManager.GetDataStatus();

		/// <summary>
		/// A coroutine to setup this manager.
		/// </summary>
		/// <param name="forceSetup"></param>
		/// <returns></returns>
		public static IEnumerator Setup(bool forceSetup = false)
		{
			yield return Manager.StartCoroutine(AudioSettingsManager.Setup(forceSetup));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static AudioMixer Mixer => AudioSettingsManager.GetDataOrThrow().Mixer;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer Main => AudioSettingsManager.GetDataOrThrow().Main;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Background Music => AudioSettingsManager.GetDataOrThrow().Music;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Spatial SoundEffects => AudioSettingsManager.GetDataOrThrow().SoundEffects;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Spatial Voices => AudioSettingsManager.GetDataOrThrow().Voices;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Background Ambience => AudioSettingsManager.GetDataOrThrow().Ambience;
		/// <summary>
		/// TODO
		/// </summary>
		public static float MuteVolumeDb => AudioSettingsManager.GetDataOrThrow().MuteVolumeDb;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="percent"></param>
		/// <returns></returns>
		public static float ConvertPercentToVolumeDb(float percent) =>
			AudioSettingsManager.GetDataOrThrow().PercentToDbCurve.Evaluate(percent);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="clip"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="clip"/> is <c>null</c>.
		/// </exception>
		public static double CalculateClipLengthSeconds(AudioClip clip)
		{
			if (clip == null)
			{
				throw new System.ArgumentNullException(nameof(clip));
			}

			double returnSeconds = clip.samples;
			returnSeconds /= clip.frequency;
			return returnSeconds;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="timeStamp"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public static int CalculateTimeSample(AudioClip clip, double timeStamp)
		{
			if (clip == null)
			{
				throw new System.ArgumentNullException(nameof(clip));
			}
			else if (timeStamp < 0)
			{
				throw new System.ArgumentOutOfRangeException(nameof(timeStamp), "Timestamp can't be negative.");
			}

			return (int)System.Math.Round(clip.frequency * timeStamp);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="playMusic"></param>
		/// <param name="playAmbience"></param>
		/// <param name="fadeInArgs"></param>
		public static IEnumerator PlayMusicAndAmbience(BackgroundAudio playMusic, BackgroundAudio playAmbience, FadeInArgs fadeInArgs = null)
		{
			// Push all the valid assets/references into the map
			Dictionary<AudioLayer.Background, AudioFilePlayerPair> loadAudioMap = new(2);
			AddToAudioMap(Music, playMusic);
			AddToAudioMap(Ambience, playAmbience);

			// Star the coroutine
			yield return Manager.StartCoroutine(PlayMusicAndAmbience(loadAudioMap, fadeInArgs));

			void AddToAudioMap(AudioLayer.Background backgroundAudio, BackgroundAudio playAudio)
			{
				if (playAudio != null)
				{
					loadAudioMap.Add(backgroundAudio, new()
					{
						File = new(playAudio)
					});
				}
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="playMusicRef"></param>
		/// <param name="playAmbienceRef"></param>
		/// <param name="fadeInArgs"></param>
		public static IEnumerator PlayMusicAndAmbience(AssetReferenceT<BackgroundAudio> playMusicRef, AssetReferenceT<BackgroundAudio> playAmbienceRef, FadeInArgs fadeInArgs = null)
		{
			// Push all the valid assets/references into the map
			Dictionary<AudioLayer.Background, AudioFilePlayerPair> loadAudioMap = new(2);
			AddToAudioMap(Music, playMusicRef);
			AddToAudioMap(Ambience, playAmbienceRef);

			// Star the coroutine
			yield return Manager.StartCoroutine(PlayMusicAndAmbience(loadAudioMap, fadeInArgs));

			void AddToAudioMap(AudioLayer.Background backgroundAudio, AssetReferenceT<BackgroundAudio> playAudioRef)
			{
				if (playAudioRef != null)
				{
					loadAudioMap.Add(backgroundAudio, new()
					{
						File = new(playAudioRef)
					});
				}
			}
		}

		static IEnumerator PlayMusicAndAmbience(Dictionary<AudioLayer.Background, AudioFilePlayerPair> loadAudioMap, FadeInArgs fadeInArgs)
		{
			// Grab the corresponding player for each audio asset
			foreach (var pair in loadAudioMap)
			{
				// Attempt to grab a player, first
				BackgroundAudio.Player player = pair.Key.PlayerManager.GetPlayer(pair.Value.File);
				if (player == null)
				{
					// Create a new player, and retrieve the new one
					yield return pair.Key.PlayerManager.CreatePlayer(pair.Value.File);
					player = pair.Key.PlayerManager.GetPlayer(pair.Value.File);
				}

				// Set the player variable
				pair.Value.Player = player;
			}

			// Fade the currently playing players out
			FadeOut(Music, fadeInArgs?.FadeOut);
			FadeOut(Ambience, fadeInArgs?.FadeOut);

			// Fade the players in
			foreach (var pair in loadAudioMap)
			{
				pair.Key.GroupManager.FadeIn(pair.Value.Player, fadeInArgs);
			}

			static void FadeOut(AudioLayer.Background backgroundAudio, FadeOutArgs fadeOutArgs)
			{
				// Fade the currently playing players out
				BackgroundAudio.Player[] fadingPlayers = backgroundAudio.GroupManager.GetManagedPlayers();
				foreach (var fadingPlayer in fadingPlayers)
				{
					backgroundAudio.GroupManager.FadeOut(fadingPlayer, fadeOutArgs);
				}
			}
		}

		class AudioSettingsManager : BaseSettingsManager<AudioSettingsManager, AudioSettings>
		{
			enum SnapshotType
			{
				Default = 0,
				Paused,
				Slow,
				Quicken,
				NumberOfTypes
			}

			class SnapshotSet
			{
				public AudioMixerSnapshot[] snapshots = null;
				public float[] weights = null;
				public readonly ListSet<SnapshotType> indexToType = new((int)SnapshotType.NumberOfTypes);
			}

			Data.Status status = Global.Settings.Data.Status.Fail;
			SnapshotSet[] snapshotSettings = null;

			/// <inheritdoc/>
			protected override string AddressableName => ADDRESSABLE_NAME;

			/// <inheritdoc/>
			public override Data.Status GetStatus() => status;

			protected override IEnumerator OnSetup()
			{
				// Reset status
				status = Global.Settings.Data.Status.Loading;

				// Setup dependencies
				yield return Manager.StartCoroutine(SavesManager.Setup());
				if (SavesManager.Status == Global.Settings.Data.Status.Fail)
				{
					// Indicate dependencies failed to load
					status = Global.Settings.Data.Status.Fail;
					yield break;
				}

				// Setup data
				yield return Manager.StartCoroutine(base.OnSetup());

				// Update status (the rest of the code assumes AudioManager setup is finished)
				status = base.GetStatus();

				// Setup volume to current settings
				Data.Main.Setup();
				Data.Music.Setup();
				Data.SoundEffects.Setup();
				Data.Voices.Setup();
				Data.Ambience.Setup();

				// Setup music stacks
				SetupBackgroundLayer(Data.Music, "Music Stack");
				SetupBackgroundLayer(Data.Ambience, "Ambience Stack");

				// Force this game object to be active
				gameObject.SetActive(true);

				// Update snapshots and AudioListener
				UpdateSnapshots(TimeManager.TimeScale, TimeManager.IsManuallyPaused);
				AudioListener.pause = TimeManager.IsManuallyPaused;

				// Listen to the TimeManager event
				TimeManager.OnAfterTimeScaleChanged += OnTimeScaleChanged;
				TimeManager.OnAfterIsManuallyPausedChanged += OnPauseChanged;

				void SetupBackgroundLayer(AudioLayer.Background layer, string gameObjectName)
				{
					layer.PlayerManager = AudioPlayerManager.Create(transform, gameObjectName);
					layer.GroupManager = new MixerGroupManager(layer.PlayerManager, Data.PercentToDbCurve, layer.FadeLayers);
				}
			}

			protected override void OnDestroy()
			{
				// Unsubscribe to events
				Data.Main.Dispose();
				Data.Music.Dispose();
				Data.SoundEffects.Dispose();
				Data.Voices.Dispose();
				Data.Ambience.Dispose();

				// Check the TimeManager event
				TimeManager.OnAfterTimeScaleChanged -= OnTimeScaleChanged;
				TimeManager.OnAfterIsManuallyPausedChanged -= OnPauseChanged;

				// Call destroy
				base.OnDestroy();
			}

			void OnPauseChanged(bool _, bool newValue)
			{
				UpdateSnapshots(TimeManager.TimeScale, newValue);
				AudioListener.pause = newValue;
			}

			void OnTimeScaleChanged(float _, float newValue) => UpdateSnapshots(newValue, TimeManager.IsManuallyPaused);

			void UpdateSnapshots(float currentTimeScale, bool isPaused)
			{
				// Check if there are any time scale snapshots to adjust
				AudioSettings settings = GetData();
				if (settings.TimeScaleSnapshots.Length == 0)
				{
					return;
				}

				// Setup cache, if it hasn't been already
				if (snapshotSettings == null)
				{
					SetupCache(settings);
				}

				for (int i = 0; i < settings.TimeScaleSnapshots.Length; ++i)
				{
					// Calculate the weight distribution for each snapshot
					TimeScaleAudioModifiers modifier = settings.TimeScaleSnapshots[i];
					GetWeight(currentTimeScale, isPaused, in modifier, out SnapshotType weightFor, out float weight);

					// The first weight is always default.
					// Check to see if weight is for non-default state that user enabled (contained in indexToType.)
					snapshotSettings[i].weights[0] = 1f;
					if ((weightFor != SnapshotType.Default) && snapshotSettings[i].indexToType.Contains(weightFor))
					{
						// If so, scale default so it counter-balances the other state
						snapshotSettings[i].weights[0] = 1f - weight;
					}

					// Set the weights for the rest of the snapshots
					for (int j = 1; j < snapshotSettings[i].indexToType.Count; ++j)
					{
						SnapshotType type = snapshotSettings[i].indexToType[j];
						snapshotSettings[i].weights[j] = (type == weightFor) ? weight : 0f;
					}

					// Update all mixer with snapshot blend states
					AudioMixer mixer = modifier.Mixer;
					if (mixer == null)
					{
						mixer = settings.Mixer;
					}
					mixer.TransitionToSnapshots(snapshotSettings[i].snapshots, snapshotSettings[i].weights, 0f);

					// Update pitch
					string paramName = settings.TimeScaleSnapshots[i].PitchParam;
					if (string.IsNullOrEmpty(paramName) == false)
					{
						mixer.SetFloat(paramName, GetPitch(currentTimeScale, in modifier));
					}
				}

				void SetupCache(AudioSettings settings)
				{
					// Setup the map
					snapshotSettings = new SnapshotSet[settings.TimeScaleSnapshots.Length];
					for (int i = 0; i < snapshotSettings.Length; ++i)
					{
						// Just map the snapshots per category
						snapshotSettings[i] = new();
						ListSet<SnapshotType> indexToType = snapshotSettings[i].indexToType;
						TimeScaleAudioModifiers modifiers = settings.TimeScaleSnapshots[i];
						AddType(indexToType, SnapshotType.Default, true, modifiers.DefaultSnapshot);
						AddType(indexToType, SnapshotType.Paused, modifiers.EnablePause, modifiers.PausedSnapshot);
						AddType(indexToType, SnapshotType.Slow, modifiers.EnableSlow, modifiers.SlowTimeSnapshot);
						AddType(indexToType, SnapshotType.Quicken, modifiers.EnableFast, modifiers.FastTimeSnapshot);

						// Setup all arrays
						snapshotSettings[i].snapshots = new AudioMixerSnapshot[indexToType.Count];
						snapshotSettings[i].weights = new float[indexToType.Count];

						for (int j = 0; j < indexToType.Count; ++j)
						{
							snapshotSettings[i].snapshots[j] = GetSnapshot(indexToType[j], in settings.TimeScaleSnapshots[i]);
							snapshotSettings[i].weights[j] = (indexToType[j] == SnapshotType.Default) ? 1 : 0;
						}
					}

					void AddType(ListSet<SnapshotType> set, SnapshotType key, bool isFeatureEnabled, AudioMixerSnapshot snapshot)
					{
						if (isFeatureEnabled && (snapshot != null))
						{
							set.Add(key);
						}
						else if (key == SnapshotType.Default)
						{
							throw new System.ArgumentNullException(nameof(snapshot), "Default snapshot is strictly required, and cannot be null");
						}
					}

					AudioMixerSnapshot GetSnapshot(SnapshotType key, in TimeScaleAudioModifiers modifier)
					{
						switch (key)
						{
							case SnapshotType.Paused:
								return modifier.PausedSnapshot;
							case SnapshotType.Slow:
								return modifier.SlowTimeSnapshot;
							case SnapshotType.Quicken:
								return modifier.FastTimeSnapshot;
							default:
								return modifier.DefaultSnapshot;
						}
					}
				}

				void GetWeight(float currentTimeScale, bool isPaused, in TimeScaleAudioModifiers settings, out SnapshotType type, out float weight)
				{
					// Check time scale state
					if (isPaused)
					{
						// If paused, weigh pause snapshot to fullest
						type = SnapshotType.Paused;
						weight = 1f;
					}
					else if (Mathf.Approximately(currentTimeScale, 1f))
					{
						// If normal, weigh default snapshot to fullest
						type = SnapshotType.Default;
						weight = 1f;
					}
					else if (currentTimeScale < 1f)
					{
						type = SnapshotType.Slow;
						weight = 1f - Mathf.InverseLerp(settings.SlowTimeRange.x, settings.SlowTimeRange.y, currentTimeScale);
					}
					else
					{
						type = SnapshotType.Quicken;
						weight = Mathf.InverseLerp(settings.FastTimeRange.x, settings.FastTimeRange.y, currentTimeScale);
					}
				}

				float GetPitch(float currentTimeScale, in TimeScaleAudioModifiers settings)
				{
					// Check time scale state
					if (Mathf.Approximately(currentTimeScale, 1f))
					{
						return 1f;
					}
					else if (currentTimeScale < 1f)
					{
						if (settings.EnableSlow == false)
						{
							return 1f;
						}

						// Compute where the timescale is in this range
						float time = Mathf.InverseLerp(settings.SlowTimeRange.x, settings.SlowTimeRange.y, currentTimeScale);

						// Compute the pitch
						return Mathf.Lerp(settings.SlowPitchRange.x, settings.SlowPitchRange.y, time);
					}
					else
					{
						if (settings.EnableFast == false)
						{
							return 1f;
						}

						// Compute where the timescale is in this range
						float time = Mathf.InverseLerp(settings.FastTimeRange.x, settings.FastTimeRange.y, currentTimeScale);

						// Compute the pitch
						return Mathf.Lerp(settings.FastPitchRange.x, settings.FastPitchRange.y, time);
					}
				}
			}
		}
	}
}
