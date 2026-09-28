namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    public class AudioPlayer : MonoBehaviour
    {
        public static AudioPlayer Singleton { get; private set; } = null;

        [SerializeField]
        private List<AudioClip> audioClips = new List<AudioClip>();

        [SerializeField]
        private int audioSourcesToCreate = 3;
        private List<AudioSource> audioSources { get; set; } = new List<AudioSource>();

        [SerializeField]
        private float MinimumPitchForBlip;

        [SerializeField]
        private float MaximumPitchForBlip;

        private void Awake()
        {
            // If we already have an instance, don't make another
            if (Singleton != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Singleton = this;
            DontDestroyOnLoad(this.gameObject);

            for (int ii = 0; ii < audioSourcesToCreate; ii++)
            {
                GameObject audioSourceObject = new GameObject();
                audioSourceObject.transform.parent = this.transform;
                AudioSource source = audioSourceObject.AddComponent<AudioSource>();
                this.audioSources.Add(source);
            }
        }

        public void PlayBlip()
        {
            AudioSource toUse = null;

            for (int ii = 0; ii < this.audioSources.Count; ii++)
            {
                if (this.audioSources[ii].isPlaying)
                {
                    continue;
                }

                toUse = this.audioSources[ii];
            }

            if (toUse == null)
            {
                return;
            }

            toUse.clip = this.audioClips[Random.Range(0, this.audioClips.Count - 1)];
            toUse.pitch = Random.Range(this.MinimumPitchForBlip, this.MaximumPitchForBlip);
            toUse.Play();
        }
    }
}
