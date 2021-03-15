using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inside.Pool;

public class AudioSystemBehaviour : MonoBehaviour
{
    public AudioSystem System { get; private set; }
    private List<AudioPlayOrder> _ongoingOrders;

    private ComponentPool<AudioSource> _pool;

    public static AudioSystemBehaviour CreateNew(AudioSystem system)
    {
        AudioSystemBehaviour instance = new GameObject()
            .AddComponent<AudioSystemBehaviour>();
        instance.System = system;
        DontDestroyOnLoad(instance.gameObject);
        instance.gameObject.name = $"{typeof(AudioSystemBehaviour)}";
        instance.Initialize();
        return instance;
    }

    private void Initialize()
    {
        _ongoingOrders = new List<AudioPlayOrder>();

        _pool = new ComponentPool<AudioSource>()
        {
            Prefab = System.AudioSourcePrefab
        };
        _pool.SetParent(transform);
        _pool.Prewarm(System.InitialPoolSize);
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

        if(order.Source == null)
        {
            yield break;
        }
        _pool.Return(order.Source);
        _ongoingOrders.Remove(order);
        order.UpdateState(AudioPlayOrder.PlayState.Finished);
        order.Source = null;
    }
}
