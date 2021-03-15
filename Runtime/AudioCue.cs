using System;
using UnityEngine;


namespace Erethan.AudioService
{

	/// <summary>
	/// A collection of audio clips that are played in parallel, and support randomisation.
	/// </summary>
	[CreateAssetMenu(fileName = "new AudioCue", menuName = "Inside/Audio/Audio Cue")]
	public class AudioCue : ScriptableObject
	{
		public bool looping = false;
		[SerializeField] private AudioClipsGroup[] _audioClipGroups = default;


		public AudioPlayOrder[] GetNewOrders()
		{
			int numberOfClips = _audioClipGroups.Length;
			AudioPlayOrder[] newOrders = new AudioPlayOrder[numberOfClips];
			for (int i = 0; i < numberOfClips; i++)
			{
				newOrders[i] = new AudioPlayOrder()
				{
					audioCue = this,
					Clip = _audioClipGroups[i].GetNextClip(),
					Loop = looping,
					State = AudioPlayOrder.PlayState.Playing
				};

			}

			return newOrders;
		}

		public AudioClip[] GetClips()
		{
			int numberOfClips = _audioClipGroups.Length;
			AudioClip[] resultingClips = new AudioClip[numberOfClips];

			for (int i = 0; i < numberOfClips; i++)
			{
				resultingClips[i] = _audioClipGroups[i].GetNextClip();
			}

			return resultingClips;
		}
	}

	/// <summary>
	/// Represents a group of AudioClips that can be treated as one, and provides automatic randomisation or sequencing based on the <c>SequenceMode</c> value.
	/// </summary>
	[Serializable]
	public class AudioClipsGroup
	{
		public SequenceMode sequenceMode = SequenceMode.RandomNoImmediateRepeat;
		public AudioClip[] audioClips;

		private int _nextClipToPlay = -1;
		private int _lastClipPlayed = -1;

		/// <summary>
		/// Chooses the next clip in the sequence, either following the order or randomly.
		/// </summary>
		/// <returns>A reference to an AudioClip</returns>
		public AudioClip GetNextClip()
		{
			// Fast out if there is only one clip to play
			if (audioClips.Length == 1)
				return audioClips[0];

			if (_nextClipToPlay == -1)
			{
				// Index needs to be initialised: 0 if Sequential, random if otherwise
				_nextClipToPlay = (sequenceMode == SequenceMode.Sequential) ? 0 : UnityEngine.Random.Range(0, audioClips.Length);
			}
			else
			{
				// Select next clip index based on the appropriate SequenceMode
				switch (sequenceMode)
				{
					case SequenceMode.Random:
						_nextClipToPlay = UnityEngine.Random.Range(0, audioClips.Length);
						break;

					case SequenceMode.RandomNoImmediateRepeat:
						do
						{
							_nextClipToPlay = UnityEngine.Random.Range(0, audioClips.Length);
						} while (_nextClipToPlay == _lastClipPlayed);
						break;

					case SequenceMode.Sequential:
						_nextClipToPlay = (int)Mathf.Repeat(++_nextClipToPlay, audioClips.Length);
						break;
				}
			}

			_lastClipPlayed = _nextClipToPlay;

			return audioClips[_nextClipToPlay];
		}

		public enum SequenceMode
		{
			Random,
			RandomNoImmediateRepeat,
			Sequential,
		}
	}


	[Serializable]
	public class AudioPlayOrder
	{

		public bool Loop { get; set; }
		public AudioCue audioCue { get; set; }
		public AudioClip Clip { get; set; }
		public PlayState State { get; set; }
		internal AudioSource Source { get; set; }

		public event Action<AudioPlayOrder> Finish;

		public enum PlayState
		{
			Ordered,
			Playing,
			Paused,
			Stopped,
			Finished
		}

		public void UpdateState(PlayState newState)
		{
			if (State == PlayState.Playing
				&& newState == PlayState.Finished)
			{
				State = newState;
				Finish?.Invoke(this);
				return;
			}
			State = newState;
		}

	}
}