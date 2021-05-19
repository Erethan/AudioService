using UnityEngine;
using Erethan.ScriptableServices;

namespace Erethan.AudioService
{
    [CreateAssetMenu(fileName = "Audio Service", menuName = "Erethan/Audio/Audio Service")]
    public class AudioService : ScriptableService<AudioServiceBehaviour>
    {
        [Header("Scene Change")]
        [SerializeField] private bool _fadeOnSceneChange = false;
        [SerializeField] private float _fadeSeconds = 0.5f;

        [Header("Audio Source Pool")]
        [SerializeField] private int _initialPoolSize = default;
        [SerializeField] private AudioSource _audioSourcePrefab = default;
        
        public int InitialPoolSize => _initialPoolSize;
        public AudioSource AudioSourcePrefab => _audioSourcePrefab;

        protected override void ConfigureBehaviour()
        {
            ControllerBehaviour.SourcePrefab = _audioSourcePrefab;
            ControllerBehaviour.InitialPoolSize = _initialPoolSize;
            ControllerBehaviour.FadeOnSceneChange = _fadeOnSceneChange;
            ControllerBehaviour.FadeSeconds = _fadeSeconds;
        }

        public void PlayAudio(AudioPlayOrder order) => ControllerBehaviour.PlayAudio(order);
        public void StopAudio(AudioPlayOrder order) => ControllerBehaviour.StopAudio(order);
        public void FadeStopAudio(AudioPlayOrder order, float fadeSeconds = 1f) => ControllerBehaviour.FadeStopAudio(order, fadeSeconds);

    }
}