using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="SingleLoopingMusic.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 4/16/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Sets up the background audio to a single looping music
	/// </summary>
	[CreateAssetMenu(menuName = "Omiya Games/Audio/Looping Music", fileName = "Looping Music", order = (MENU_ORDER))]
	public class SingleLoopingMusic : BackgroundAudio
	{
		// FIXME: remove the public from all of these member variables
		[SerializeField]
		private AudioClip loop;

		[Header("Intro Stings")]
		[SerializeField]
		private AudioClip introSting;
		[SerializeField]
		private double playLoopAfterSeconds;

		/// <summary>
		/// TODO
		/// </summary>
		public double PlayLoopAfterSeconds => playLoopAfterSeconds;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioClip IntroSting => introSting;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioClip Loop => loop;

		/// <inheritdoc/>
		public override Player GeneratePlayer(GameObject attach)
		{
			var returnScript = attach.AddComponent<SingleLoopingMusicPlayer>();
			returnScript.Setup(this);
			return returnScript;
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();

			SetLoopDelayToIntroStingDuration();
		}

		/// <summary>
		/// Calculates how long <see cref="IntroSting"/> is,
		/// and sets <see cref="PlayLoopAfterSeconds"/>.
		/// This produces a more accurate value than
		/// <see cref="AudioClip.length"/>.
		/// </summary>
		[ContextMenu("Calculate Intro Sting Duration")]
		public void SetLoopDelayToIntroStingDuration()
		{
			// Make sure we have a sting to play
			if (IntroSting != null)
			{
				// Calculate the duration based on samples and frequency
				playLoopAfterSeconds = AudioManager.CalculateClipLengthSeconds(IntroSting);
			}
		}
	}
}
