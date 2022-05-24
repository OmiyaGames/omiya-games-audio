using UnityEngine;
using OmiyaGames.Audio.Collections;

namespace OmiyaGames.Audio
{
	public partial class AudioLayer
	{
		///-----------------------------------------------------------------------
		/// <remarks>
		/// <copyright file="Background.cs" company="Omiya Games">
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
		/// Audio category applicable to ones predominately used in the background,
		/// i.e. music and ambience.
		/// </summary>
		[System.Serializable]
		public class Background : SubLayer
		{
			[SerializeField]
			MixerGroupManager.Layer[] fadeLayers;

			/// <summary>
			/// TODO
			/// </summary>
			public MixerGroupManager.Layer[] FadeLayers
			{
				get => fadeLayers;
				internal set => fadeLayers = value;
			}

			/// <summary>
			/// TODO
			/// </summary>
			public MusicDataStack Player
			{
				get;
				internal set;
			} = null;

			/// <summary>
			/// TODO
			/// </summary>
			public AudioPlayerManager PlayerManager
			{
				get;
				internal set;
			} = null;

			/// <summary>
			/// TODO
			/// </summary>
			public MixerGroupManager GroupManager
			{
				get;
				internal set;
			}
		}
	}
}
