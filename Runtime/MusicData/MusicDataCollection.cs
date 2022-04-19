using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OmiyaGames.Audio.Collections
{
	internal class MusicDataCollection<T> : IReadOnlyCollection<MusicManager.Data<T>>
	{

		readonly LinkedList<MusicManager.Data<T>> collection = new LinkedList<MusicManager.Data<T>>();

		/// <summary>
		/// TODO
		/// </summary>
		public MusicManager.Data<T> First => collection.First?.Value;
		/// <summary>
		/// TODO
		/// </summary>
		public MusicManager.Data<T> Last => collection.Last?.Value;
		/// <summary>
		/// TODO
		/// </summary>
		public int Count => collection.Count;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="metadata"></param>
		public void AddFirst(MusicData music, T metadata) => AddFirst(MusicManager.Create(music), metadata);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="metadata"></param>
		public void AddFirst(in AsyncOperationHandle<MusicData> music, T metadata) => AddFirst(MusicManager.Create(music), metadata);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="metadata"></param>
		public void AddLast(MusicData music, T metadata) => AddLast(MusicManager.Create(music), metadata);

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="music"></param>
		/// <param name="metadata"></param>
		public void AddLast(in AsyncOperationHandle<MusicData> music, T metadata) => AddLast(MusicManager.Create(music), metadata);

		/// <summary>
		/// TODO
		/// </summary>
		public void RemoveFirst()
		{
			// Check if there's a node to remove
			MusicManager.Data<T> firstData = First;
			if (firstData != null)
			{
				// If so, remove from the collection
				collection.RemoveFirst();

				// Decrement the resource counter
				firstData.manager.DecrementCounter();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void RemoveLast()
		{
			// Check if there's a node to remove
			MusicManager.Data<T> lastData = Last;
			if (lastData != null)
			{
				// If so, remove from the collection
				collection.RemoveLast();

				// Decrement the resource counter
				lastData.manager.DecrementCounter();
			}
		}

		/// <inheritdoc/>
		public IEnumerator<MusicManager.Data<T>> GetEnumerator() => collection.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)collection).GetEnumerator();

		void AddFirst(MusicManager data, T metadata)
		{
			// Add a new node to the list
			collection.AddFirst(new MusicManager.Data<T>(data, metadata));

			// Increment the resource counter
			data.IncrementCounter();
		}

		void AddLast(MusicManager data, T metadata)
		{
			// Add a new node to the list
			collection.AddLast(new MusicManager.Data<T>(data, metadata));

			// Increment the resource counter
			data.IncrementCounter();
		}
	}
}
