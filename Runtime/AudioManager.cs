using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
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
		public static MusicDataStack BackgroundMusicHistory => AudioSettingsManager.GetInstanceOrThrow().BackgroundMusicStack;
		/// <summary>
		/// TODO
		/// </summary>
		public static MusicDataStack BackgroundAmbienceHistory => AudioSettingsManager.GetInstanceOrThrow().BackgroundAmbienceStack;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="percent"></param>
		/// <returns></returns>
		public static float ConvertPercentToVolumeDb(float percent) =>
			AudioSettingsManager.GetDataOrThrow().PercentToDbCurve.Evaluate(percent);

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
			/// <summary>
			/// TODO
			/// </summary>
			public MusicDataStack BackgroundMusicStack
			{
				get;
				private set;
			} = null;
			/// <summary>
			/// TODO
			/// </summary>
			public MusicDataStack BackgroundAmbienceStack
			{
				get;
				private set;
			} = null;

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
				BackgroundMusicStack = new MusicDataStack(this, Data.MusicSetup, Data.PercentToDbCurve, "Music Stack");
				BackgroundAmbienceStack = new MusicDataStack(this, Data.AmbienceSetup, Data.PercentToDbCurve, "Ambience Stack");

				// Force this game object to be active
				gameObject.SetActive(true);

				// Update snapshots and AudioListener
				UpdateSnapshots(TimeManager.TimeScale, TimeManager.IsManuallyPaused);
				AudioListener.pause = TimeManager.IsManuallyPaused;

				// Listen to the TimeManager event
				TimeManager.OnAfterTimeScaleChanged += OnTimeScaleChanged;
				TimeManager.OnAfterIsManuallyPausedChanged += OnPauseChanged;
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
