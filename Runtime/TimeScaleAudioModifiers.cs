using UnityEngine;
using UnityEngine.Audio;

namespace OmiyaGames.Audio
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="TimeScaleAudioModifiers.cs" company="Omiya Games">
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
	/// Helper struct for params in <seealso cref="AudioSettings"/>.
	/// </summary>
	[System.Serializable]
	public class TimeScaleAudioModifiers
	{
		[SerializeField]
		AudioMixer mixer;
		[SerializeField]
		AudioMixerSnapshot defaultSnapshot;

		[Header("Pause")]
		[SerializeField]
		bool enablePause = true;
		[SerializeField]
		AudioMixerSnapshot pausedSnapshot;

		[Header("Slow")]
		[SerializeField]
		bool enableSlow = false;
		[SerializeField]
		AudioMixerSnapshot slowTimeSnapshot;
		[SerializeField]
		Vector2 slowTimeRange = new Vector2(0.5f, 1f);
		[SerializeField]
		Vector2 slowPitchRange = new Vector2(0.5f, 1f);

		[Header("Fast")]
		[SerializeField]
		bool enableFast = false;
		[SerializeField]
		AudioMixerSnapshot fastTimeSnapshot;
		[SerializeField]
		Vector2 fastTimeRange = new Vector2(1f, 5f);
		[SerializeField]
		Vector2 fastPitchRange = new Vector2(1f, 5f);

		[Header("Pitch")]
		[SerializeField]
		string pitchParam;

		/// <summary>
		/// TODO
		/// </summary>
		public AudioMixer Mixer => mixer;
		/// <summary>
		/// TODO
		/// </summary>
		public string PitchParam => pitchParam;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioMixerSnapshot DefaultSnapshot => defaultSnapshot;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioMixerSnapshot PausedSnapshot => pausedSnapshot;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioMixerSnapshot SlowTimeSnapshot => slowTimeSnapshot;
		/// <summary>
		/// TODO
		/// </summary>
		public Vector2 SlowTimeRange => slowTimeRange;
		/// <summary>
		/// TODO
		/// </summary>
		public Vector2 SlowPitchRange => slowPitchRange;
		/// <summary>
		/// TODO
		/// </summary>
		public AudioMixerSnapshot FastTimeSnapshot => fastTimeSnapshot;
		/// <summary>
		/// TODO
		/// </summary>
		public Vector2 FastTimeRange => fastTimeRange;
		/// <summary>
		/// TODO
		/// </summary>
		public Vector2 FastPitchRange => fastPitchRange;
		/// <summary>
		/// TODO
		/// </summary>
		public bool EnablePause => enablePause;
		/// <summary>
		/// TODO
		/// </summary>
		public bool EnableSlow => enableSlow;
		/// <summary>
		/// TODO
		/// </summary>
		public bool EnableFast => enableFast;
	}
}
