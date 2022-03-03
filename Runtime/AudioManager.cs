using System.Collections;
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
		/// TODO
		/// </summary>
		/// <returns></returns>
		public static IEnumerator Setup()
		{
			yield return Manager.StartCoroutine(AudioSettingsManager.WaitUntilReady());
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioMixer Mixer => AudioSettingsManager.GetData().Mixer;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer Main => AudioSettingsManager.GetData().Main;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Background Music => AudioSettingsManager.GetData().Music;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Spatial SoundEffects => AudioSettingsManager.GetData().SoundEffects;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Spatial Voices => AudioSettingsManager.GetData().Voices;
		/// <summary>
		/// TODO
		/// </summary>
		public static AudioLayer.Background Ambience => AudioSettingsManager.GetData().Ambience;
		/// <summary>
		/// TODO
		/// </summary>
		public static float MuteVolumeDb => AudioSettingsManager.GetData().MuteVolumeDb;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="percent"></param>
		/// <returns></returns>
		public static float ConvertPercentToVolumeDb(float percent) => AudioSettingsManager.GetData().PercentToDbCurve.Evaluate(percent);

		class AudioSettingsManager : BaseSettingsManager<AudioSettingsManager, AudioSettings>
		{
			/// <inheritdoc/>
			protected override string AddressableName => ADDRESSABLE_NAME;

			protected override IEnumerator OnSetup()
			{
				// Setup dependencies
				yield return Manager.StartCoroutine(SavesManager.Setup());

				// Setup data
				yield return Manager.StartCoroutine(base.OnSetup());

				// Retrieve settings
				AudioSettings audioData = GetData();

				// Setup vaolume to current settings
				audioData.Main.Setup();
				audioData.Music.Setup();
				audioData.SoundEffects.Setup();
				audioData.Voices.Setup();
				audioData.Ambience.Setup();

				// Check the TimeManager event
				OnPauseChanged(false, TimeManager.IsManuallyPaused);
				TimeManager.OnAfterIsManuallyPausedChanged += OnPauseChanged;

				// Pause a frame
				yield return null;
			}

			protected override void OnDestroy()
			{
				// Retrieve settings
				AudioSettings audioData = GetData();

				// Unsubscribe to events
				audioData.Main.Dispose();
				audioData.Music.Dispose();
				audioData.SoundEffects.Dispose();
				audioData.Voices.Dispose();
				audioData.Ambience.Dispose();

				// Check the TimeManager event
				TimeManager.OnAfterIsManuallyPausedChanged -= OnPauseChanged;

				// Call destroy
				base.OnDestroy();
			}

			void OnPauseChanged(bool _, bool newValue)
			{
				AudioSettings settings = GetData();
				if (string.IsNullOrEmpty(settings.DuckParam) == false)
				{
					if (newValue == true)
					{
						settings.Mixer.SetFloat(settings.DuckParam, 0f);
					}
					else
					{
						settings.Mixer.SetFloat(settings.DuckParam, MuteVolumeDb);
					}
				}
			}
		}
	}
}
