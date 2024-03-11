using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Yarn.Unity;

namespace PerilsAndPitfalls
{
    public class DialogueAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip Daniel;
        public AudioClip Douglas;

        string currentSpeakingActor;

        Dictionary<string, Tuple<AudioClip, float>> VoiceBox;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            VoiceBox = new Dictionary<string, Tuple<AudioClip, float>>() 
        {
            {"Daniel", Tuple.Create(Daniel, 0.1f)},
            {"Douglas", Tuple.Create(Douglas, 0.15f)}
        };
        }

        private void OnEnable()
        { 
            //GameEvent.onActorDialogueLine += SetSpeakingActor;
        }

        private void OnDisable()
        {
            //GameEvent.onActorDialogueLine -= SetSpeakingActor;
        }

        void SetSpeakingActor(string name)
        {
            currentSpeakingActor = name;
        }

        bool buffering = false;

       IEnumerator ClipBuffer()
        {
            buffering = true;
            float bufferLength = 0.1f;
            if (VoiceBox.ContainsKey(currentSpeakingActor)) bufferLength = VoiceBox[currentSpeakingActor].Item2;
            while (bufferLength > 0)
            {
                bufferLength -= Time.deltaTime;
                yield return null;
            }
            buffering = false;
        }


        public void PlayVoice()
        {
            if (currentSpeakingActor == null) return;
            if (buffering) return;
            StartCoroutine(ClipBuffer());
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            audioSource.Stop();
            if (VoiceBox.ContainsKey(currentSpeakingActor)) audioSource.PlayOneShot(VoiceBox[currentSpeakingActor].Item1);
        }




    }
}
