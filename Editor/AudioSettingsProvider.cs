using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using OmiyaGames.Global.Settings.Editor;
using OmiyaGames.Saves.Editor;

namespace OmiyaGames.Audio.Editor
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="AudioSettingsProvider.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 2/16/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Editor for <see cref="AudioSettings"/>.
	/// Appears under the Project Settings window.
	/// </summary>
	public class AudioSettingsProvider : BaseSettingsEditor<AudioSettings>
	{
		/// <inheritdoc/>
		public override string DefaultSettingsFileName => "AudioSettings";
		/// <inheritdoc/>
		public override string UxmlPath => AudioManager.UXML_PATH;
		/// <inheritdoc/>
		public override string AddressableGroupName => SettingsEditorHelpers.OMIYA_GAMES_GROUP_NAME;
		/// <inheritdoc/>
		public override string AddressableName => AudioManager.ADDRESSABLE_NAME;
		/// <inheritdoc/>
		public override string ConfigName => AudioManager.CONFIG_NAME;
		/// <inheritdoc/>
		public override string HeaderText => "Audio Settings";
		/// <inheritdoc/>
		public override string HelpUrl => "https://omiyagames.github.io/omiya-games-audio";

		/// <summary>
		/// Constructs a project-scoped <see cref="SettingsProvider"/>.
		/// </summary>
		public AudioSettingsProvider(string path, IEnumerable<string> keywords) : base(path, keywords) { }

		/// <summary>
		/// Registers this <see cref="SettingsProvider"/>.
		/// </summary>
		/// <returns></returns>
		[SettingsProvider]
		public static SettingsProvider CreateSettingsProvider()
		{
			// Create the settings provider
			return new AudioSettingsProvider(AudioManager.SIDEBAR_PATH, GetSearchKeywordsFromGUIContentProperties<Styles>());
		}

		/// <inheritdoc/>
		protected override VisualElement CustomizeEditSettingsTree(VisualElement returnTree, SerializedObject serializedSettings)
		{
			// Add behavior to the active settings field
			ObjectField settingsField = returnTree.Query<ObjectField>("activeSettings");
			settingsField.value = ActiveSettings;
			settingsField.RegisterCallback<ChangeEvent<Object>>(e => OnUserChangedActiveSettings(e.newValue as AudioSettings));

			// Add behavior to the saves button
			Button addSavesButton = returnTree.Query<Button>("addSaveObjects");
			addSavesButton.RegisterCallback<ClickEvent>(e =>
			{
				AudioSettings audio = ActiveSettings;
				int dataAdded = SavesSettingsProvider.AddSaveData(
					audio.Main.VolumeSaver
					, audio.Main.IsMutedSaver
					, audio.Music.VolumeSaver
					, audio.Music.IsMutedSaver
					, audio.SoundEffects.VolumeSaver
					, audio.SoundEffects.IsMutedSaver
					, audio.Voices.VolumeSaver
					, audio.Voices.IsMutedSaver
					, audio.Ambience.VolumeSaver
					, audio.Ambience.IsMutedSaver);
				EditorUtility.DisplayDialog("Added Save Data", $"Added {dataAdded} save data objects into save settings.", "OK");
			});
			return base.CustomizeEditSettingsTree(returnTree, serializedSettings);
		}

		class Styles
		{
			public static readonly GUIContent ActiveSettings = new GUIContent("Active Settings");
			public static readonly GUIContent Mixer = new GUIContent("Mixer");
			
			public static readonly GUIContent VolumeControls = new GUIContent("Volume Controls");
			public static readonly GUIContent MuteVolume = new GUIContent("Mute Volume (dB)");
			public static readonly GUIContent VolumePercentToDbCurve = new GUIContent("Volume % to dB Curve");
			
			public static readonly GUIContent MainLayer = new GUIContent("Main Layer");
			public static readonly GUIContent MusicLayer = new GUIContent("Music Layer");
			public static readonly GUIContent SoundEffectsLayer = new GUIContent("Sound Effects Layer");
			public static readonly GUIContent VoicesLayer = new GUIContent("Voices Layer");
			public static readonly GUIContent AmbienceLayer = new GUIContent("Ambience Layer");

			public static readonly GUIContent VolumeSaver = new GUIContent("Volume Saver");
			public static readonly GUIContent IsMutedSaver = new GUIContent("Is Muted Saver");
			public static readonly GUIContent VolumeParam = new GUIContent("Volume Param");
			public static readonly GUIContent PitchParam = new GUIContent("Pitch Param");
			public static readonly GUIContent DefaultGroup = new GUIContent("Default Group");
			public static readonly GUIContent DefaultUiGroup = new GUIContent("Default UI Group");

			public static readonly GUIContent AddSaversToSavesSettings = new GUIContent("Add Savers to Saves Settings");

			public static readonly GUIContent Snapshots = new GUIContent("Snapshots");
			public static readonly GUIContent TimeScaleSnapshots = new GUIContent("Time Scale Snapshots");
			public static readonly GUIContent DefaultSnapshot = new GUIContent("Default Snapshot");
			public static readonly GUIContent EnablePause = new GUIContent("Enable Pause");
			public static readonly GUIContent PausedSnapshot = new GUIContent("Paused Snapshot");
			public static readonly GUIContent EnableSlow = new GUIContent("Enable Slow");
			public static readonly GUIContent SlowTimeSnapshot = new GUIContent("Slow Time Snapshot");
			public static readonly GUIContent EnableQuicken = new GUIContent("Enable Quicken");
			public static readonly GUIContent QuickenTimeSnapshot = new GUIContent("Quicken Time Snapshot");
			public static readonly GUIContent SlowTimeRange = new GUIContent("Slow Time Range");
			public static readonly GUIContent LowestPitch = new GUIContent("Lowest Pitch");
			public static readonly GUIContent QuickenTimeRange = new GUIContent("Quicken Time Range");
			public static readonly GUIContent HighestPitch = new GUIContent("Highest Pitch");
		}
	}
}
