using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

// TODO: consider moving this to a different package so other addressable-relying
// packages can rely on this struct, too.
namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AssetRefSerialized.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/18/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Helper serialized class to display in Unity inspector. Generates an
	/// <seealso cref="AssetRef{TObject}"/>.
	/// </summary>
	[Serializable]
	public class AssetRefSerialized<TObject> where TObject : UnityEngine.Object
	{
		[SerializeField]
		bool useAddressable = false;
		[SerializeField]
		TObject asset;
		[SerializeField]
		AssetReferenceT<TObject> assetRef;

		/// <summary>
		/// Converts this object to a struct.
		/// </summary>
		public static implicit operator AssetRef<TObject>(AssetRefSerialized<TObject> convert) => convert.Create();

		/// <summary>
		/// Generates a new instance of <seealso cref="AssetRef{TObject}"/>.
		/// </summary>
		public AssetRef<TObject> Create() => useAddressable ? new(assetRef) : new(asset);

		/// <summary>
		/// Indicates if asset is null or not.
		/// </summary>
		public bool HasValue => (useAddressable && !string.IsNullOrEmpty(assetRef.AssetGUID)) || (!useAddressable && (asset != null));
	}
}
