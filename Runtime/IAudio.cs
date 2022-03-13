using UnityEngine;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="IAudio.cs" company="Omiya Games">
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
	/// <strong>Date:</strong> 8/18/2015<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// <item>
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
	/// An abstract class that implements methods shared in <code>BackgroundMusic</code>,
	/// <code>AmbientMusic</code>, and <code>SoundEffect</code>.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	[System.Obsolete("Doesn't add anything")]
	public abstract class IAudio : MonoBehaviour
	{
		/// <summary>
		/// TODO
		/// </summary>
		public enum State
		{
			/// <summary>
			/// TODO
			/// </summary>
			Stopped,
			/// <summary>
			/// TODO
			/// </summary>
			Playing,
			/// <summary>
			/// TODO
			/// </summary>
			Paused
		}

		[HideInInspector]
		AudioSource audioCache = null;
		State currentState = State.Stopped;

		/// <summary>
		/// Indicates whether this sound effect also pauses when
		/// <seealso cref="Managers.TimeManager.IsManuallyPaused"/>
		/// is true.
		/// </summary>
		public abstract bool IsPausedOnTimeStop
		{
			get;
			set;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public virtual AudioSource CurrentAudio => Helpers.GetComponentCached(this, ref audioCache);

		/// <summary>
		/// TODO
		/// </summary>
		public State CurrentState
		{
			get
			{
				UpdateStateToAudioIsPlaying(ref currentState);
				return currentState;
			}
			set
			{
				UpdateStateToAudioIsPlaying(ref currentState);
				if (ChangeAudioSourceState(currentState, value) == true)
				{
					currentState = value;
				}
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void Play() => CurrentState = State.Playing;

		/// <summary>
		/// TODO
		/// </summary>
		public void Stop() => CurrentState = State.Stopped;

		/// <summary>
		/// TODO
		/// </summary>
		public void Pause() => CurrentState = State.Paused;

		protected virtual void Awake()
		{
			if (CurrentAudio.playOnAwake == true)
			{
				currentState = State.Playing;
			}

			CurrentAudio.ignoreListenerPause = !IsPausedOnTimeStop;
		}

		protected virtual void Reset()
		{
			audioCache = GetComponent<AudioSource>();
		}

		#region Helper Methods
		protected virtual void UpdateStateToAudioIsPlaying(ref State state)
		{
			if ((state == State.Playing) && (CurrentAudio.isPlaying == false))
			{
				state = State.Stopped;
			}
			else if ((state == State.Stopped) && (CurrentAudio.isPlaying == true))
			{
				state = State.Playing;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="before"></param>
		/// <param name="after"></param>
		/// <returns>
		/// Returns true if changing from <paramref name="before"/>
		/// to <paramref name="after"/> results in a change of state.
		/// </returns>
		protected virtual bool ChangeAudioSourceState(State before, State after)
		{
			// Make sure the before and after are two different states
			bool returnFlag = false;
			if (before != after)
			{
				// Check the state we're switching into
				switch (after)
				{
					case State.Playing:
						if (before == State.Stopped)
						{
							// Play the audio if we're switching from stopped to playing
							CurrentAudio.Play();
						}
						else
						{
							// Unpause the audio if we're switching from paused to play
							CurrentAudio.UnPause();
						}
						returnFlag = true;
						break;
					case State.Paused:
						if (before == State.Playing)
						{
							// Pause the audio if we're switching from playing to paused
							CurrentAudio.Pause();
							returnFlag = true;
						}
						break;
					default:
						// Check if we're paused
						if (before == State.Paused)
						{
							// Unpause the audio
							CurrentAudio.UnPause();
						}

						// Stop the audio
						CurrentAudio.Stop();
						returnFlag = true;
						break;
				}
			}
			return returnFlag;
		}
		#endregion
	}
}
