using UnityEngine;
using UnityEngine.Audio;
using OmiyaGames.Global.Settings;
using OmiyaGames.Saves;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
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

		// Note: many of these variable defaults are set in the Reset() method.
		[SerializeField]
		AudioMixer mixer;

		[Header("Volume Controls")]
		[SerializeField]
		float muteVolumeDb = -80;
		[SerializeField]
		AnimationCurve percentToDbCurve;

		[Header("Exposed Parameter Names")]
		[SerializeField]
		string duckParam = "Duck Level";

		[SerializeField]
		Layer main = new();
		[SerializeField]
		Layer.Background music = new();
		[SerializeField]
		Layer.Spatial soundEffects = new();
		[SerializeField]
		Layer.Spatial voices = new();
		[SerializeField]
		Layer.Background ambience = new();

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
		public Layer Main => main;
		/// <summary>
		/// TODO
		/// </summary>
		public Layer.Background Music => music;
		/// <summary>
		/// TODO
		/// </summary>
		public Layer.Spatial SoundEffects => soundEffects;
		/// <summary>
		/// TODO
		/// </summary>
		public Layer.Spatial Voices => voices;
		/// <summary>
		/// TODO
		/// </summary>
		public Layer.Background Ambience => ambience;
		/// <summary>
		/// TODO
		/// </summary>
		public string DuckParam => duckParam;

		/// <inheritdoc/>
		protected override bool OnUpgrade(int oldVersion, out string errorMessage)
		{
			// Implementing as a reminder this method exists
			return base.OnUpgrade(oldVersion, out errorMessage);
		}

#if UNITY_EDITOR
		void Reset()
		{
			const string DATA_DIRECTORY = "Packages/com.omiyagames.audio/Runtime/Data/";
			const string DEFAULT_MIXER_PATH = DATA_DIRECTORY + "DefaultMixer.mixer";
			const float DEFAULT_FADE_DURATION = 0.25f;
			const int NUM_KEYFRAMES = 15;

			const string MAIN_VOLUME_PATH = DATA_DIRECTORY + "MainVolumeSetting.asset";
			const string MAIN_MUTE_PATH = DATA_DIRECTORY + "MainMuteSetting.asset";

			const string MUSIC_VOLUME_PATH = DATA_DIRECTORY + "MusicVolumeSetting.asset";
			const string MUSIC_MUTE_PATH = DATA_DIRECTORY + "MusicMuteSetting.asset";

			const string SFX_VOLUME_PATH = DATA_DIRECTORY + "SoundEffectsVolumeSetting.asset";
			const string SFX_MUTE_PATH = DATA_DIRECTORY + "SoundEffectsMuteSetting.asset";

			const string VOICES_VOLUME_PATH = DATA_DIRECTORY + "VoicesVolumeSetting.asset";
			const string VOICES_MUTE_PATH = DATA_DIRECTORY + "VoicesMuteSetting.asset";

			const string AMBIENCE_VOLUME_PATH = DATA_DIRECTORY + "AmbienceVolumeSetting.asset";
			const string AMBIENCE_MUTE_PATH = DATA_DIRECTORY + "AmbienceMuteSetting.asset";

			// Setup mixer
			mixer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioMixer>(DEFAULT_MIXER_PATH);

			// Setup main layer
			main.VolumeParam = "Main Volume";
			main.PitchParam = "Main Pitch";
			main.VolumeSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(MAIN_VOLUME_PATH);
			main.IsMutedSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(MAIN_MUTE_PATH);

			// Setup music layer
			music.VolumeParam = "Music Volume";
			music.PitchParam = "Music Pitch";
			music.VolumeSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(MUSIC_VOLUME_PATH);
			music.IsMutedSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(MUSIC_MUTE_PATH);
			music.DefaultGroup = mixer.FindMatchingGroups("World Music")[0];
			music.DefaultFadeDurationSeconds = DEFAULT_FADE_DURATION;
			music.FadeGroups = GetFadeGroups(Mixer, "Fade Music");

			// Setup sound effects layer
			soundEffects.VolumeParam = "Sound Effects Volume";
			soundEffects.PitchParam = "Sound Effects Pitch";
			soundEffects.VolumeSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(SFX_VOLUME_PATH);
			soundEffects.IsMutedSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(SFX_MUTE_PATH);
			soundEffects.DefaultGroup = mixer.FindMatchingGroups("World Sound Effects")[0];
			soundEffects.DefaultUiGroup = mixer.FindMatchingGroups("UI Sound Effects")[0];

			// Setup voices layer
			voices.VolumeParam = "Voices Volume";
			voices.PitchParam = "Voices Pitch";
			voices.VolumeSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(VOICES_VOLUME_PATH);
			voices.IsMutedSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(VOICES_MUTE_PATH);
			voices.DefaultGroup = mixer.FindMatchingGroups("World Voices")[0];
			voices.DefaultUiGroup = mixer.FindMatchingGroups("UI Voices")[0];

			// Setup ambience layer
			ambience.VolumeParam = "Ambience Volume";
			ambience.PitchParam = "Ambience Pitch";
			ambience.VolumeSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(AMBIENCE_VOLUME_PATH);
			ambience.IsMutedSaver = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(AMBIENCE_MUTE_PATH);
			ambience.DefaultGroup = mixer.FindMatchingGroups("World Ambience")[0];
			ambience.DefaultFadeDurationSeconds = DEFAULT_FADE_DURATION;
			ambience.FadeGroups = GetFadeGroups(Mixer, "Fade Ambience");

			// Setup the keyframes
			percentToDbCurve = CreateDefaultCurve(NUM_KEYFRAMES);

			static AudioMixerGroup[] GetFadeGroups(AudioMixer mixer, string prepend)
			{
				const int NUM_GROUPS = 3;
				AudioMixerGroup[] toReturn = new AudioMixerGroup[NUM_GROUPS];
				System.Text.StringBuilder builder = new(prepend.Length + 2);

				// Find all groups
				for (int i = 0; i < NUM_GROUPS; ++i)
				{
					// Generate name of group
					builder.Clear();
					builder.Append(prepend);
					builder.Append(' ');
					builder.Append(i + 1);

					// Find this group
					toReturn[i] = mixer.FindMatchingGroups(builder.ToString())[0];
				}
				return toReturn;
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

				static float DefaultPercentToVolumeDbConversion(float percent)
				{
					const float MIN_PERCENT_CLAMP = 0.0001f;
					percent = Mathf.Clamp(percent, MIN_PERCENT_CLAMP, 1f);
					return Mathf.Log10(percent) * 20;
				}
			}
		}
#endif
	}
}
