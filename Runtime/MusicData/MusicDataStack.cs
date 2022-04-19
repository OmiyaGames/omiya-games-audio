using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OmiyaGames.Audio
{
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
	public class MusicDataStack : IDisposable, IReadOnlyCollection<MusicData>
	{
		/// <summary>
		/// TODO
		/// </summary>
		[Serializable]
		public struct FadeLayer
		{
			[SerializeField]
			[Tooltip("The group to pipe the Audio Source for this layer.")]
			AudioMixerGroup group;
			[SerializeField]
			[Tooltip("The parameter name that adjusts the group's volume.")]
			string paramName;

			/// <summary>
			/// TODO
			/// </summary>
			public AudioMixerGroup Group => group;
			/// <summary>
			/// TODO
			/// </summary>
			public string ParamName => paramName;
		}

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
			FadeLayer[] fadeLayers;

			public AudioSource DefaultAudioSetup => defaultAudioSetup;

			public FadeLayer[] FadeLayers => fadeLayers;
		}

		class StackData
		{
			public StackData(MusicData music, GameObject gameObject, in FadeLayer layer)
			{
				Music = music;
				GameObject = gameObject;
				Layer = layer;
				MusicLoader = null;
			}

			public StackData(AsyncOperationHandle<MusicData> musicLoader, GameObject gameObject, in FadeLayer layer)
			{
				Music = musicLoader.Result;
				GameObject = gameObject;
				Layer = layer;
				MusicLoader = musicLoader;
			}

			public MusicData Music
			{
				get;
			}
			public GameObject GameObject
			{
				get;
			}
			[Obsolete("Will be removing this property entirely")]
			public FadeLayer Layer
			{
				get;
			}
			public AsyncOperationHandle<MusicData>? MusicLoader
			{
				get;
			}
			public bool IsInStack
			{
				get;
				set;
			} = true;
		}

		int nextLayer = 0;

		/// <summary>
		/// TODO
		/// </summary>
		internal MusicDataStack(MonoBehaviour manager, Settings settings, AnimationCurve percentToDbCurve, string name = "stack")
		{
			// Do some null-checking
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			if (percentToDbCurve == null)
			{
				throw new ArgumentNullException(nameof(percentToDbCurve));
			}

			// FIXME: do some more thorough test on settings

			// Setup member variables
			PercentToDbCurve = percentToDbCurve;
			SetupInfo = settings;
			Manager = manager;

			// Setup parentTransform
			var stackObject = new GameObject(name);
			stackObject.transform.SetParent(Manager.transform);
			ParentTransform = stackObject.transform;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public int Count => Stack.Count;
		MonoBehaviour Manager
		{
			get;
		}
		Transform ParentTransform
		{
			get;
		}
		[Obsolete("Will be split up into smaller parts")]
		Settings SetupInfo
		{
			get;
		}
		AnimationCurve PercentToDbCurve
		{
			get;
		}
		LinkedList<StackData> Stack
		{
			get;
		} = new LinkedList<StackData>();
		// FIXME: setup a helper class handling fade-outs

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fadeInSeconds"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public IEnumerator Push(MusicData data, float fadeInSeconds = 0)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			// Setup the data
			GenerateStackData(data, out FadeLayer layer, out GameObject newObject);
			data.Setup(newObject, layer.Group, SetupInfo.DefaultAudioSetup);

			// Push the layer info in the stack
			StackData fadeOut = Stack.Last?.Value;
			StackData fadeIn = new StackData(data, newObject, in layer);
			Stack.AddLast(fadeIn);

			// Perform an audio fade-out
			yield return Manager.StartCoroutine(FadeMusic(fadeIn, fadeOut, fadeInSeconds));
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fadeInSeconds"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public IEnumerator Push(AssetReferenceT<MusicData> data, float fadeInSeconds = 0)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			// Attempt to load the data
			AsyncOperationHandle<MusicData> loadDataHandle = data.LoadAssetAsync();
			yield return loadDataHandle;

			// Make sure it loaded correctly
			switch (loadDataHandle.Status)
			{
				case AsyncOperationStatus.None:
					throw new Exception($"Unable to load AssetReference \"{data.SubObjectName}\"");
				case AsyncOperationStatus.Failed:
					throw loadDataHandle.OperationException;
			}

			// Setup the data
			MusicData results = loadDataHandle.Result;
			GenerateStackData(results, out FadeLayer layer, out GameObject newObject);
			results.Setup(newObject, layer.Group, SetupInfo.DefaultAudioSetup);

			// Push the layer info in the stack
			StackData fadeOut = Stack.Last?.Value;
			StackData fadeIn = new StackData(loadDataHandle, newObject, in layer);
			Stack.AddLast(fadeIn);

			// Perform an audio fade-out
			yield return Manager.StartCoroutine(FadeMusic(fadeIn, fadeOut, fadeInSeconds));
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="fadeOutSeconds"></param>
		/// <returns></returns>
		public IEnumerator Pop(float fadeOutSeconds = 0)
		{
			// Check if there's anything in the stack
			if (Count == 0)
			{
				yield break;
			}

			// Pop the last music
			StackData fadeOut = Stack.Last.Value;
			fadeOut.IsInStack = false;
			Stack.RemoveLast();

			// Perform an audio fade-out
			StackData fadeIn = Stack.Last?.Value;
			yield return Manager.StartCoroutine(FadeMusic(fadeIn, fadeOut, fadeOutSeconds));

			// Clean up the popped data
			CleanUpData(in fadeOut);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public MusicData Peek() => Stack.Last?.Value.Music;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		public IEnumerator Reset(AssetReferenceT<MusicData> data, float fadeInSeconds = 0)
		{
			// Basically push the data
			yield return Manager.StartCoroutine(Push(data, fadeInSeconds));

			// Once that's done, clear all but one data
			while (Stack.Count > 1)
			{
				Stack.First.Value.IsInStack = false;
				CleanUpData(Stack.First.Value);
				Stack.RemoveFirst();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="data"></param>
		public IEnumerator Reset(MusicData data, float fadeInSeconds = 0)
		{
			// Basically push the data
			yield return Manager.StartCoroutine(Push(data, fadeInSeconds));

			// Once that's done, clear all but one data
			while (Stack.Count > 1)
			{
				Stack.First.Value.IsInStack = false;
				CleanUpData(Stack.First.Value);
				Stack.RemoveFirst();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public IEnumerator Clear(float fadeOutSeconds = 0)
		{
			// Make sure there's actually something to clear
			if (Count > 0)
			{
				yield break;
			}

			// Perform an audio fade-out
			yield return Manager.StartCoroutine(FadeMusic(null, Stack.Last.Value, fadeOutSeconds));

			// Actually clean-up and release all stack items
			foreach (var data in Stack)
			{
				data.IsInStack = false;
				CleanUpData(in data);
			}

			// Clear the stack
			Stack.Clear();
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void Dispose()
		{
			// Clear the stack
			Manager.StartCoroutine(Clear());

			// Destroy the transform
			UnityEngine.Object.Destroy(ParentTransform.gameObject);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public IEnumerator<MusicData> GetEnumerator()
		{
			foreach (var stackData in Stack)
			{
				yield return stackData.Music;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)Stack).GetEnumerator();
		}

		#region Helper Methods
		void GenerateStackData(MusicData data, out FadeLayer layer, out GameObject newObject)
		{
			// Determine which layer to assign this data
			layer = SetupInfo.FadeLayers[nextLayer++];
			if (nextLayer > SetupInfo.FadeLayers.Length)
			{
				nextLayer = 0;
			}

			// Setup the data
			newObject = new GameObject(data.name);
			newObject.transform.SetParent(ParentTransform);
		}

		IEnumerator FadeMusic(StackData fadeIn, StackData fadeOut, float durationSeconds)
		{
			// Make sure both fade-outs aren't null
			if ((fadeOut == null) && (fadeIn == null))
			{
				throw new ArgumentException($"{nameof(fadeOut)} and {nameof(fadeIn)} can't be null at the same time.");
			}

			// Setup the fade
			if (fadeIn != null)
			{
				// Mute the fade-in
				SetVolume(fadeIn, 0);

				// Play the fade-in music
				fadeIn.Music.Play(fadeIn.GameObject);
			}

			if (durationSeconds > 0)
			{
				float fadeInVolumePercent = AudioManager.MuteVolumeDb, fadeOutVolumeDb = 0;
				if (fadeOut != null)
				{
					// Grab all the necessary parameters
					AudioMixer mixer = fadeOut.Layer.Group.audioMixer;
					string paramName = fadeOut.Layer.ParamName;

					// Compute the volume
					mixer.GetFloat(paramName, out fadeOutVolumeDb);
				}

				// FIXME: actually change the volume gradually over time
				// Also, notice either fade outs can be null
				yield return null;
				throw new NotImplementedException();
			}

			// Close out the fade
			if (fadeIn != null)
			{
				// Blast the fade-in to full volume
				SetVolume(fadeIn, 1);
			}

			if (fadeOut != null)
			{
				// Mute the fade-out
				SetVolume(fadeIn, 0);

				// Stop the fade-out music
				fadeOut.Music.Stop(null);
			}

			void SetVolume(in StackData fadeIn, float volumePercent)
			{
				// Grab all the necessary parameters
				AudioMixer mixer = fadeIn.Layer.Group.audioMixer;
				string paramName = fadeIn.Layer.ParamName;

				// Compute the volume
				float volumeDb = PercentToDbCurve.Evaluate(volumePercent);

				// Update the mixer
				mixer.SetFloat(paramName, volumeDb);
			}
		}

		static void CleanUpData(in StackData data)
		{
			// Clean-up stack item
			data.Music.CleanUp(data.GameObject);
			UnityEngine.Object.Destroy(data.GameObject);

			// Release loader of music
			if (data.MusicLoader.HasValue)
			{
				Addressables.Release(data.MusicLoader.Value);
			}
		}
		#endregion
	}
}
