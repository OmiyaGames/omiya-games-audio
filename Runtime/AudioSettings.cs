using UnityEngine;
using UnityEngine.Audio;
using OmiyaGames.Global.Settings;

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
		[SerializeField]
		float muteVolumeDb = -80;
		[SerializeField]
		AnimationCurve timeToVolumeDbCurve = new(new(0, -40), new(1, 0));

		[Header("Volume Settings")]
		[SerializeField]
		string mainVolume = "Main Volume";
		[SerializeField]
		string musicVolume = "Music Volume";
		[SerializeField]
		string soundEffectsVolume ="Sound Effects Volume";
		[SerializeField]
		string voiceVolume = "Voice Volume";
		[SerializeField]
		string ambienceVolume = "Ambience Volume";

		[Header("Pitch Settings")]
		[SerializeField]
		string mainPitch = "Main Pitch";
		[SerializeField]
		string musicPitch = "Music Pitch";
		[SerializeField]
		string soundEffectsPitch = "Sound Effects Pitch";
		[SerializeField]
		string voicePitch = "Voice Pitch";
		[SerializeField]
		string ambiencePitch = "Ambience Pitch";

		[Header("Effect Settings")]
		[SerializeField]
		string duckingLevel = "Duck Level";

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
		public string VoiceVolume => voiceVolume;
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
		public string VoicePitch => voicePitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string AmbiencePitch => ambiencePitch;
		/// <summary>
		/// TODO
		/// </summary>
		public string DuckingLevel => duckingLevel;

		/// <inheritdoc/>
		protected override bool OnUpgrade(int oldVersion, out string errorMessage)
		{
			// Implementing as a reminder this method exists
			return base.OnUpgrade(oldVersion, out errorMessage);
		}
	}
}
