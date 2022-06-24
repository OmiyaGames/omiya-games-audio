using UnityEngine;
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
	/// <strong>Version:</strong> 1.0.0<br/>
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
		ProgressBar introProgressBar = null, loopProgressBar = null;
		double introEndTime = 0, loopStartTime = 0;

		/// <inheritdoc/>
		public override VisualElement CreateInspectorGUI()
		{
			// Create a tree from the UXML file.
			VisualElement returnTree = new VisualElement();
			VisualTreeAsset originalTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.omiyagames.audio/Editor/SingleLoopingMusic.uxml");
			originalTree.CloneTree(returnTree);

			// Configure intro stinger to auto-enable the play after loop fields
			introProgressBar = returnTree.Q<ProgressBar>("introProgress");
			loopProgressBar = returnTree.Q<ProgressBar>("loopProgress");
			ObjectField checkIntro = returnTree.Q<ObjectField>("introSting");
			ObjectField checkLoop = returnTree.Q<ObjectField>("mainLoop");
			DoubleField playAfter = returnTree.Q<DoubleField>("playLoopAfterSeconds");
			Button resetplayAfter = returnTree.Q<Button>("resetPlayLoopAfterButton");
			Button previewButton = returnTree.Q<Button>("previewButton");

			// Setup the controls
			previewButton.text = LABEL_PLAY;
			UpdateIntroControls(checkIntro.value != null);
			UpdateIntroPreview((TargetAudio.IntroSting != null));
			UpdateLoopPreview(TargetAudio.Loop);
			UpdatePreviewControls((TargetAudio.IntroSting != null) || (TargetAudio.Loop != null));

			checkIntro.RegisterCallback<ChangeEvent<Object>>(e =>
			{
				bool enableControls = (e.newValue != null);
				UpdateIntroControls(enableControls);

				// If intro is set to a new value, update it's duration
				if (enableControls && (e.newValue != e.previousValue))
				{
					TargetAudio.SetLoopDelayToIntroStingDuration();
				}

				// Update preview
				UpdateIntroPreview(enableControls);
				UpdateLoopPreview(TargetAudio.Loop);
				UpdatePreviewControls(enableControls || (TargetAudio.Loop != null));
			});
			checkLoop.RegisterCallback<ChangeEvent<Object>>(e =>
			{
				// Update preview
				UpdateLoopPreview(e.newValue as AudioClip);
				UpdateIntroPreview(TargetAudio.IntroSting != null);
				UpdatePreviewControls((TargetAudio.IntroSting != null) && (e.newValue != null));
			});
			resetplayAfter.RegisterCallback<ClickEvent>(e =>
			{
				TargetAudio.SetLoopDelayToIntroStingDuration();
			});
			previewButton.RegisterCallback<ClickEvent>(e =>
			{
				if (audioPreview.IsPlaying)
				{
					audioPreview.Dispose();
					previewButton.text = LABEL_PLAY;
					introProgressBar.value = 0;
					loopProgressBar.value = 0;
				}
				else
				{
					audioPreview.Play(TargetAudio);
					previewButton.text = LABEL_STOP;

					// Setup floats
					introEndTime = audioPreview.PlayStartTime;
					if (TargetAudio.IntroSting != null)
					{
						introProgressBar.lowValue = 0;
						introProgressBar.highValue = (float)TargetAudio.PlayLoopAfterSeconds;
						introEndTime += TargetAudio.PlayLoopAfterSeconds;
					}
					if (TargetAudio.Loop != null)
					{
						loopProgressBar.lowValue = 0;
						loopProgressBar.highValue = (float)AudioManager.CalculateClipLengthSeconds(TargetAudio.Loop);
					}
					loopStartTime = introEndTime;
				}

				// Zero the progress bar
				introProgressBar.value = 0;
			});

			// Bind to the object
			returnTree.Bind(serializedObject);
			return returnTree;

			void UpdateIntroControls(bool enableControls)
			{
				playAfter.SetEnabled(enableControls);
				resetplayAfter.SetEnabled(enableControls);
			}

			void UpdateIntroPreview(bool display)
			{
				if (display)
				{
					introProgressBar.style.display = DisplayStyle.Flex;

					// Check if intro is visible
					if (loopProgressBar.style.display == DisplayStyle.None)
					{
						// If not, take full length
						introProgressBar.style.width = new Length(100, LengthUnit.Percent);
					}
					else
					{
						// Otherwise, calculate width
						double introLength = TargetAudio.PlayLoopAfterSeconds;
						double fullLength = introLength + AudioManager.CalculateClipLengthSeconds(TargetAudio.Loop);
						introProgressBar.style.width = new Length((float)(introLength * 100 / fullLength), LengthUnit.Percent);
					}
				}
				else
				{
					introProgressBar.style.display = DisplayStyle.None;
				}
			}

			void UpdateLoopPreview(AudioClip loop)
			{
				if (loop != null)
				{
					// Display progress bar
					loopProgressBar.style.display = DisplayStyle.Flex;

					// Check if intro is visible
					if (introProgressBar.style.display == DisplayStyle.None)
					{
						// If not, take full length
						loopProgressBar.style.width = new Length(100, LengthUnit.Percent);
					}
					else
					{
						// Otherwise, calculate width
						double loopLength = AudioManager.CalculateClipLengthSeconds(loop);
						double fullLength = loopLength + TargetAudio.PlayLoopAfterSeconds;
						loopProgressBar.style.width = new Length((float)(loopLength * 100 / fullLength), LengthUnit.Percent);
					}
				}
				else
				{
					loopProgressBar.style.display = DisplayStyle.None;
				}
			}

			void UpdatePreviewControls(bool enable)
			{
				introProgressBar.SetEnabled(enable);
				loopProgressBar.SetEnabled(enable);
				previewButton.SetEnabled(enable);
			}
		}

		/// <inheritdoc/>
		void UpdateProgressBar()
		{
			if (audioPreview.IsPlaying)
			{
				// Check if intro progress bar is visible
				if ((introProgressBar != null) && (introProgressBar.style.display != DisplayStyle.None))
				{
					// Update intro progress
					if (UnityEngine.AudioSettings.dspTime < introEndTime)
					{
						introProgressBar.value = (float)(UnityEngine.AudioSettings.dspTime - audioPreview.PlayStartTime);
					}
					else
					{
						introProgressBar.value = introProgressBar.highValue;
					}
				}

				// Check if loop progress bar is visible
				if ((loopProgressBar != null) && (loopProgressBar.style.display != DisplayStyle.None))
				{
					// Offset loop start time
					while ((UnityEngine.AudioSettings.dspTime - loopStartTime) > loopProgressBar.highValue)
					{
						loopStartTime += loopProgressBar.highValue;
					}

					// Update loop progress
					if (UnityEngine.AudioSettings.dspTime > introEndTime)
					{
						loopProgressBar.value = (float)(UnityEngine.AudioSettings.dspTime - loopStartTime);
					}
					else
					{
						loopProgressBar.value = loopProgressBar.lowValue;
					}
				}
			}
		}

		/// <inheritdoc/>
		void OnDestroy()
		{
			OnDisable();
		}

		/// <inheritdoc/>
		void OnEnable()
		{
			EditorApplication.update += UpdateProgressBar;
		}

		/// <inheritdoc/>
		void OnDisable()
		{
			EditorApplication.update -= UpdateProgressBar;
			audioPreview.Dispose();
		}

		SingleLoopingMusic TargetAudio => ((SingleLoopingMusic)target);
	}
}
