using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Erethan.ScriptableServices;
using Erethan.ScriptableServices.Pool;

namespace Erethan.AudioService
{

    public class AudioServiceBehaviour : ScriptableServiceBehaviour
    {
        public AudioSource SourcePrefab { get; set; }
        public int InitialPoolSize { get; set; }
        private List<AudioPlayOrder> _ongoingOrders;

        private ComponentPool<AudioSource> _pool;

        public override void Initialize()
        {
            _ongoingOrders = new List<AudioPlayOrder>();

            _pool = new ComponentPool<AudioSource>()
            {
                Prefab = SourcePrefab
            };
            _pool.SetParent(transform);
            _pool.Prewarm(InitialPoolSize);
        }

        public void PlayAudio(AudioPlayOrder order)
        {
            order.Source = _pool.Request();

            order.Source.clip = order.Clip;
            order.Source.loop = order.Loop;


            StartCoroutine(SourcePlayingRoutine(order));
        }

        public void StopAudio(AudioPlayOrder order)
        {
            if (order.State != AudioPlayOrder.PlayState.Playing
                && order.State != AudioPlayOrder.PlayState.Paused)
                return;

            if (!_ongoingOrders.Contains(order))
                return;
            order.Source.Stop();
            _pool.Return(order.Source);
            _ongoingOrders.Remove(order);
            order.UpdateState(AudioPlayOrder.PlayState.Stopped);
            order.Source = null;
        }

        private IEnumerator SourcePlayingRoutine(AudioPlayOrder order)
        {
            order.Source.Play();
            order.UpdateState(AudioPlayOrder.PlayState.Playing);
            _ongoingOrders.Add(order);

            while (order.Source != null &&
                order.Source.isPlaying)
            {
                yield return new WaitForSeconds(order.Clip.length);
            }

            if (order.Source == null)
            {
                yield break;
            }
            _pool.Return(order.Source);
            _ongoingOrders.Remove(order);
            order.UpdateState(AudioPlayOrder.PlayState.Finished);
            order.Source = null;
        }

    }
}