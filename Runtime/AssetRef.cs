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
	/// <strong>Version:</strong> 1.0.0<br/>
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
		/// Creates a wrapper for a project asset.
		/// Should <em>not</em> be used for addressable assets.
		/// </summary>
		/// <seealso cref="AssetRef(AssetReferenceT{TObject})"/>
		/// <param name="asset">
		/// The project asset.
		/// </param>
		public AssetRef(TObject asset)
		{
			isLoading = false;
			Asset = asset;
			Reference = null;
		}

		/// <summary>
		/// Creates a wrapper for an addressable asset.
		/// </summary>
		/// <param name="reference">
		/// The addressable asset.
		/// </param>
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
		/// <para>
		/// If <seealso cref="Reference"/> is <see langword="null"/>,
		/// gets the referenced project asset.
		/// </para><para>
		/// If not, and <seealso cref="CurrentState"/> is <see cref="AssetRef.State.Ready"/>,
		/// gets the loaded addressable asset from <seealso cref="Reference"/>.
		/// </para><para>
		/// Otherwise, returns <see langword="null"/>.
		/// </para>
		/// </summary>
		public TObject Asset
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the addressable asset's reference, if constructed
		/// by <seealso cref="AssetRef.ctor(AssetReferenceT{TObject})"/>
		/// </summary>
		public AssetReferenceT<TObject> Reference
		{
			get;
		}

		/// <summary>
		/// Indicates whether this object is null, project asset (<see cref="AssetRef.State.ProjectAsset"/>),
		/// or whether <seealso cref="Reference"/> has loaded or unloaded an asset.
		/// </summary>
		public AssetRef.State CurrentState
		{
			get
			{
				if (Reference == null)
				{
					if (Asset != null)
					{
						return AssetRef.State.ProjectAsset;
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

		/// <summary>
		/// Gets the name of the asset, or <see langword="null"/>
		/// if there isn't any.
		/// </summary>
		public string Name
		{
			get
			{
				if (Reference != null)
				{
					return Reference.SubObjectName;
				}
				else if (Asset != null)
				{
					return Asset.name;
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
		/// Starts loading the addressable from <seealso cref="Reference"/>.
		/// </summary>
		/// <returns>
		/// A coroutine loading the addressable asset.
		/// </returns>
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
		/// Unloads the addressable from <seealso cref="Reference"/>.
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
	/// Contains helper enums for <seealso cref="AssetRef{TObject}"/>.
	/// </summary>
	public static class AssetRef
	{
		/// <summary>
		/// Provides the state of <seealso cref="AssetRef{TObject}"/>,
		/// and whether it's referencing a project asset or addressable asset.
		/// </summary>
		public enum State
		{
			/// <summary>
			/// Indicates <seealso cref="AssetRef{TObject}"/> is <see langword="null"/>,
			/// and doesn't reference anything.
			/// </summary>
			Null,
			/// <summary>
			/// Indicates <seealso cref="AssetRef{TObject}"/> references an asset
			/// in the Unity project.
			/// </summary>
			ProjectAsset,
			/// <summary>
			/// Indicates <seealso cref="AssetRef{TObject}"/> references an addresable
			/// asset, and is currently unloaded.
			/// </summary>
			Unloaded,
			/// <summary>
			/// Indicates <seealso cref="AssetRef{TObject}"/> references an addresable
			/// asset, and is in the middle of loading the asset.
			/// </summary>
			Loading,
			/// <summary>
			/// Indicates <seealso cref="AssetRef{TObject}"/> references an addresable
			/// asset, and has the asset loaded and ready to use.
			/// </summary>
			Ready
		}
	}
}
