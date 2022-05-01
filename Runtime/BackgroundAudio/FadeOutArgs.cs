using System;
using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="FadeOutArgs.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 4/18/2022<br/>
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
	/// TODO
	/// <seealso cref="UnityEngine.AudioSettings.dspTime"/>
	/// </summary>
	[Serializable]
	public class FadeOutArgs : EventArgs
	{
		[SerializeField]
		double delaySeconds = 0;
		[SerializeField]
		double durationSeconds = 0;
		[SerializeField]
		bool pause = false;

		/// <summary>
		/// Delay fade-out effect in seconds, normalized by DSP scale.
		/// <seealso cref="UnityEngine.AudioSettings.dspTime"/>
		/// </summary>
		public double DelaySeconds
		{
			get => delaySeconds;
			set => delaySeconds = PlaybackArgs.ClampNegative(value);
		}
		/// <summary>
		/// Duration of fade-out when it begins, in seconds, normalized by DSP scale.
		/// <seealso cref="UnityEngine.AudioSettings.dspTime"/>
		/// </summary>
		public double DurationSeconds
		{
			get => durationSeconds;
			set => durationSeconds = PlaybackArgs.ClampNegative(value);
		}
		/// <summary>
		/// If true, pauses the audio after being faded out.
		/// Otherwise, stops it.
		/// </summary>
		public bool Pause
		{
			get => pause;
			set => pause = value;
		}

		/// <summary>
		/// Fixes any invalid property values.
		/// </summary>
		public void FixData()
		{
			DelaySeconds = delaySeconds;
			DurationSeconds = durationSeconds;
		}
	}
}
