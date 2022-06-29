using System.Collections;
using System.Collections.Generic;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AudioPlayerManager.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/17/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Retains a history of <seealso cref="BackgroundAudio"/>, capped by
	/// <see cref="MaxCapacity"/>.
	/// </summary>
	/// <seealso cref="AssetRef{TObject}"/>
	public class AudioHistory : IReadOnlyList<AssetRef<BackgroundAudio>>
	{
		#region EventArgs
		/// <summary>
		/// TODO
		/// </summary>
		public class AddArgs : System.EventArgs
		{
			/// <summary>
			/// TODO
			/// </summary>
			/// <param name="newAsset"></param>
			public AddArgs(AssetRef<BackgroundAudio> newAsset)
			{
				NewAsset = newAsset;
			}

			/// <summary>
			/// TODO
			/// </summary>
			public AssetRef<BackgroundAudio> NewAsset
			{
				get;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public class RemoveArgs : AddArgs
		{
			/// <summary>
			/// TODO
			/// </summary>
			/// <param name="newAsset"></param>
			/// <param name="isOldestRemoved"></param>
			public RemoveArgs(AssetRef<BackgroundAudio> newAsset, bool isOldestRemoved) : base(newAsset)
			{
				IsOldestRemoved = isOldestRemoved;
			}

			/// <summary>
			/// TODO
			/// </summary>
			public bool IsOldestRemoved
			{
				get;
			}
		}
		#endregion

		/// <summary>
		/// Default history size.
		/// </summary>
		public const int DEFAULT_HISTORY_SIZE = 100;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		public delegate void OnAdd(AudioHistory source, AddArgs args);
		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		public delegate void OnRemove(AudioHistory source, RemoveArgs args);

		/// <summary>
		/// TODO
		/// </summary>
		public event OnAdd OnBeforeAdd;
		/// <summary>
		/// TODO
		/// </summary>
		public event OnAdd OnAfterAdd;
		/// <summary>
		/// TODO
		/// </summary>
		public event OnRemove OnBeforeRemove;
		/// <summary>
		/// TODO
		/// </summary>
		public event OnRemove OnAfterRemove;
		/// <summary>
		/// TODO
		/// </summary>
		public event System.Action<AudioHistory> OnBeforeClear;
		/// <summary>
		/// TODO
		/// </summary>
		public event System.Action<AudioHistory> OnAfterClear;

		/// <summary>
		/// Stories history in chronological order.
		/// </summary>
		readonly LinkedList<AssetRef<BackgroundAudio>> history = new();
		int maxCapacity;

		/// <summary>
		/// Constructs a new history with a specified capacity.
		/// </summary>
		/// <param name="maxCapacity">
		/// The maximum capacity of the history.
		/// </param>
		public AudioHistory(int maxCapacity = DEFAULT_HISTORY_SIZE)
		{
			MaxCapacity = maxCapacity;
		}

		/// <summary>
		/// Gets or sets the maximum capacity of this history.
		/// </summary>
		public int MaxCapacity
		{
			get => maxCapacity;
			set
			{
				// Check if argument is valid
				if (value < 1)
				{
					throw new System.ArgumentException("Max size has to be 1 or larger", nameof(value));
				}

				// Set size
				maxCapacity = value;

				// Trim history
				while (history.Count > maxCapacity)
				{
					RemoveOldest();
				}
			}
		}
		/// <summary>
		/// How many entries are currently in the history.
		/// </summary>
		public int Count => history.Count;
		/// <summary>
		/// Gets a specific audio clip, <paramref name="index"/> place
		/// from oldest entry in the history.
		/// </summary>
		/// <param name="index">
		/// Placement from oldest entry, up.
		/// </param>
		/// <returns>
		/// The <see cref="BackgroundAudio"/>, <paramref name="index"/>
		/// place from oldest entry.
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">
		/// If <paramref name="index"/> is negative or greater or equal to <see cref="Count"/>
		/// </exception>
		/// <remarks>
		/// This property's performance is O(n).  It does attempt to iterate either
		/// in chronological or reverse order, whichever is faster.  This does make
		/// getting the newest or oldest entries a O(1) operation.
		/// </remarks>
		public AssetRef<BackgroundAudio> this[int index]
		{
			get
			{
				// Make sure index is within range
				if ((index < 0) || (index >= Count))
				{
					throw new System.ArgumentOutOfRangeException(nameof(index));
				}

				// Check if iterating in chronological order is faster
				LinkedListNode<AssetRef<BackgroundAudio>> node;
				if (index <= (Count - index))
				{
					// Search in chronological order
					node = history.First;
					for (int i = 0; i < index; ++i)
					{
						node = node.Next;
					}
				}
				else
				{
					// Search in reverse order
					node = history.Last;
					for (int i = 0; i < index; ++i)
					{
						node = node.Previous;
					}
				}
				return node.Value;
			}
		}
		/// <summary>
		/// Grabs the oldest entry in the history.
		/// </summary>
		public AssetRef<BackgroundAudio> Oldest
		{
			get
			{
				LinkedListNode<AssetRef<BackgroundAudio>> node = history.First;
				if(node != null)
				{
					return node.Value;
				}
				else
				{
					return AssetRef<BackgroundAudio>.NULL;
				}
			}
		}
		/// <summary>
		/// Grabs the newest entry in the history.
		/// </summary>
		public AssetRef<BackgroundAudio> Newest
		{
			get
			{
				LinkedListNode<AssetRef<BackgroundAudio>> node = history.Last;
				if(node != null)
				{
					return node.Value;
				}
				else
				{
					return AssetRef<BackgroundAudio>.NULL;
				}
			}
		}
		/// <summary>
		/// Gets an iterator in either chronological or reverse history order.
		/// </summary>
		/// <param name="newestToOldest">
		/// If <see langword="true"/>, enumerates the history in reverse order.
		/// Otherwise, iteraates in the chronological order.
		/// </param>
		/// <returns>
		/// Iterator through the whole history.
		/// </returns>
		public IEnumerator<AssetRef<BackgroundAudio>> GetEnumerator(bool newestToOldest)
		{
			if (newestToOldest)
			{
				// Go through the last 
				LinkedListNode<AssetRef<BackgroundAudio>> node = history.Last;
				while (node != null)
				{
					yield return node.Value;
					node = node.Previous;
				}
			}
			else
			{
				LinkedListNode<AssetRef<BackgroundAudio>> node = history.First;
				while (node != null)
				{
					yield return node.Value;
					node = node.Next;
				}
			}
		}

		/// <summary>
		/// Adds an asset to the history as the newest entry.
		/// </summary>
		/// <param name="asset">
		/// Newest asset to add to the history.
		/// </param>
		public void Add(in AssetRef<BackgroundAudio> asset)
		{
			// Call event
			AddArgs args = new(asset);
			OnBeforeAdd?.Invoke(this, args);

			// Prune the history until it's below the max size
			while (history.Count >= MaxCapacity)
			{
				RemoveOldest();
			}
			history.AddLast(asset);

			// Call event
			OnAfterAdd?.Invoke(this, args);
		}

		/// <summary>
		/// Removes the oldest entry in the history.
		/// </summary>
		/// <remarks>
		/// Does nothing if history is empty.
		/// </remarks>
		public void RemoveOldest()
		{
			// Make sure there's something in the history
			if (history.Count == 0)
			{
				return;
			}

			// Call event
			RemoveArgs args = new(history.First.Value, true);
			OnBeforeRemove?.Invoke(this, args);

			// Remove first element
			history.RemoveFirst();

			// Call event
			OnAfterRemove?.Invoke(this, args);
		}

		/// <summary>
		/// Removes the newest entry in the history.
		/// </summary>
		/// <remarks>
		/// Does nothing if history is empty.
		/// </remarks>
		public void RemoveNewest()
		{
			// Make sure there's something in the history
			if (history.Count == 0)
			{
				return;
			}

			// Call event
			RemoveArgs args = new(history.Last.Value, true);
			OnBeforeRemove?.Invoke(this, args);

			// Remove last element
			history.RemoveLast();

			// Call event
			OnAfterRemove?.Invoke(this, args);
		}

		/// <summary>
		/// Empties the history.
		/// </summary>
		public void Clear()
		{
			// Make sure there's something in the history
			if (history.Count == 0)
			{
				return;
			}

			// Call event
			OnBeforeClear?.Invoke(this);

			// Clear history
			history.Clear();

			// Call event
			OnAfterClear?.Invoke(this);
		}

		/// <summary>
		/// Indicates if a specific asset is already in the history.
		/// </summary>
		/// <param name="asset">
		/// The audio clip to search in the history.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if asset is in the history;
		/// <see langword="false"/>, otherwise.
		/// </returns>
		public bool Contains(in AssetRef<BackgroundAudio> asset) => history.Contains(asset);

		/// <inheritdoc/>
		public IEnumerator<AssetRef<BackgroundAudio>> GetEnumerator() => history.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)history).GetEnumerator();
	}
}
