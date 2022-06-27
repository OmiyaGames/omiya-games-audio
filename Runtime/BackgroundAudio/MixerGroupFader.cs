using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="MixerGroupFader.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/26/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Unity inspector class for pairing a
	/// <seealso cref="AudioMixerGroup"/> with a parameter name
	/// for changing its volume.
	/// </summary>
	[Serializable]
	public class MixerGroupFader
	{
		[SerializeField]
		[Tooltip("The group to pipe the Audio Source for this layer.")]
		AudioMixerGroup group;
		[SerializeField]
		[Tooltip("The parameter name that adjusts the group's volume.")]
		string paramName;

		/// <summary>
		/// Gets this layer's <see cref="AudioMixerGroup"/>.
		/// </summary>
		public AudioMixerGroup Group => group;
		/// <summary>
		/// Gets the <seealso cref="Group"/>'s parameter name
		/// to adjust its volume.
		/// </summary>
		public string ParamName => paramName;
		public BackgroundAudio.Player Player
		{
			get;
			set;
		} = null;
		public Action<BackgroundAudio.Player> BeforePlayerDestroy
		{
			get;
			set;
		} = null;
		public double StartTime
		{
			get;
			set;
		} = 0;
		public double FadeDuration
		{
			get;
			set;
		} = 0;
		public float VolumePercent
		{
			get;
			set;
		} = 0;
		public Coroutine FadeRoutine
		{
			get;
			set;
		} = null;
	}
}
