using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Erethan.AudioService
{

    [CreateAssetMenu(fileName = "Audio Service", menuName = "Erethan/Audio/Audio Service")]
    public class AudioService : ScriptableObject
    {
        private AudioServiceBehaviour _controllerBehaviour;
        private AudioServiceBehaviour ControllerBehaviour
        {
            get
            {
                if (_controllerBehaviour == null)
                {
                    _controllerBehaviour = AudioServiceBehaviour.CreateNew(this);
                }
                return _controllerBehaviour;
            }
            set
            {
                _controllerBehaviour = value;
            }
        }

        public void Initialize()
        {
            _ = ControllerBehaviour;
        }



        public void PlayAudio(AudioPlayOrder order)
        {
            ControllerBehaviour.PlayAudio(order);
        }

        public void StopAudio(AudioPlayOrder order)
        {
            ControllerBehaviour.StopAudio(order);
        }


        [Header("Audio Source Pool")]
        [SerializeField] private int _initialPoolSize = default;
        [SerializeField] private AudioSource _audioSourcePrefab = default;
        public int InitialPoolSize => _initialPoolSize;
        public AudioSource AudioSourcePrefab => _audioSourcePrefab;

    }
}