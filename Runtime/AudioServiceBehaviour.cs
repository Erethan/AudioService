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

        private void Update()
        {
            foreach (var order in _ongoingOrders)
            {
                if (order.Origin == null)
                    continue;
                order.Source.transform.position = order.Origin.position;
            }
        }

        public void PlayAudio(AudioPlayOrder order)
        {
            order.Source = _pool.Request();
            order.Source.clip = order.Clip;
            order.Source.loop = order.Loop;
            order.Source.volume = order.Volume;
            order.Source.pitch = order.Pitch;
            order.Source.spatialBlend = order.SpacialBlend;

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

            
            yield return new WaitWhile(() => order.Source.isPlaying);
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