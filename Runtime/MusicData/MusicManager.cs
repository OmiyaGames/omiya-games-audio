using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OmiyaGames.Audio.Collections
{
	internal class MusicManager
	{
		/// <summary>
		/// TODO
		/// </summary>
		public class Data<T>
		{
			internal readonly MusicManager manager;

			internal Data(MusicManager manager, T metaData)
			{
				this.manager = manager;
				MetaData = metaData;
			}

			/// <summary>
			/// TODO
			/// </summary>
			public MusicData Music => manager.Music;
			/// <summary>
			/// TODO
			/// </summary>
			public T MetaData
			{
				get;
			}
		}

		event System.Action<MusicManager> OnDispose;

		static readonly Dictionary<MusicData, MusicManager> dataCache = new Dictionary<MusicData, MusicManager>();

		internal static MusicManager Create(MusicData music, System.Action<MusicManager> onDispose = null)
		{
			if (dataCache.TryGetValue(music, out MusicManager returnManager) == false)
			{
				returnManager = new MusicManager(music);
				dataCache.Add(music, returnManager);

				if (onDispose != null)
				{
					returnManager.OnDispose += onDispose;
				}
			}
			return returnManager;
		}

		internal static MusicManager Create(AsyncOperationHandle<MusicData> music, System.Action<MusicManager> onDispose = null)
		{
			if (dataCache.TryGetValue(music.Result, out MusicManager returnManager) == false)
			{
				returnManager = new MusicManager(music);
				dataCache.Add(music.Result, returnManager);

				if (onDispose != null)
				{
					returnManager.OnDispose += onDispose;
				}
			}
			return returnManager;
		}

		readonly AsyncOperationHandle<MusicData>? musicLoader;
		uint counter = 0;

		MusicManager(MusicData music)
		{
			Music = music;
			musicLoader = null;
		}

		MusicManager(AsyncOperationHandle<MusicData> musicLoader)
		{
			Music = musicLoader.Result;
			this.musicLoader = musicLoader;
		}

		public MusicData Music
		{
			get;
		}

		internal void IncrementCounter() => ++counter;
		internal void DecrementCounter()
		{
			// Decrement the counter first
			if (counter > 0)
			{
				--counter;
			}

			// Check if the counter went to 0
			if (counter == 0)
			{
				// Remove this data from the cache
				dataCache.Remove(Music);

				// Unload the addressable
				if (musicLoader.HasValue)
				{
					Addressables.Release(musicLoader.Value);
				}

				// Clean-up anything else
				OnDispose?.Invoke(this);
			}
		}
	}
}
