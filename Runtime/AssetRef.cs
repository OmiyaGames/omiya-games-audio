using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// TODO: consider moving this to a different package so other addressable-relying
// packages can rely on this struct, too.
namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AssetRef.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/3/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// A common wrapper to an asset.  Use this to represent Assets that could
	/// <em>either</em> be a direct reference to a project asset, or an
	/// addressable reference.
	/// </summary>
	public struct AssetRef<TObject> : IEquatable<TObject>, IEquatable<AssetRef<TObject>>, IEquatable<AssetReferenceT<TObject>> where TObject : UnityEngine.Object
	{
		public static readonly AssetRef<TObject> NULL = new AssetRef<TObject>();

		bool isLoading;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="asset"></param>
		public AssetRef(TObject asset)
		{
			isLoading = false;
			Asset = asset;
			Reference = null;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="reference"></param>
		public AssetRef(AssetReferenceT<TObject> reference)
		{
			isLoading = false;
			Asset = null;
			Reference = reference;
		}

		/// <summary>
		/// Converts <see cref="TObject"/> to <see cref="AssetRef{TObject}"/>.
		/// </summary>
		public static implicit operator AssetRef<TObject>(TObject convert) => new AssetRef<TObject>(convert);

		/// <summary>
		/// Converts <see cref="AssetReferenceT{TObject}"/> to <see cref="AssetRef{TObject}"/>.
		/// </summary>
		public static implicit operator AssetRef<TObject>(AssetReferenceT<TObject> convert) => new AssetRef<TObject>(convert);

		/// <summary>
		/// TODO
		/// </summary>
		public TObject Asset
		{
			get;
			private set;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public AssetReferenceT<TObject> Reference
		{
			get;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public AssetRef.State CurrentState
		{
			get
			{
				if (Reference == null)
				{
					if (Asset != null)
					{
						return AssetRef.State.DirectAsset;
					}

					return AssetRef.State.Null;
				}

				if (string.IsNullOrEmpty(Reference.AssetGUID))
				{
					return AssetRef.State.Null;
				}

				if (isLoading)
				{
					return AssetRef.State.Loading;
				}

				if (Asset != null)
				{
					return AssetRef.State.Ready;
				}

				return AssetRef.State.Unloaded;
			}
		}

		/// <inheritdoc/>
		public string Name
		{
			get
			{
				if (Asset != null)
				{
					return Asset.name;
				}

				if (Reference != null)
				{
					return Reference.SubObjectName;
				}

				return null;
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			// Compare reference GUID first
			if (string.IsNullOrEmpty(Reference?.AssetGUID) == false)
			{
				return Reference.AssetGUID.GetHashCode();
			}
			else if (Asset != null)
			{
				return Asset.GetHashCode();
			}
			return 0;
		}

		/// <inheritdoc/>
		public bool Equals(TObject other) => (Asset == other);

		/// <inheritdoc/>
		public bool Equals(AssetReferenceT<TObject> other) => (Reference?.AssetGUID == other?.AssetGUID);

		/// <inheritdoc/>
		public bool Equals(AssetRef<TObject> other)
		{
			// Check reference first, followed by asset
			if ((Reference != null) && (other.Reference != null))
			{
				return Reference.AssetGUID == other.Reference.AssetGUID;
			}
			return Asset == other.Asset;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public IEnumerator LoadAssetAsync()
		{
			if (CurrentState == AssetRef.State.Unloaded)
			{
				// Flag as loading
				isLoading = true;

				// Start the loading process
				var loadAssetHandle = Reference.LoadAssetAsync();
				loadAssetHandle.Completed += LoadAssetHandle_Completed;
				yield return loadAssetHandle;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void ReleaseAsset()
		{
			// Check if the handle is done
			if ((Reference != null) && Reference.OperationHandle.IsDone)
			{
				// Release the handle
				Addressables.Release(Reference.OperationHandle);

				// Reset member variables
				Asset = null;
			}
		}

		#region Event handler
		void LoadAssetHandle_Completed(AsyncOperationHandle<TObject> obj)
		{
			// Make sure loading succeeded
			if (obj.Status == AsyncOperationStatus.Succeeded)
			{
				// Setup Asset
				Asset = obj.Result;

				// Indicate loading is done
				isLoading = false;
			}
			else
			{
				// On fail, release the handle immediately
				ReleaseAsset();

				// Indicate loading is done
				isLoading = false;

				// Throw an exception
				throw obj.OperationException;
			}
		}
		#endregion
	}

	/// <summary>
	/// TODO
	/// </summary>
	public static class AssetRef
	{
		/// <summary>
		/// TODO
		/// </summary>
		public enum State
		{
			/// <summary>
			/// 
			/// </summary>
			Null,
			/// <summary>
			/// 
			/// </summary>
			DirectAsset,
			/// <summary>
			/// 
			/// </summary>
			Unloaded,
			/// <summary>
			/// 
			/// </summary>
			Loading,
			/// <summary>
			/// 
			/// </summary>
			Ready
		}
	}
}
