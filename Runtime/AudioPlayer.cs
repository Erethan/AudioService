using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Erethan.AudioService
{

	public class AudioPlayer : MonoBehaviour
	{
		[SerializeField] private AudioService service = default;
		[SerializeField] private AudioCue _audioCue = default;
		[SerializeField] private bool _playOnStart = default;

		[SerializeField] private UnityEvent _finish = default;

		private List<AudioPlayOrder> _ongoingOrders;

		private void Awake()
		{
			_ongoingOrders = new List<AudioPlayOrder>();
		}

        private void Start()
        {
			if (_playOnStart)
				StartCoroutine(PlayDelayed());
        }

        private IEnumerator PlayDelayed()
		{
			yield return new WaitForSeconds(.1f);

			PlayAudioCue();
		}

		public void PlayAudioCue()
		{
			foreach (var order in _audioCue.GetNewOrders())
			{
				order.Origin = transform;
				order.Finish += OnOrderFinish;
				_ongoingOrders.Add(order);

				service.PlayAudio(order);
			}
		}

		public void StopAudioCue()
		{
			foreach (var order in _ongoingOrders)
			{
				service.StopAudio(order);
			}
		}

		private void OnOrderFinish(AudioPlayOrder finishedOrder)
		{
			finishedOrder.Finish -= OnOrderFinish;
			_ongoingOrders.Remove(finishedOrder);

			if (finishedOrder.State == AudioPlayOrder.PlayState.Finished)
			{
				_finish.Invoke();
			}
		}

	}
}