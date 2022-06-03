using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OmiyaGames.Audio
{
	// FIXME: move this to a different package
	public struct AssetRef<TObject> : IEquatable<TObject>, IEquatable<AssetRef<TObject>>, IEquatable<AssetReferenceT<TObject>> where TObject : UnityEngine.Object
	{
		public enum State
		{
			Null,
			DirectAsset,
			Unloaded,
			Loading,
			Ready
		}

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
		public State CurrentState
		{
			get
			{
				if (Reference == null)
				{
					if (Asset != null)
					{
						return State.DirectAsset;
					}

					return State.Null;
				}

				if (string.IsNullOrEmpty(Reference.AssetGUID))
				{
					return State.Null;
				}

				if (isLoading)
				{
					return State.Loading;
				}

				if (Asset != null)
				{
					return State.Ready;
				}

				return State.Unloaded;
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
			if (CurrentState == State.Unloaded)
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
			if (Reference.OperationHandle.IsDone)
			{
				// Release the handle
				Addressables.Release(Reference.OperationHandle);

				// Reset member variables
				Asset = null;
			}
		}

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
	}
}
