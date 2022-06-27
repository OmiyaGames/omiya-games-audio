using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using OmiyaGames.Common.Editor;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="TimeScaleAudioModifiersDrawer.cs" company="Omiya Games">
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
	/// <strong>Version:</strong> 0.1.0-exp.1<br/>
	/// <strong>Date:</strong> 3/14/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial version.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// An editor to make it easier to edit <see cref="TimeScaleAudioModifiers"/> scripts.
	/// </summary>
	/// <seealso cref="TimeScaleAudioModifiers"/>
	[CustomPropertyDrawer(typeof(TimeScaleAudioModifiers))]
	public class TimeScaleAudioModifiersDrawer : PropertyDrawer
	{
		/// <inheritdoc/>
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			// Create container from UXML
			VisualTreeAsset originalTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.omiyagames.audio/Editor/PropertyDrawers/TimeScaleAudioModifiers.uxml");
			VisualElement container = originalTree.CloneTree(property.propertyPath);

			// Grab some controls
			Toggle pauseToggle = container.Q<Toggle>("enablePause");
			ObjectField pauseSnapshot = container.Q<ObjectField>("pausedSnapshot");

			Toggle slowToggle = container.Q<Toggle>("enableSlow");
			RangeSlider slowRange = container.Q<RangeSlider>("slowTimeRange");
			VisualElement slowGroup = container.Q<VisualElement>("slowGroup");

			Toggle fastToggle = container.Q<Toggle>("enableFast");
			RangeSlider fastRange = container.Q<RangeSlider>("fastTimeRange");
			VisualElement fastGroup = container.Q<VisualElement>("fastGroup");

			TextField pitchParam = container.Q<TextField>("pitchParam");

			// Update UI states
			UpdatePauseField(pauseToggle.value);
			UpdateSlowFields(slowToggle.value);
			UpdateFastFields(fastToggle.value);

			// Listen to UI behavior
			pauseToggle.RegisterCallback<ChangeEvent<bool>>(e => UpdatePauseField(e.newValue));
			slowToggle.RegisterCallback<ChangeEvent<bool>>(e => UpdateSlowFields(e.newValue));
			fastToggle.RegisterCallback<ChangeEvent<bool>>(e => UpdateFastFields(e.newValue));

			return container;
			void UpdatePauseField(bool isEnabled) => pauseSnapshot.SetEnabled(isEnabled);

			void UpdateSlowFields(bool isEnabled)
			{
				slowRange.SetEnabled(isEnabled);

				// Only show fields if setting is enabled
				slowGroup.style.display = isEnabled ? DisplayStyle.Flex : DisplayStyle.None;

				// Update pitch section
				UpdatePitchGroup(isEnabled, fastToggle.value);
			}

			void UpdateFastFields(bool isEnabled)
			{
				fastRange.SetEnabled(isEnabled);

				// Only show fields if setting is enabled
				fastGroup.style.display = isEnabled ? DisplayStyle.Flex : DisplayStyle.None;

				// Update pitch section
				UpdatePitchGroup(slowToggle.value, isEnabled);
			}

			void UpdatePitchGroup(bool isSlowEnabled, bool isFastEnabled) => pitchParam.SetEnabled(isSlowEnabled || isFastEnabled);
		}
	}
}
