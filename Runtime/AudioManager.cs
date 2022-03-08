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
	/// </description>
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
				public readonly AudioMixerSnapshot[] cache = new AudioMixerSnapshot[(int)SnapshotType.NumberOfTypes];
				public readonly float[] weights = new float[(int)SnapshotType.NumberOfTypes];
			}

			Data.Status status = Global.Settings.Data.Status.Fail;
			SnapshotSet[] snapshots = null;

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
				if (snapshots == null)
				{
					SetupCache(settings);
				}

				for (int i = 0; i < settings.TimeScaleSnapshots.Length; ++i)
				{
					TimeScaleAudioModifiers modifier = settings.TimeScaleSnapshots[i];
					for (int j = 0; j < (int)SnapshotType.NumberOfTypes; ++j)
					{
						// Set the weight of the snapshot
						snapshots[i].weights[j] = GetWeight(currentTimeScale, isPaused, (SnapshotType)j, in modifier);
					}

					// Update all mixer with snapshot blend states
					AudioMixer mixer = modifier.Mixer;
					if (mixer == null)
					{
						mixer = settings.Mixer;
					}
					mixer.TransitionToSnapshots(snapshots[i].cache, snapshots[i].weights, 0f);

					// Update pitch
					string paramName = settings.TimeScaleSnapshots[i].PitchParam;
					if (string.IsNullOrEmpty(paramName) == false)
					{
						mixer.SetFloat(paramName, GetPitch(currentTimeScale, in modifier));
					}
				}

				void SetupCache(AudioSettings settings)
				{
					// FIXME: adjust the cache to take into account that some snapshot fields might be null
					snapshots = new SnapshotSet[settings.TimeScaleSnapshots.Length];

					for (int i = 0; i < snapshots.Length; ++i)
					{
						// Just map the snapshots per category
						snapshots[i] = new SnapshotSet();
						snapshots[i].cache[(int)SnapshotType.Default] = settings.TimeScaleSnapshots[i].DefaultSnapshot;
						snapshots[i].cache[(int)SnapshotType.Paused] = settings.TimeScaleSnapshots[i].PausedSnapshot;
						snapshots[i].cache[(int)SnapshotType.Slow] = settings.TimeScaleSnapshots[i].SlowTimeSnapshot;
						snapshots[i].cache[(int)SnapshotType.Quicken] = settings.TimeScaleSnapshots[i].QuickenTimeSnapshot;

						// Set only the default snapshot with full weight
						snapshots[i].weights[(int)SnapshotType.Default] = 1f;
					}
				}

				float GetWeight(float currentTimeScale, bool isPaused, SnapshotType type, in TimeScaleAudioModifiers settings)
				{
					// Check time scale state
					// FIXME: adjust the results to take into account that some snapshot fields might be null
					if (isPaused)
					{
						// If paused, weigh pause snapshot to fullest
						return (type == SnapshotType.Paused) ? 1f : 0;
					}
					else if (Mathf.Approximately(currentTimeScale, 1f))
					{
						// If normal, weigh pause snapshot to fullest
						return (type == SnapshotType.Default) ? 1f : 0;
					}
					else
					{
						// Grab the appropriate min-max range
						Vector2 minMaxRange = settings.QuickenTimeRange;
						if (currentTimeScale < 1f)
						{
							minMaxRange = settings.SlowTimeRange;
						}

						// Compute where the timescale is in this range
						float time = Mathf.InverseLerp(minMaxRange.x, minMaxRange.y, currentTimeScale);

						// Compute the rate
						if (currentTimeScale < 1f)
						{
							if (type == SnapshotType.Slow)
							{
								return 1f - time;
							}
							else if (type == SnapshotType.Default)
							{
								return time;
							}
						}
						else
						{
							if (type == SnapshotType.Quicken)
							{
								return time;
							}
							else if (type == SnapshotType.Default)
							{
								return 1f - time;
							}
						}
						return 0;
					}
				}

				float GetPitch(float currentTimeScale, in TimeScaleAudioModifiers settings)
				{
					// Check time scale state
					// FIXME: adjust the results to take into account whether user enabled the feature or not
					if (Mathf.Approximately(currentTimeScale, 1f))
					{
						return 1f;
					}
					else
					{
						// Grab the appropriate min-max range
						Vector2 minMaxRange = settings.QuickenTimeRange;
						if (currentTimeScale < 1f)
						{
							minMaxRange = settings.SlowTimeRange;
						}

						// Compute where the timescale is in this range
						float time = Mathf.InverseLerp(minMaxRange.x, minMaxRange.y, currentTimeScale);

						// Compute the pitch
						if (currentTimeScale < 1f)
						{
							return Mathf.Lerp(settings.LowestPitch, 1f, time);
						}
						return Mathf.Lerp(1f, settings.HighestPitch, time);
					}
				}
			}
		}
	}
}
