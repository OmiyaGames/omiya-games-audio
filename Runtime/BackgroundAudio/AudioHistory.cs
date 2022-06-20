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
	/// TODO
	/// </summary>
	public class AudioHistory : IReadOnlyList<AssetRef<BackgroundAudio>>
	{
		/// <summary>
		/// Default history size.
		/// </summary>
		public const int DEFAULT_HISTORY_SIZE = 100;

		/// <summary>
		/// Stories history in chronological order.
		/// </summary>
		readonly LinkedList<AssetRef<BackgroundAudio>> history = new();
		int maxCapacity;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="maxCapacity"></param>
		public AudioHistory(int maxCapacity = DEFAULT_HISTORY_SIZE)
		{
			MaxCapacity = maxCapacity;
		}

		/// <summary>
		/// TODO
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
		/// TODO
		/// </summary>
		public int Count => history.Count;
		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		/// <exception cref="System.IndexOutOfRangeException"></exception>
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
		/// TODO
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
		/// TODO
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
		/// Returns an iterator enumerating in either chronological or reverse order.
		/// </summary>
		/// <param name="newestToOldest"></param>
		/// <returns></returns>
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
		/// Adds an asset to the history.
		/// </summary>
		/// <param name="asset">Asset to add.</param>
		public void Add(in AssetRef<BackgroundAudio> asset)
		{
			// Prune the history until it's below the max size
			while (history.Count >= MaxCapacity)
			{
				RemoveOldest();
			}
			history.AddLast(asset);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void RemoveOldest() => history.RemoveFirst();

		/// <summary>
		/// TODO
		/// </summary>
		public void RemoveNewest() => history.RemoveLast();

		/// <summary>
		/// Empties the history.
		/// </summary>
		public void Clear() => history.Clear();

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		public bool Contains(in AssetRef<BackgroundAudio> asset) => history.Contains(asset);

		/// <inheritdoc/>
		public IEnumerator<AssetRef<BackgroundAudio>> GetEnumerator() => history.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)history).GetEnumerator();
	}
}
