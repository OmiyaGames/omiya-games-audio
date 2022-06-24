using UnityEditor;
using UnityEngine;
using OmiyaGames.Common.Editor;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="PlaybackBehaviorDrawer.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/23/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial version.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// An editor to make it easier to edit <see cref="PlaybackBehavior"/> fields.
	/// </summary>
	/// <seealso cref="MusicDataStack.FadeLayer"/>
	[CustomPropertyDrawer(typeof(PlaybackBehavior))]
	public class PlaybackBehaviorDrawer : PropertyDrawer
	{
		const string NAME_BEHAVIOR = "behavior";
		const string NAME_FADE_DURATION = "fadeDurationSeconds";
		const string NAME_AUDIO = "audioFile";
		const string NAME_RESTART = "alwaysRestart";

		/// <inheritdoc/>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.PropertyScope(position, label, property))
			{
				// Check the current behavior
				SerializedProperty behavior = property.FindPropertyRelative(NAME_BEHAVIOR);
				if (behavior.enumValueIndex == (int)PlaybackBehavior.FadeBehavior.DoNothing)
				{
					// Show a single enum
					EditorGUI.PropertyField(position, behavior, label, false);
					return;
				}

				// Draw a fold-out
				position.height = EditorGUIUtility.singleLineHeight;
				property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);

				// Draw a single enum in the same row
				var newBehavior = (PlaybackBehavior.FadeBehavior)EditorGUI.EnumPopup(position, label, (PlaybackBehavior.FadeBehavior)behavior.enumValueIndex);
				behavior.enumValueIndex = (int)newBehavior;

				// Check if expanded
				if (property.isExpanded == false)
				{
					return;
				}

				// Increment indentation
				int lastLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel += 1;

				// Change display based on behavior
				if (behavior.enumValueIndex == (int)PlaybackBehavior.FadeBehavior.FadeToSilence)
				{
					// Draw the fade duration
					position.y += EditorGUIUtility.singleLineHeight + EditorHelpers.VerticalMargin;
					EditorGUI.PropertyField(position, property.FindPropertyRelative(NAME_FADE_DURATION), false);
				}
				else
				{
					// Draw the audio file
					position.y += EditorGUIUtility.singleLineHeight + EditorHelpers.VerticalMargin;
					EditorGUI.PropertyField(position, property.FindPropertyRelative(NAME_AUDIO), false);

					// Draw the fade duration
					position.y += EditorGUIUtility.singleLineHeight + EditorHelpers.VerticalMargin;
					EditorGUI.PropertyField(position, property.FindPropertyRelative(NAME_FADE_DURATION), false);

					// Draw the fade duration
					position.y += EditorGUIUtility.singleLineHeight + EditorHelpers.VerticalMargin;
					EditorGUI.PropertyField(position, property.FindPropertyRelative(NAME_RESTART), false);
				}

				// Revert indentation
				EditorGUI.indentLevel = lastLevel;
			}
		}

		/// <inheritdoc/>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			// Check if property is expanded
			if (property.isExpanded)
			{
				// Change height based on behavior settings
				SerializedProperty behavior = property.FindPropertyRelative("behavior");
				switch (behavior.enumValueIndex)
				{
					case (int)PlaybackBehavior.FadeBehavior.FadeToSilence:
						// Show two properties
						return EditorHelpers.GetHeight(2);
					case (int)PlaybackBehavior.FadeBehavior.FadeInNewAudio:
						// Show all properties
						return EditorHelpers.GetHeight(4);
				}
			}

			// Show only one property
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
