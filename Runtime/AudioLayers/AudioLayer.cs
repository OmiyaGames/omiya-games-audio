using System;
using UnityEngine;
using UnityEngine.Audio;
using OmiyaGames.Saves;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="Layer.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 2/27/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Base info on each audio category.
	/// </summary>
	[Serializable]
	public partial class AudioLayer : IDisposable
	{
		[SerializeField]
		[Tooltip("The SaveObject storing volume settings for this layer.")]
		SaveFloat volumeSaver;
		[SerializeField]
		[Tooltip("The SaveObject storing whether this layer is muted or not.")]
		SaveBool isMutedSaver;
		[SerializeField]
		[Tooltip("The mixer's paramer name to adjust volume.")]
		string volumeParam;
		[SerializeField]
		[Tooltip("The mixer's paramer name to adjust pitch.")]
		string pitchParam;

		/// <summary>
		/// A sublayer to <seealso cref="AudioLayer"/>
		/// </summary>
		public abstract class SubLayer : AudioLayer
		{
			[SerializeField]
			[Tooltip("The default mixer group to attach to the audio source if a script associated with this layer is created.")]
			AudioMixerGroup defaultGroup;

			/// <summary>
			/// TODO
			/// </summary>
			public AudioMixerGroup DefaultGroup
			{
				get => defaultGroup;
				internal set => defaultGroup = value;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public string VolumeParam
		{
			get => volumeParam;
			internal set => volumeParam = value;
		}
		/// <summary>
		/// TODO
		/// </summary>
		public string PitchParam
		{
			get => pitchParam;
			internal set => pitchParam = value;
		}
		/// <summary>
		/// TODO
		/// </summary>
		public SaveFloat VolumeSaver
		{
			get => volumeSaver;
			internal set => volumeSaver = value;
		}
		/// <summary>
		/// TODO
		/// </summary>
		public SaveBool IsMutedSaver
		{
			get => isMutedSaver;
			internal set => isMutedSaver = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public float VolumeDb => GetMixerFloat(VolumeParam, AudioManager.MuteVolumeDb);
		/// <summary>
		/// TODO
		/// </summary>
		public float VolumePercent
		{
			get => VolumeSaver.Value;
			set => VolumeSaver.Value = value;
		}
		/// <summary>
		/// TODO
		/// </summary>
		public float Pitch
		{
			get => GetMixerFloat(PitchParam, 1);
			set => SetMixerFloat(PitchParam, value);
		}
		/// <summary>
		/// TODO
		/// </summary>
		public bool IsMuted
		{
			get => IsMutedSaver.Value;
			set => IsMutedSaver.Value = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="settings">
		/// TODO
		/// </param>
		public void Setup()
		{
			// Setup the volume DB
			UpdateVolumeDb(VolumePercent, IsMuted);

			// Subscribe to volume and mute save changes
			VolumeSaver.OnAfterValueChanged += UpdateMainVolume;
			IsMutedSaver.OnAfterValueChanged += UpdateMainVolume;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void Dispose()
		{
			// Unsubscribe to volume and mute save changes
			VolumeSaver.OnAfterValueChanged -= UpdateMainVolume;
			IsMutedSaver.OnAfterValueChanged -= UpdateMainVolume;
		}

		#region Helper Methods
		float GetMixerFloat(string fieldName, float defaultValue)
		{
			if (string.IsNullOrEmpty(fieldName) || (AudioManager.Mixer.GetFloat(fieldName, out float returnValue) == false))
			{
				returnValue = defaultValue;
			}
			return returnValue;
		}

		void SetMixerFloat(string fieldName, float setValue)
		{
			if (string.IsNullOrEmpty(fieldName) == false)
			{
				AudioManager.Mixer.SetFloat(fieldName, setValue);
			}
		}

		void UpdateMainVolume(float _, float newVolume) => UpdateVolumeDb(newVolume, IsMuted);

		void UpdateMainVolume(bool _, bool isMute) => UpdateVolumeDb(VolumePercent, isMute);

		void UpdateVolumeDb(float volumePercent, bool isMute)
		{
			// Setup the volume DB
			float volumeDb = AudioManager.MuteVolumeDb;
			if (isMute == false)
			{
				volumeDb = AudioManager.ConvertPercentToVolumeDb(volumePercent);
			}
			SetMixerFloat(VolumeParam, volumeDb);
		}
		#endregion
	}	
}
