using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="PlaybackBehavior.cs" company="Omiya Games">
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
	/// <description>Initial draft.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Helper serialized class to display in Unity inspector. Generates
	/// details for fade-ins and fade-outs.
	/// </summary>
	[System.Serializable]
	public class PlaybackBehavior
	{
		public const float DEFAULT_FADE_DURATION = 0.5f;

		/// <summary>
		/// The action to take.
		/// </summary>
		public enum FadeBehavior
		{
			/// <summary>
			/// Do nothing; let's the last audio
			/// continue to play.
			/// </summary>
			DoNothing,
			/// <summary>
			/// Fade out the last playing audio into silence.
			/// </summary>
			FadeToSilence,
			/// <summary>
			/// Fade out the last playing audio,
			/// and fade in <seealso cref="AudioFile"/>.
			/// </summary>
			FadeInNewAudio,
		}

		[SerializeField]
		FadeBehavior behavior = FadeBehavior.FadeInNewAudio;
		[SerializeField]
		AssetRefSerialized<BackgroundAudio> audioFile;
		[SerializeField]
		[Range(0, 30f)]
		double fadeDurationSeconds = DEFAULT_FADE_DURATION;
		[SerializeField]
		[Tooltip("If true, restarts the music even if it's already playing in the background.")]
		bool alwaysRestart = false;

		/// <summary>
		/// Constructs a new instance with default arguments.
		/// </summary>
		/// <param name="behavior">
		/// Sets default <seealso cref="Behavior"/>.
		/// </param>
		/// <param name="fadeDurationSeconds">
		/// Sets default <seealso cref="FadeDurationSeconds"/>.
		/// </param>
		/// <param name="alwaysRestart">
		/// Sets default <seealso cref="AlwaysRestart"/>.
		/// </param>
		public PlaybackBehavior(FadeBehavior behavior = FadeBehavior.DoNothing, double fadeDurationSeconds = DEFAULT_FADE_DURATION, bool alwaysRestart = false)
		{
			this.behavior = behavior;
			this.fadeDurationSeconds = fadeDurationSeconds;
			this.alwaysRestart = alwaysRestart;
		}

		/// <summary>
		/// Gets the action to take.
		/// </summary>
		/// <remarks>
		/// This property is set in the Unity Inspector.
		/// </remarks>
		public FadeBehavior Behavior
		{
			get
			{
				// If the audio file is not set, perform a fade-out instead
				if ((behavior == FadeBehavior.FadeInNewAudio) && (AudioFile.HasValue == false))
				{
					return FadeBehavior.FadeToSilence;
				}
				else
				{
					return behavior;
				}
			}
		}
		/// <summary>
		/// Gets how long to perform the fade-in or fade-out, in seconds.
		/// Normalized by <seealso cref="UnityEngine.AudioSettings.dspTime"/>.
		/// </summary>
		/// <remarks>
		/// This property is set in the Unity Inspector.
		/// </remarks>
		public double FadeDurationSeconds => fadeDurationSeconds;
		/// <summary>
		/// Gets the audio to fade in.
		/// </summary>
		/// <remarks>
		/// This property is set in the Unity Inspector.
		/// </remarks>
		public AssetRefSerialized<BackgroundAudio> AudioFile => audioFile;
		/// <summary>
		/// If <see langword="true"/>, restarts playing
		/// <seealso cref="AudioFile"/> even if it's already
		/// playing in the background.
		/// </summary>
		/// <remarks>
		/// This property is set in the Unity Inspector.
		/// </remarks>
		public bool AlwaysRestart => alwaysRestart;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="script"></param>
		/// <param name="audioLayer"></param>
		/// <returns></returns>
		public Coroutine StartCoroutine(MonoBehaviour script, AudioLayer.Background audioLayer, System.Action<BackgroundAudio.Player> onFinish = null)
		{
			switch (Behavior)
			{
				case FadeBehavior.FadeInNewAudio:
					// Fade in this audio file
					return script.StartCoroutine(audioLayer.PlayNextCoroutine(AudioFile, GetFadeInArgs(), GetFadeOutArgs(), onFinish));
				case FadeBehavior.FadeToSilence:
					// Fade out last playing audio
					audioLayer.FadeOutCurrentPlaying(GetFadeOutArgs());
					break;
			}

			// By default, return null
			onFinish?.Invoke(null);
			return null;
		}

		/// <summary>
		/// Generates a fade-in arguments based on this instance's
		/// properties.
		/// </summary>
		/// <returns>
		/// The fade-in arguments using
		/// <seealso cref="FadeDurationSeconds"/> and
		/// <seealso cref="AlwaysRestart"/>.
		/// </returns>
		public FadeInArgs GetFadeInArgs() => new()
		{
			DurationSeconds = FadeDurationSeconds,
			ForceRestart = AlwaysRestart,
		};

		/// <summary>
		/// Generates a fade-out arguments based on this instance's
		/// properties.
		/// </summary>
		/// <returns>
		/// The fade-in arguments using
		/// <seealso cref="FadeDurationSeconds"/>.
		/// </returns>
		public FadeOutArgs GetFadeOutArgs() => new()
		{
			DurationSeconds = FadeDurationSeconds,
		};
	}
}
