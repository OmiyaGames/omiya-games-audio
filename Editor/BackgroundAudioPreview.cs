using UnityEngine;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="BackgroundAudioPreview.cs" company="Omiya Games">
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2014-2022 Omiya Games
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
	/// <strong>Date:</strong> 6/19/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial version.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Helper class to create an audio preview of a player.
	/// </summary>
	/// <seealso cref="SingleLoopingMusic"/>
	public class BackgroundAudioPreview : System.IDisposable
	{
		static GameObject playerObject = null;

		/// <summary>
		/// TODO
		/// </summary>
		public bool IsPlaying => (PlayStartTime >= 0);

		/// <summary>
		/// TODO
		/// </summary>
		/// <seealso cref="UnityEngine.AudioSettings.dspTime"/>
		public double PlayStartTime
		{
			get;
			private set;
		} = -1;

		/// <summary>
		/// TODO
		/// </summary>
		public void Dispose()
		{
			if (playerObject != null)
			{
				PlayStartTime = -1;

				Helpers.Destroy(playerObject);
				playerObject = null;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void Play(BackgroundAudio audioFile)
		{
			if (IsPlaying == false)
			{
				// Dispose any prior-playing music
				Dispose();

				// Set the start time
				PlayStartTime = UnityEngine.AudioSettings.dspTime;

				// Generate the preview object
				playerObject = new GameObject("Preview Audio");
				playerObject.hideFlags = HideFlags.HideAndDontSave;
				playerObject.SetActive(true);

				// Play the audio file
				audioFile.GeneratePlayer(playerObject).Play(new()
				{
					DelaySeconds = 0,
					SkipForwardToSeconds = 0,
				});
			}
		}
	}
}
