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

		[SerializeField]
		AudioMixer mixer = null;

		[Header("Volume Controls")]
		[SerializeField]
		float muteVolumeDb = -80;
		[SerializeField]
		AnimationCurve timeToVolumeDbCurve = new(new(0, -40, 106.4f, 106.4f), new(1, 0, 0, 0));

		[Header("Volume Fields")]
		[SerializeField]
		string mainVolume = "Main Volume";
		[SerializeField]
		string musicVolume = "Music Volume";
		[SerializeField]
		string soundEffectsVolume ="Sound Effects Volume";
		[SerializeField]
		string voicesVolume = "Voices Volume";
		[SerializeField]
		string ambienceVolume = "Ambience Volume";

		[Header("Pitch Fields")]
		[SerializeField]
		string mainPitch = "Main Pitch";
		[SerializeField]
		string musicPitch = "Music Pitch";
		[SerializeField]
		string soundEffectsPitch = "Sound Effects Pitch";
		[SerializeField]
		string voicesPitch = "Voices Pitch";
		[SerializeField]
		string ambiencePitch = "Ambience Pitch";

		[Header("Effect Fields")]
		[SerializeField]
		string duckLevel = "Duck Level";

		[Header("Saved Settings")]
		[SerializeField]
		SaveFloat mainVolumeSettings;
		[SerializeField]
		SaveBool mainMuteSettings;
		[SerializeField]
		SaveFloat musicVolumeSettings;
		[SerializeField]
		SaveBool musicMuteSettings;
		[SerializeField]
		SaveFloat soundEffectsVolumeSettings;
		[SerializeField]
		SaveBool soundEffectsMuteSettings;
		[SerializeField]
		SaveFloat voicesVolumeSettings;
		[SerializeField]
		SaveBool voicesMuteSettings;
		[SerializeField]
		SaveFloat ambienceVolumeSettings;
		[SerializeField]
		SaveBool ambienceMuteSettings;

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
		public AnimationCurve TimeToVolumeDbCurve => timeToVolumeDbCurve;
		/// <summary>
		/// TODO
		/// </summary>
		public string MainVolume => mainVolume;
		/// <summary>
		/// TODO
		/// </summary>
		public string MusicVolume => musicVolume;
		/// <summary>
		/// TODO
		/// </summary>
		public string SoundEffectsVolume => soundEffectsVolume;
		/// <summary>
		/// TODO
		/// </summary>
		public string VoicesVolume => voicesVolume;
		/// <summary>
		/// TODO
		/// </summary>
		public string AmbienceVolume => ambienceVolume;
		/// <summary>
		/// TODO
		/// </summary>
		public string MainPitch => mainPitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string MusicPitch => musicPitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string SoundEffectsPitch => soundEffectsPitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string VoicesPitch => voicesPitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string AmbiencePitch => ambiencePitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string DuckingLevel => duckLevel;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveFloat MainVolumeSettings => mainVolumeSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveBool MainMuteSettings => mainMuteSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveFloat MusicVolumeSettings => musicVolumeSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveBool MusicMuteSettings => musicMuteSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveFloat SoundEffectsVolumeSettings => soundEffectsVolumeSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveBool SoundEffectsMuteSettings => soundEffectsMuteSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveFloat VoicesVolumeSettings => voicesVolumeSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveBool VoicesMuteSettings => voicesMuteSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveFloat AmbienceVolumeSettings => ambienceVolumeSettings;
		/// <summary>
		/// TODO
		/// </summary>
		public SaveBool AmbienceMuteSettings => ambienceMuteSettings;

		/// <inheritdoc/>
		protected override bool OnUpgrade(int oldVersion, out string errorMessage)
		{
			// Implementing as a reminder this method exists
			return base.OnUpgrade(oldVersion, out errorMessage);
		}

#if UNITY_EDITOR
		const string DATA_DIRECTORY = "Packages/com.omiyagames.audio/Runtime/Data/";
		const string DEFAULT_MIXER_PATH = DATA_DIRECTORY + "DefaultMixer.mixer";
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

		void Reset()
		{
			mixer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioMixer>(DEFAULT_MIXER_PATH);
			
			mainVolumeSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(MAIN_VOLUME_PATH);
			mainMuteSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(MAIN_MUTE_PATH);

			musicVolumeSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(MUSIC_VOLUME_PATH);
			musicMuteSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(MUSIC_MUTE_PATH);
			
			soundEffectsVolumeSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(SFX_VOLUME_PATH);
			soundEffectsMuteSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(SFX_MUTE_PATH);

			voicesVolumeSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(VOICES_VOLUME_PATH);
			voicesMuteSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(VOICES_MUTE_PATH);

			ambienceVolumeSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveFloat>(AMBIENCE_VOLUME_PATH);
			ambienceMuteSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBool>(AMBIENCE_MUTE_PATH);

			// TODO: consider adding these settings into the save settings as well.
		}
#endif
	}
}
