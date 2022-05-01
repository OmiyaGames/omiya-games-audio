using UnityEngine;
using UnityEngine.Audio;
using OmiyaGames.Global.Settings;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AudioSettings.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 2/13/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>
	/// Initial draft.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Scriptable object with settings info.
	/// </summary>
	public class AudioSettings : BaseSettingsData
	{
		// Don't forget to update this property each time there's an upgrade to make!
		/// <inheritdoc/>
		public override int CurrentVersion => 0;

		public const string DATA_DIRECTORY = "Packages/com.omiyagames.audio/Runtime/Data/";

		// Note: many of these variable defaults are set in the Reset() method.
		[SerializeField]
		AudioMixer mixer;

		[Header("Volume Controls")]
		[SerializeField]
		float muteVolumeDb = -80;
		[SerializeField]
		AnimationCurve percentToDbCurve;

		[SerializeField]
		AudioLayer main = new();
		[SerializeField]
		AudioLayer.Background music = new();
		[SerializeField]
		AudioLayer.Spatial soundEffects = new();
		[SerializeField]
		AudioLayer.Spatial voices = new();
		[SerializeField]
		AudioLayer.Background ambience = new();

		[SerializeField]
		MusicDataStack.Settings musicSetup;
		[SerializeField]
		MusicDataStack.Settings ambienceSetup;

		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs("timeScaleSnapshots")]
		TimeScaleAudioModifiers[] timeScaleEffects;

		/// <summary>
		/// The main mixer of this game.
		/// </summary>
		public AudioMixer Mixer => mixer;
		/// <summary>
		/// The volume in which <see cref="Mixer"/>
		/// interprets as mute.
		/// </summary>
		public float MuteVolumeDb => muteVolumeDb;
		/// <summary>
		/// TODO
		/// </summary>
		public AnimationCurve PercentToDbCurve => percentToDbCurve;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioLayer Main => main;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioLayer.Background Music => music;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioLayer.Spatial SoundEffects => soundEffects;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioLayer.Spatial Voices => voices;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioLayer.Background Ambience => ambience;
		/// <summary>
		/// TODO
		/// </summary>
		public TimeScaleAudioModifiers[] TimeScaleSnapshots => timeScaleEffects;
		/// <summary>
		/// TODO
		/// </summary>
		[System.Obsolete("To be replaced with AudioLayer.Background.fadeLayers")]
		public MusicDataStack.Settings MusicSetup => musicSetup;
		/// <summary>
		/// TODO
		/// </summary>
		[System.Obsolete("To be replaced with AudioLayer.Background.fadeLayers")]
		public MusicDataStack.Settings AmbienceSetup => ambienceSetup;

		/// <inheritdoc/>
		protected override bool OnUpgrade(int oldVersion, out string errorMessage)
		{
			// Implementing as a reminder this method exists
			return base.OnUpgrade(oldVersion, out errorMessage);
		}

		/// <summary>
		/// The default percent to decibels converter,
		/// assuming mute-decibel is -80 dB.
		/// </summary>
		/// <param name="percent">
		/// Value from 0 to 1.
		/// </param>
		/// <returns>
		/// Volume, in decibels, where -80 dB is mute.
		/// </returns>
		public static float DefaultPercentToVolumeDbConversion(float percent)
		{
			const float MIN_PERCENT_CLAMP = 0.0001f;
			percent = Mathf.Clamp(percent, MIN_PERCENT_CLAMP, 1f);
			return Mathf.Log10(percent) * 20;
		}

#if UNITY_EDITOR
		void Reset()
		{
			const string DEFAULT_SETTINGS_PATH = DATA_DIRECTORY + "Audio Settings - Default.asset";
			const int NUM_KEYFRAMES = 15;

			// Grab the default settings
			AudioSettings defaultSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioSettings>(DEFAULT_SETTINGS_PATH);

			// Copy all the member variables
			mixer = defaultSettings.mixer;
			
			muteVolumeDb = defaultSettings.muteVolumeDb;
			percentToDbCurve = CreateDefaultCurve(NUM_KEYFRAMES);

			main = defaultSettings.main;
			music = defaultSettings.music;
			soundEffects = defaultSettings.soundEffects;
			voices = defaultSettings.voices;
			ambience = defaultSettings.ambience;

			timeScaleEffects = defaultSettings.timeScaleEffects;
		}

		static AnimationCurve CreateDefaultCurve(int numberOfKeyFrames)
		{
			const float REDUCTION_MULTIPLIER = 0.65f;

			// Generate the first keyframe (at time 0)
			Keyframe[] defaultKeyframes = new Keyframe[numberOfKeyFrames];
			defaultKeyframes[0] = new(0, DefaultPercentToVolumeDbConversion(0));

			// Generate the rest (1 through almost 0)
			float time = 1f;
			for (int i = 1; i < defaultKeyframes.Length; ++i)
			{
				defaultKeyframes[i] = new(time, DefaultPercentToVolumeDbConversion(time));
				defaultKeyframes[i].weightedMode = WeightedMode.None;

				// Calculate the next time
				time *= REDUCTION_MULTIPLIER;
			}

			// Apply keyframes to the curve
			AnimationCurve returnCurve = new(defaultKeyframes);

			// Change tangent mode on all the keyframes
			for (int i = 0; i < numberOfKeyFrames; ++i)
			{
				UnityEditor.AnimationUtility.SetKeyLeftTangentMode(returnCurve, i, UnityEditor.AnimationUtility.TangentMode.ClampedAuto);
				UnityEditor.AnimationUtility.SetKeyRightTangentMode(returnCurve, i, UnityEditor.AnimationUtility.TangentMode.ClampedAuto);
			}
			return returnCurve;
		}
#endif
	}
}
