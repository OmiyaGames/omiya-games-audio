using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="ChangeMusicOnStart.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 4/14/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Changes the background music on scene start.
	/// </summary>
	public class ChangeMusicOnStart : MonoBehaviour
	{
		[SerializeField]
		bool clearAllMusicBeforePlaying = true;
		[SerializeField]
		float fadeInSeconds = 0.5f;
		[SerializeField]
		AssetReferenceT<MusicData> playMusic;

		/// <summary>
		/// Sets up the <seealso cref="MusicData"/> and <seealso cref="AudioManager"/>.
		/// </summary>
		/// <returns>The coroutine for loading everything.</returns>
		public virtual IEnumerator Start()
		{
			// Setup the manager
			yield return AudioManager.Setup();

			// Verify if everthing loaded correctly
			if (AudioManager.Status == Global.Settings.Data.Status.Fail)
			{
				Debug.LogError("Unable to AudioManager.", this);
				yield break;
			}

			// Setup this music
			if(clearAllMusicBeforePlaying)
			{
				yield return StartCoroutine(AudioManager.BackgroundMusicStack.Reset(playMusic, fadeInSeconds));
			}
			else
			{
				yield return StartCoroutine(AudioManager.BackgroundMusicStack.Push(playMusic, fadeInSeconds));
			}
		}
	}
}
