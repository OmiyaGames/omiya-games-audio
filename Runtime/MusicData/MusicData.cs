using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="MusicData.cs" company="Omiya Games">
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
	public abstract class MusicData : ScriptableObject
	{
		/// <summary>
		/// TODO
		/// </summary>
		public enum PlayState
		{
			/// <summary>
			/// Unrecognized music
			/// </summary>
			Invalid = -1,
			/// <summary>
			/// Music has been stopped.
			/// </summary>
			Stopped = 0,
			/// <summary>
			/// Music is playing.
			/// </summary>
			Playing,
			/// <summary>
			/// Music has been paused.
			/// </summary>
			Paused
		}

		public const int MENU_ORDER = 210;

		// FIXME: might be better for Setup to create an instance of a MonoBehavior
		/// <summary>
		/// Sets up <paramref name="attach"/> with <see cref="AudioSource"/>s
		/// and other items to play the music.
		/// </summary>
		/// <param name="attach">
		/// The script generated <see cref="AudioSource"/>s will be attached
		/// or be child of.
		/// </param>
		/// <param name="group"></param>
		/// <param name="audioPrefab"></param>
		public abstract void Setup(GameObject attach, AudioMixerGroup group, AudioSource audioPrefab = null);

		/// <summary>
		/// Cleans-up a setup of data.
		/// </summary>
		/// <param name="attach"></param>
		/// <returns></returns>
		public abstract void CleanUp(GameObject attach);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="attach"></param>
		/// <param name="args"></param>
		public abstract void Play(GameObject attach, PlaybackArgs args);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="attach"></param>
		/// 
		public abstract void Stop(GameObject attach);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="attach"></param>
		public abstract void Pause(GameObject attach);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="attach"></param>
		/// <returns>TODO</returns>
		public abstract PlayState IsPlaying(GameObject attach);
	}
}
