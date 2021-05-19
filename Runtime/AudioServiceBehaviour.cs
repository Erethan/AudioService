using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            SceneManager.sceneUnloaded += OnActiveSceneChanged;

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
            order.UpdateState(AudioPlayOrder.PlayState.Stopped);
            _ongoingOrders.Remove(order);
            _pool.Return(order.Source);
        }

        public void FadeStopAudio(AudioPlayOrder order, float fadeSeconds = 1f)
        {
            if(fadeSeconds <= 0)
            {
                StopAudio(order);
                return;
            }    

            StartCoroutine(FadeStopRoutine(order, fadeSeconds));
        }

        private IEnumerator FadeStopRoutine(AudioPlayOrder order, float fadeSeconds)
        {
            
            float startVolume = order.Source.volume;
            float startTime = Time.time;
            while (order.Source.volume > 0)
            {
                yield return null;
                if (order.State != AudioPlayOrder.PlayState.Playing)
                    yield break;
                float volumeRatio = 1 - (Time.time - startTime)/fadeSeconds;
                order.Source.volume = Mathf.Clamp01(startVolume * volumeRatio);
            }
            StopAudio(order);
        }

        private IEnumerator SourcePlayingRoutine(AudioPlayOrder order)
        {
            order.Source.Play();
            order.UpdateState(AudioPlayOrder.PlayState.Playing);
            _ongoingOrders.Add(order);

            
            yield return new WaitWhile(() => order.Source.isPlaying);
            if (!_ongoingOrders.Contains(order))
            {
                yield break;
            }

            order.UpdateState(AudioPlayOrder.PlayState.Finished);
            _ongoingOrders.Remove(order);
            _pool.Return(order.Source);
        }


        private void OnActiveSceneChanged(Scene unloadedScene)
        {
            for (int i = _ongoingOrders.Count - 1; i >= 0; i--)
            {
                StopAudio(_ongoingOrders[i]);
            }
            
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnActiveSceneChanged;
        }


    }

    
}