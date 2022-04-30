using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OmiyaGames.Audio
{
	using Collections;

	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="MusicDataStack.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 4/17/2022<br/>
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
	/// </summary>
	public class MusicDataStack : IDisposable, IReadOnlyCollection<BackgroundAudio>
	{
		/// <summary>
		/// TODO
		/// </summary>
		[Serializable]
		public class Settings
		{
			[SerializeField]
			[Tooltip("A prefab to setup an Audio Source for playing clips in the background.")]
			AudioSource defaultAudioSetup;
			[SerializeField]
			MusicFader.Layer[] fadeLayers;

			/// <summary>
			/// TODO
			/// </summary>
			public AudioSource DefaultAudioSetup => defaultAudioSetup;

			/// <summary>
			/// TODO
			/// </summary>
			public MusicFader.Layer[] FadeLayers => fadeLayers;
		}

		class StackData
		{
			public StackData(BackgroundAudio music, GameObject gameObject)
			{
				Music = music;
				GameObject = gameObject;
				MusicLoader = null;
				AssetGuid = null;
			}

			public StackData(in AsyncOperationHandle<BackgroundAudio> musicLoader, string assetGuid, GameObject gameObject)
			{
				Music = musicLoader.Result;
				GameObject = gameObject;
				MusicLoader = musicLoader;
				AssetGuid = assetGuid;
			}

			public BackgroundAudio Music
			{
				get;
			}
			public GameObject GameObject
			{
				get;
			}
			public AsyncOperationHandle<BackgroundAudio>? MusicLoader
			{
				get;
			}
			public string AssetGuid
			{
				get;
			}
		}

		// TODO: consider splitting this up so fader and history are more separated
		readonly MusicDataCollection<StackData> history = new MusicDataCollection<StackData>();
		readonly MusicFader fadeQueue;
		readonly MonoBehaviour manager;
		readonly Transform parentTransform;

		/// <summary>
		/// TODO
		/// </summary>
		internal MusicDataStack(MonoBehaviour manager, Settings settings, AnimationCurve percentToDbCurve, string name = "stack")
		{
			// Do some null-checking
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			// Construct member variables
			this.manager = manager;
			fadeQueue = new MusicFader(manager, settings.DefaultAudioSetup, percentToDbCurve, settings.FadeLayers);

			// Setup parentTransform
			var stackObject = new GameObject(name);
			stackObject.transform.SetParent(manager.transform);
			parentTransform = stackObject.transform;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public int Count => history.Count;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public void Push(BackgroundAudio data, FadeInArgs args = null)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			// Push the layer info in the stack
			GameObject newObject = new GameObject(data.name);
			newObject.transform.SetParent(parentTransform);
			StackData fadeIn = new StackData(data, newObject);
			history.AddLast(data, fadeIn);

			// Perform an audio fade-out
			fadeQueue.FadeIn(data, newObject, args);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public IEnumerator Push(AssetReferenceT<BackgroundAudio> data, FadeInArgs args = null)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			// Attempt to load the data
			AsyncOperationHandle<BackgroundAudio> loadDataHandle = data.LoadAssetAsync();
			if (loadDataHandle.Status == AsyncOperationStatus.None)
			{
				yield return loadDataHandle;

				// Make sure it loaded correctly
				switch (loadDataHandle.Status)
				{
					case AsyncOperationStatus.None:
						throw new Exception($"Unable to load AssetReference \"{data.SubObjectName}\"");
					case AsyncOperationStatus.Failed:
						throw loadDataHandle.OperationException;
				}
			}

			// Push the layer info in the stack
			GameObject newObject = new GameObject(loadDataHandle.Result.name);
			newObject.transform.SetParent(parentTransform);
			StackData fadeIn = new StackData(loadDataHandle, data.AssetGUID, newObject);
			history.AddLast(in loadDataHandle, fadeIn);

			// Perform an audio fade-out
			fadeQueue.FadeIn(loadDataHandle, newObject, args);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public void Pop(FadeInArgs args = null)
		{
			// Check if there's anything in the stack
			if (Count == 0)
			{
				return;
			}

			// Pop the last music
			history.RemoveLast();

			// Perform an audio fade-out
			MusicManager.Data<StackData> fadeIn = history.Last;
			if (fadeIn != null)
			{
				fadeQueue.FadeIn(fadeIn.Music, fadeIn.MetaData.GameObject, args);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public BackgroundAudio Peek() => history.Last?.Music;

		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public string PeekAssetGuid() => history.Last?.MetaData?.AssetGuid;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		public void Reset(BackgroundAudio data, FadeInArgs args = null)
		{
			// Basically push the data
			Push(data, args);

			// Once that's done, clear all but one data
			while (history.Count > 1)
			{
				history.RemoveFirst();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void Clear(FadeOutArgs args = null)
		{
			// Make sure there's actually something to clear
			if (Count == 0)
			{
				return;
			}

			// Perform an audio fade-out
			fadeQueue.FadeOut(args);

			// Clear the stack
			while (history.Count > 0)
			{
				history.RemoveFirst();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void Dispose()
		{
			// Clear the stack
			Clear();

			// Destroy the transform
			UnityEngine.Object.Destroy(parentTransform.gameObject);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public IEnumerator<BackgroundAudio> GetEnumerator()
		{
			foreach (var stackData in history)
			{
				yield return stackData.Music;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)history).GetEnumerator();
		}
	}
}
