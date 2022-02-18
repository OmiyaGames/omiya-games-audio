using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using OmiyaGames.Saves;
using OmiyaGames.Managers;
using OmiyaGames.Global;
using OmiyaGames.Global.Settings;

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
	public class AudioManager : BaseSettingsManager<AudioManager, AudioSettings>
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

		/// <inheritdoc/>
		protected override string AddressableName => ADDRESSABLE_NAME;
		/// <summary>
		/// TODO
		/// </summary>
		public static float MuteVolumeDb => GetData().MuteVolumeDb;
		/// <summary>
		/// TODO
		/// </summary>
		public static float MainVolumeDb
		{
			get
			{
				AudioSettings setting = GetData();
				return GetVolumeDb(setting, setting.MainVolume);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float MusicVolumeDb
		{
			get
			{
				AudioSettings setting = GetData();
				return GetVolumeDb(setting, setting.MusicVolume);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float SoundEffectsVolumeDb
		{
			get
			{
				AudioSettings setting = GetData();
				return GetVolumeDb(setting, setting.SoundEffectsVolume);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float VoiceVolumeDb
		{
			get
			{
				AudioSettings setting = GetData();
				return GetVolumeDb(setting, setting.VoiceVolume);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float AmbienceVolumeDb
		{
			get
			{
				AudioSettings setting = GetData();
				return GetVolumeDb(setting, setting.AmbienceVolume);
			}
		}

		// FIXME: convert this to an ITrackable
		/// <summary>
		/// TODO
		/// </summary>
		public static float MainVolumePercent
		{
			get
			{
				// FIXME: this is incorrect
				AudioSettings setting = GetData();
				return GetVolumeDb(setting, setting.MainVolume);
			}
			set
			{
				AudioSettings setting = GetData();
				float volumeDecibels = Mathf.Clamp01(value);
				volumeDecibels = ConvertPercentToVolumeDb(setting, volumeDecibels);
				SetMixerFloat(setting, setting.MainVolume, volumeDecibels);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static float MainPitch
		{
			get
			{
				AudioSettings setting = GetData();
				return GetPitch(setting, setting.MainPitch);
			}
			set
			{
				AudioSettings setting = GetData();
				SetMixerFloat(setting, setting.MainPitch, value);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float MusicPitch
		{
			get
			{
				AudioSettings setting = GetData();
				return GetPitch(setting, setting.MusicPitch);
			}
			set
			{
				AudioSettings setting = GetData();
				SetMixerFloat(setting, setting.MusicPitch, value);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float SoundEffectsPitch
		{
			get
			{
				AudioSettings setting = GetData();
				return GetPitch(setting, setting.SoundEffectsPitch);
			}
			set
			{
				AudioSettings setting = GetData();
				SetMixerFloat(setting, setting.SoundEffectsPitch, value);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float VoicePitch
		{
			get
			{
				AudioSettings setting = GetData();
				return GetPitch(setting, setting.VoicePitch);
			}
			set
			{
				AudioSettings setting = GetData();
				SetMixerFloat(setting, setting.VoicePitch, value);
			}
		}
		/// <summary>
		/// TODO
		/// </summary>
		public static float AmbiencePitch
		{
			get
			{
				AudioSettings setting = GetData();
				return GetPitch(setting, setting.AmbiencePitch);
			}
			set
			{
				AudioSettings setting = GetData();
				SetMixerFloat(setting, setting.AmbiencePitch, value);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="percent"></param>
		/// <returns></returns>
		public static float ConvertPercentToVolumeDb(float percent) => ConvertPercentToVolumeDb(GetData(), percent);

		// Start is called before the first frame update
		void Start()
		{
			// Retrieve settings
			AudioSettings audioData = GetData();
			GameSettings saveData = Singleton.Get<GameSettings>();
			if (saveData != null)
			{
				UpdateVolumeDb(audioData, audioData.MusicVolume, saveData.MusicVolume, saveData.IsMusicMuted);
				UpdateVolumeDb(audioData, audioData.SoundEffectsVolume, saveData.SoundVolume, saveData.IsSoundMuted);
			}

			// Check the TimeManager event
			TimeManager.OnAfterManualPauseChanged += OnPauseChanged;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			TimeManager.OnAfterManualPauseChanged -= OnPauseChanged;
		}

		#region Helper Methods
		static float GetVolumeDb(AudioSettings setting, string fieldName) => GetMixerFloat(setting, fieldName, setting.MuteVolumeDb);

		static float GetPitch(AudioSettings setting, string fieldName) => GetMixerFloat(setting, fieldName, 1);

		static float GetMixerFloat(AudioSettings setting, string fieldName, float defaultValue)
		{
			float returnValue = defaultValue;
			if (string.IsNullOrEmpty(fieldName) == false)
			{
				setting.Mixer.GetFloat(fieldName, out returnValue);
			}
			return returnValue;
		}

		static float ConvertPercentToVolumeDb(AudioSettings settings, float percent) => settings.TimeToVolumeDbCurve.Evaluate(percent);

		static void SetMixerFloat(AudioSettings setting, string fieldName, float newValue)
		{
			if (string.IsNullOrEmpty(fieldName) == false)
			{
				setting.Mixer.SetFloat(fieldName, newValue);
			}
		}

		static void UpdateVolumeDb(AudioSettings setting, string fieldName, float volumePercent, bool isMuted) => SetMixerFloat(setting, fieldName, (isMuted ? MuteVolumeDb : volumePercent));

		void OnPauseChanged(TimeManager pauseCheck)
		{
			AudioSettings settings = GetData();
			if (string.IsNullOrEmpty(settings.DuckingLevel) == false)
			{
				if (TimeManager.IsManuallyPaused == true)
				{
					settings.Mixer.SetFloat(settings.DuckingLevel, 0f);
				}
				else
				{
					settings.Mixer.SetFloat(settings.DuckingLevel, MuteVolumeDb);
				}
			}
		}
		#endregion
	}
}
