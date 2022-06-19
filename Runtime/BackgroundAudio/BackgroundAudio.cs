using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="BackgroundAudio.cs" company="Omiya Games">
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
	/// <strong>Version:</strong> 1.1.0-pre.1<br/>
	/// <strong>Date:</strong> 4/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// An interface for music, used to generate <seealso cref="AudioSource"/>
	/// playing a clip.
	/// </summary>
	public abstract class BackgroundAudio : ScriptableObject
	{
		/// <summary>
		/// TODO
		/// </summary>
		public enum PlayState
		{
			/// <summary>
			/// Music has been stopped.
			/// </summary>
			Stopped = 0,
			/// <summary>
			/// Music is playing.
			/// </summary>
			Playing,
			/// <summary>
			/// Music is scheduled to start playing,
			/// but haven't, yet.
			/// </summary>
			Scheduled,
			/// <summary>
			/// Music has been paused.
			/// </summary>
			//Paused,
		}

		public const int MENU_ORDER = 210;

		[SerializeField]
		AudioSource mainAudioSourcePrefab;

		/// <summary>
		/// TODO
		/// </summary>
		public AudioSource MainAudioSourcePrefab => mainAudioSourcePrefab;

		/// <summary>
		/// Sets up <paramref name="attach"/> with <see cref="AudioSource"/>s
		/// and other items to play the music.
		/// </summary>
		/// <param name="attach">
		/// The script generated <see cref="AudioSource"/>s will be attached
		/// or be child of.
		/// </param>
		/// 
		/// 
		public abstract Player GeneratePlayer(GameObject attach);

		/// <inheritdoc/>
		public virtual void Reset()
		{
#if UNITY_EDITOR
			const string DEFAULT_PREFAB_PATH = AudioSettings.DATA_DIRECTORY + "Default Background Audio Source.prefab";
			mainAudioSourcePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioSource>(DEFAULT_PREFAB_PATH);
#endif
		}

		/// <summary>
		/// TODO
		/// </summary>
		public abstract class Player : MonoBehaviour
		{
			/// <summary>
			/// TODO
			/// </summary>
			public event System.Action<Player> OnBeforeDestroy;
			/// <summary>
			/// TODO
			/// </summary>
			public abstract event ITrackable<PlayState>.ChangeEvent OnBeforeChangeState;
			/// <summary>
			/// TODO
			/// </summary>
			public abstract event ITrackable<PlayState>.ChangeEvent OnAfterChangeState;

			/// <summary>
			/// TODO
			/// </summary>
			public abstract BackgroundAudio Data
			{
				get;
			}
			/// <summary>
			/// TODO
			/// </summary>
			public abstract PlayState State
			{
				get;
			}
			/// <summary>
			/// TODO
			/// </summary>
			public abstract AudioMixerGroup MixerGroup
			{
				set;
			}

			/// <summary>
			/// TODO
			/// </summary>
			/// <param name="args"></param>
			public abstract void Play(PlaybackArgs args);

			/// <summary>
			/// TODO
			/// </summary>
			public abstract void Stop();

			/// <summary>
			/// TODO
			/// </summary>
			//public abstract void Pause();

			/// <summary>
			/// TODO
			/// </summary>
			//public virtual void Resume()
			//{
			//	if (State == PlayState.Paused)
			//	{
			//		Play(null);
			//	}
			//}

			/// <inheritdoc/>
			protected virtual void OnDestroy()
			{
				OnBeforeDestroy?.Invoke(this);
			}
		}
	}
}
