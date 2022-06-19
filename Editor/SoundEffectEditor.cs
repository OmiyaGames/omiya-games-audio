using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using OmiyaGames.Common.Editor;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="SoundEffectEditor.cs" company="Omiya Games">
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2014-2022 Omiya Games
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
	/// <strong>Date:</strong> 5/25/2015<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial version.</description>
	/// </item><term>
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 2/18/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Moved to new package.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// An editor to make it easier to edit <see cref="SoundEffect"/> scripts.
	/// </summary>
	/// <seealso cref="SoundEffect"/>
	[CustomEditor(typeof(SoundEffect), true)]
	[CanEditMultipleObjects]
	public class SoundEffectEditor : UnityEditor.Editor
	{
		/// <inheritdoc/>
		public override VisualElement CreateInspectorGUI()
		{
			// Create a tree from the UXML file.
			VisualElement returnTree = new VisualElement();
			VisualTreeAsset originalTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.omiyagames.audio/Editor/SoundEffect.uxml");
			originalTree.CloneTree(returnTree);

			// Make sure toggles disable their respective sliders
			Toggle checkToggle = returnTree.Q<Toggle>("mutatePitchToggle");
			RangeSlider pitchSlider = returnTree.Q<RangeSlider>("mutatePitchSlider");
			pitchSlider.SetEnabled(checkToggle.value);
			checkToggle.RegisterCallback<ChangeEvent<bool>>(e => pitchSlider.SetEnabled(e.newValue));

			checkToggle = returnTree.Q<Toggle>("mutateVolumeToggle");
			RangeSlider volumeSlider = returnTree.Q<RangeSlider>("mutateVolumeSlider");
			volumeSlider.SetEnabled(checkToggle.value);
			checkToggle.RegisterCallback<ChangeEvent<bool>>(e => volumeSlider.SetEnabled(e.newValue));

			// Setup the limits of all sliders
			SliderInt layerSlider = returnTree.Q<SliderInt>("maxLayers");
			layerSlider.lowValue = SoundEffect.MIN_LAYERS;
			layerSlider.highValue = SoundEffect.MAX_LAYERS;
			pitchSlider.lowLimit = SoundEffect.MIN_PITCH;
			pitchSlider.highLimit = SoundEffect.MAX_PITCH;
			volumeSlider.lowLimit = SoundEffect.MIN_VOLUME;
			volumeSlider.highLimit = SoundEffect.MAX_VOLUME;

			// Bind to the object
			returnTree.Bind(serializedObject);
			return returnTree;
		}
	}
}
