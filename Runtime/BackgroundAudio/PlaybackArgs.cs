using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="PlaybackArgs.cs" company="Omiya Games">
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
	/// <strong>Version:</strong> 1.0.0<br/>
	/// <strong>Date:</strong> 4/18/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Provides configuration details to <seealso cref="BackgroundAudio.Player"/>
	/// on how to play a <see cref="BackgroundAudio"/>.
	/// </summary>
	public class PlaybackArgs : EventArgs
	{
		[SerializeField]
		double delaySeconds = 0;
		[SerializeField]
		double skipForwardToSeconds = 0;

		/// <summary>
		/// If <paramref name="time"/> is negative,
		/// returns 0.  Otherwise, returns <paramref name="time"/>.
		/// </summary>
		internal static double ClampNegative(double time) => time < 0 ? 0 : time;

		/// <summary>
		/// Delay time in seconds, normalized by DSP scale.
		/// <seealso cref="UnityEngine.AudioSettings.dspTime"/>
		/// </summary>
		public double DelaySeconds
		{
			get => delaySeconds;
			set => delaySeconds = ClampNegative(value);
		}

		/// <summary>
		/// Allows playing from the middle of an audio clip by skipping
		/// forward in time by X seconds, normalized by DSP scale.
		/// <seealso cref="UnityEngine.AudioSettings.dspTime"/>
		/// </summary>
		public double SkipForwardToSeconds
		{
			get => skipForwardToSeconds;
			set => skipForwardToSeconds = ClampNegative(value);
		}

		/// <summary>
		/// Fixes any invalid property values.
		/// </summary>
		public virtual void FixData()
		{
			DelaySeconds = delaySeconds;
			SkipForwardToSeconds = skipForwardToSeconds;
		}
	}
}