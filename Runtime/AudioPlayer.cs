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

		[SerializeField] private UnityEvent FinishEvent = default;

		/*
		[Header("Configuration")]
		[SerializeField] private AudioConfigurationSO _audioConfiguration = default;
		*/

		private List<AudioPlayOrder> _ongoingOrders;

		private void Start()
		{
			_ongoingOrders = new List<AudioPlayOrder>();
			if (_playOnStart)
				StartCoroutine(PlayDelayed());
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.S))
			{
				for (int i = 0; i < _ongoingOrders.Count; i++)
				{

					if (!(_ongoingOrders[i].State == AudioPlayOrder.PlayState.Playing))
					{
						continue;
					}
					service.StopAudio(_ongoingOrders[i]);
				}
				_ongoingOrders.Clear();
			}
			else if (Input.GetKeyDown(KeyCode.P))
			{
				if (_ongoingOrders.Count == 0)
				{
					PlayAudioCue();
				}
			}
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
				FinishEvent.Invoke();
			}
		}

	}
}