namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Holds and pools <see cref="AudioSource"/> to play random <see cref="AudioClip"/>s.
    /// Useful for small variant sound effects.
    /// </summary>
    public class RandomAudioPlayer : MonoBehaviour
    {
        /// <summary>
        /// Reference to this Singleton. Once included in a Scene, should never unload.
        /// Relies on placement of "Singletons.prefab" in some loaded scene.
        /// Set in <see cref="Awake"/>; do not refer to Singleton in other <see cref="Awake"/> scripts.
        /// </summary>
        public static RandomAudioPlayer Singleton { get; private set; } = null;

        /// <summary>
        /// Audio clips to choose from. One will be selected entirely at randomly.
        /// If empty, no sound will play.
        /// </summary>
        [SerializeField]
        private List<AudioClip> audioClips = new List<AudioClip>();

        /// <summary>
        /// Number of <see cref="AudioSource"/>s to pool. Will create up to this many, and attach them to
        /// <see cref="GameObject"/>s that are children of this object.
        /// If this object is frequently invoked to make sounds, it may opt to not play a sound if there are no
        /// currently-not-playing <see cref="AudioSource"/>s in the pool.
        /// </summary>
        [SerializeField]
        private int audioSourcesToPool = 3;

        /// <summary>
        /// Currently pooled <see cref="AudioSource"/>.
        /// As this object is set to <see cref="Object.DontDestroyOnLoad"/>, these <see cref="AudioSource"/>s
        /// will be retained across scenes.
        /// </summary>
        private List<AudioSource> pooledAudioSources { get; set; } = new List<AudioSource>();

        /// <summary>
        /// When a sound is played, a number is chosen randomly from 0-1.
        /// That is used in this curve's 'time' value to determine a pitch.
        /// The played sound effect will use that pitch.
        /// 1 is unaffected, 2 is much higher pitch, .5 is much lower pitch.
        /// </summary>
        [SerializeReference]
        private AnimationCurve pitchCurve;

        private void Awake()
        {
            // If we already have an instance, don't make another
            if (Singleton != null)
            {
                Destroy(this.gameObject);
                return;
            }

            // This is now the singleton; unparent it so that we can mark as DontDestroyOnLoad
            this.transform.SetParent(null);
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);

            this.PoolAdditionalAudioSources(this.audioSourcesToPool);
        }

        /// <summary>
        /// Chooses a <see cref="AudioClip"/> from <see cref="audioClips"/> at random,
        /// applies a variant pitch to it using <see cref="pitchCurve"/>,
        /// and tries to play it on an available <see cref="pooledAudioSources"/>.
        /// If there are no available sources, won't play anything.
        /// </summary>
        public void PlayRandomSound()
        {
            // If there are no clips, we can't play anything!
            if (this.audioClips.Count == 0)
            {
                return;
            }

            // Find a non-busy AudioSource
            AudioSource toUse = null;

            for (int ii = 0; ii < this.pooledAudioSources.Count; ii++)
            {
                if (this.pooledAudioSources[ii].isPlaying)
                {
                    continue;
                }

                toUse = this.pooledAudioSources[ii];
                break;
            }

            // If there weren't any open, we can't play
            if (toUse == null)
            {
                return;
            }

            toUse.clip = this.audioClips[Random.Range(0, this.audioClips.Count - 1)];
            toUse.pitch = this.pitchCurve.Evaluate(Random.Range(0, 1f));
            toUse.Play();
        }

        /// <summary>
        /// Creates additional <see cref="GameObject"/>s to hold one <see cref="AudioSource"/> component each.
        /// </summary>
        /// <param name="count">Number to add to currently pooled resources.</param>
        private void PoolAdditionalAudioSources(int count)
        {
            for (int ii = 0; ii < count; ii++)
            {
                GameObject audioSourceObject = new GameObject();
                audioSourceObject.transform.parent = this.transform;
                AudioSource source = audioSourceObject.AddComponent<AudioSource>();
                this.pooledAudioSources.Add(source);
            }
        }
    }
}
