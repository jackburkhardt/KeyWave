using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yarn.Unity;
using Random = UnityEngine.Random;

// See https://github.com/jackburkhardt/KeyWave/wiki/Audio-System for more info on the audio system.
    public class AudioManager : MonoBehaviour
    {
        public static AudioSource SFXAudioSource;
        public static AudioSource MusicAudioSource;
        
        private static Dictionary<string, AudioClip> backgroundAudioClips = new Dictionary<string, AudioClip>();
        private static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private static Dictionary<string, List<AudioClip>> audioClipsByCategory = new Dictionary<string, List<AudioClip>>();

        private void Awake()
        {
            LoadClips();
            LoadBackgroundClips();
            SFXAudioSource = gameObject.AddComponent<AudioSource>();
            MusicAudioSource = gameObject.AddComponent<AudioSource>();
        }

        private void LoadClips()
        {
            // loads all clips in the top level folder (meaning they dont have a category)
            var uncategorizedClips = Resources.LoadAll<AudioClip>("Audio/Clips");
            foreach (var clip in uncategorizedClips)
            {
                audioClips.Add(clip.name, clip);
                
                // check if clip name has '_' in it, if so, it is part of a category
                // create the category if it doesn't exist, and add the clip to the category
                if (!clip.name.Contains("_")) continue;
                
                // the category name is the first part of the clip name, before the '_'
                var category = clip.name.Split('_')[0];
                if (!audioClipsByCategory.ContainsKey(category))
                {
                    audioClipsByCategory.Add(category, new List<AudioClip>());
                }
                audioClipsByCategory[category].Add(clip);
            }
        }

        private void LoadBackgroundClips()
        {
            var backgroundClips = Resources.LoadAll<AudioClip>("Audio/Background");
            foreach (var clip in backgroundClips)
            {
                backgroundAudioClips.Add(clip.name, clip);
            }
        }
        
        /// <summary>
        /// Plays the audio clip with the given name. Must match the filename of the clip.
        /// If the clip is not found, nothing plays. If another clip is playing, this will interrupt it.
        /// </summary>
        [YarnCommand("play_clip")]
        public static void PlayClip(string clipName)
        {
            if (audioClips.ContainsKey(clipName))
            {
                SFXAudioSource.PlayOneShot(audioClips[clipName]);
            }
            else
            {
                Debug.LogError($"PlayClip: Clip {clipName} not found.");
            }
        }
        
        /// <summary>
        /// Plays a random audio clip from the given category.
        /// If the category is not found, nothing plays. If another clip is playing, this will interrupt it.
        /// </summary>
        [YarnCommand("play_random_clip")]
        public static void PlayRandomClip(string categoryName)
        {
            if (audioClipsByCategory.ContainsKey(categoryName))
            {
                SFXAudioSource.PlayOneShot(audioClipsByCategory[categoryName]
                    [Random.Range(0, audioClipsByCategory[categoryName].Count)]);
            }
            else
            {
                Debug.LogError($"PlayRandomClip: Category {categoryName} not found.");
            }
        }
        
        /// <summary>
        /// Plays the audio clip with the given name. Must match the filename of the clip.
        /// If the clip is not found, nothing plays. If another clip is playing, this will interrupt it.
        /// </summary>
        [YarnCommand("play_background_clip")]
        public static void PlayBackgroundClip(string clipName)
        {
            if (audioClips.ContainsKey(clipName))
            {
                MusicAudioSource.PlayOneShot(audioClips[clipName]);
            }
            else
            {
                Debug.LogError($"PlayBackgroundClip: Clip {clipName} not found.");
            }
        }
    }
