using UnityEditor;
using UnityEngine;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="FadeLayerDrawer.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/18/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial version.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// An editor to make it easier to edit <see cref="AssetRefSerialized"/> fields.
	/// </summary>
	/// <seealso cref="MusicDataStack.FadeLayer"/>
	[CustomPropertyDrawer(typeof(AssetRefSerialized<>))]
	public class AssetRefSerializedDrawer : PropertyDrawer
	{
		const int CHECKBOX_WIDTH = 112;

		/// <inheritdoc/>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Draw label first
			position = EditorGUI.PrefixLabel(position, label);

			// Determine position of object field
			position.width -= CHECKBOX_WIDTH + Common.Editor.EditorHelpers.VerticalMargin;

			// Draw object field
			SerializedProperty useAddressable = property.FindPropertyRelative("useAddressable");
			if (useAddressable.boolValue)
			{
				SerializedProperty test = property.FindPropertyRelative("assetRef");
				EditorGUI.PropertyField(position, test, GUIContent.none);
			}
			else
			{
				SerializedProperty asset = property.FindPropertyRelative("asset");
				EditorGUI.ObjectField(position, asset, GUIContent.none);
			}

			// Determine position of checkbox
			position.x += position.width + Common.Editor.EditorHelpers.VerticalMargin;
			position.width = CHECKBOX_WIDTH;

			// Draw checkbox
			useAddressable.boolValue = EditorGUI.ToggleLeft(position, useAddressable.displayName, useAddressable.boolValue);
		}
	}
}
