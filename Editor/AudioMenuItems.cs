using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AudioMenuItems.cs" company="Omiya Games">
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
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 3/7/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>
	/// Initial draft.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Add menu items for creating audio scripts into the game.
	/// </summary>
	public class AudioMenuItems : MonoBehaviour
	{
		[MenuItem("GameObject/Audio/One-off Sound Effect", false, 100)]
		static void CreateOneOffSoundEffect(MenuCommand command)
		{
			// Create the new object
			CreateSoundEffectScript(command, "One-off Sound Effect", out GameObject newObject, out SoundEffect effect, out AudioSource audioSource);

			// Setup each component
			// TODO: set layer setting to 3
			effect.IsMutatingPitch = true;
			effect.IsMutatingVolume = true;
			SetupSpatialAudioSource(audioSource, GetSettings()?.SoundEffects);

			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
		}

		[MenuItem("GameObject/Audio/One-off Voice", false, 101)]
		static void CreateOneOffVoice(MenuCommand command)
		{
			// Create the new object
			CreateSoundEffectScript(command, "One-off Voice", out GameObject newObject, out SoundEffect effect, out AudioSource audioSource);

			// Setup each component
			effect.IsMutatingPitch = false;
			effect.IsMutatingVolume = true;
			SetupSpatialAudioSource(audioSource, GetSettings()?.Voices);

			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
		}

		static AudioSettings GetSettings()
		{
			EditorBuildSettings.TryGetConfigObject(AudioManager.CONFIG_NAME, out AudioSettings settings);
			return settings;
		}

		static void CreateSoundEffectScript(MenuCommand command, string name, out GameObject newObject, out SoundEffect effect, out AudioSource audioSource)
		{
			// Check if there's a canvas in the parent
			bool applyRectTransform = false;
			GameObject parentObject = command.context as GameObject;
			if (parentObject != null)
			{
				Transform checkIsUx = parentObject.transform;
				while (checkIsUx != null)
				{
					if (checkIsUx is RectTransform || checkIsUx.TryGetComponent<Canvas>(out _))
					{
						// Halt if transform is already a rect transform, or it has a Canvas component
						applyRectTransform = true;
						break;
					}
					checkIsUx = checkIsUx.transform.parent;
				}
			}

			// Create a custom game object
			newObject = applyRectTransform ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);

			// Ensure the game object gets parented and named correctly
			GameObjectUtility.SetParentAndAlign(newObject, parentObject);
			GameObjectUtility.EnsureUniqueNameForSibling(newObject);

			// Add the appropriate components
			effect = newObject.AddComponent<SoundEffect>();
			audioSource = newObject.GetComponent<AudioSource>();
			UnityEditorInternal.ComponentUtility.MoveComponentDown(audioSource);

			// Highlight this object
			Selection.activeObject = newObject;
		}

		static void SetupSpatialAudioSource(AudioSource audioSource, AudioLayer.Spatial layer)
		{
			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 1f;
			audioSource.loop = false;

			if (layer != null)
			{
				if (audioSource.transform is RectTransform)
				{
					audioSource.outputAudioMixerGroup = layer.DefaultUiGroup;
				}
				else
				{
					audioSource.outputAudioMixerGroup = layer.DefaultGroup;
				}
			}
		}
	}
}
