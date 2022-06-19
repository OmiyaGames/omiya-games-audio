using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="SingleLoopingMusicEditor.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 6/19/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial version.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// An editor to make it easier to edit <see cref="SingleLoopingMusic"/> scripts.
	/// </summary>
	/// <seealso cref="SingleLoopingMusic"/>
	[CustomEditor(typeof(SingleLoopingMusic), true)]
	[CanEditMultipleObjects]
	public class SingleLoopingMusicEditor : UnityEditor.Editor
	{
		const string LABEL_PLAY = "Play";
		const string LABEL_STOP = "Stop";

		readonly BackgroundAudioPreview audioPreview = new();
		ProgressBar previewProgressBar = null;
		double loopStartTime = 0;

		/// <inheritdoc/>
		public override VisualElement CreateInspectorGUI()
		{
			// Create a tree from the UXML file.
			VisualElement returnTree = new VisualElement();
			VisualTreeAsset originalTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.omiyagames.audio/Editor/SingleLoopingMusic.uxml");
			originalTree.CloneTree(returnTree);

			// Configure intro stinger to auto-enable the play after loop fields
			ObjectField checkIntro = returnTree.Q<ObjectField>("introSting");
			DoubleField playAfter = returnTree.Q<DoubleField>("playLoopAfterSeconds");
			Button resetplayAfter = returnTree.Q<Button>("resetPlayLoopAfterButton");

			bool enableControls = (checkIntro.value != null);
			playAfter.SetEnabled(enableControls);
			resetplayAfter.SetEnabled(enableControls);

			checkIntro.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
			{
				bool enableControls = (e.newValue != null);
				playAfter.SetEnabled(enableControls);
				resetplayAfter.SetEnabled(enableControls);

				// If intro is set to a new value, update it's duration
				if (enableControls && (e.newValue != e.previousValue))
				{
					TargetMusic.SetLoopDelayToIntroStingDuration();
				}
			});
			resetplayAfter.RegisterCallback<ClickEvent>(e =>
			{
				TargetMusic.SetLoopDelayToIntroStingDuration();
			});

			// Configure the preview stuff
			previewProgressBar = returnTree.Q<ProgressBar>("playerProgress");
			Button previewButton = returnTree.Q<Button>("previewButton");

			previewButton.text = LABEL_PLAY;

			previewButton.RegisterCallback<ClickEvent>(e =>
			{
				if (audioPreview.IsPlaying)
				{
					audioPreview.Stop();
					previewButton.text = LABEL_PLAY;
				}
				else
				{
					audioPreview.Play();
					previewButton.text = LABEL_STOP;
					if (TargetMusic.IntroSting != null)
					{
						loopStartTime = audioPreview.PlayStartTime + TargetMusic.PlayLoopAfterSeconds;
					}
				}

				// Zero the progress bar
				previewProgressBar.value = 0;
			});

			// Bind to the object
			returnTree.Bind(serializedObject);
			return returnTree;
		}

		/// <inheritdoc/>
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (audioPreview.IsPlaying && (previewProgressBar != null))
			{
				// FIXME: actuall indicate progress in the audio preview
			}
		}

		SingleLoopingMusic TargetMusic => ((SingleLoopingMusic)target);
	}
}
