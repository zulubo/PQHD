using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    public class Audio : MonoBehaviour
    {
        public static Audio I;
        
        [SerializeField] AudioMixerGroup mixerGroup;

        private List<AudioSource> pool = new();
        
        
        private void Awake()
        {
            I = this;
        }

        public class PlayingSound
        {
            public AudioSource source;
            public bool finished = false;
            public Transform follow;
            
            public PlayingSound(AudioSource source)
            {
                this.source = source;
                this.follow = null;
            }

            public PlayingSound(AudioSource source, Transform follow)
            {
                this.source = source;
                this.follow = follow;
            }

            public void Stop()
            {
                if (!finished)
                {
                    finished = true;
                    source.Stop();
                    source.enabled = false;
                }
            }
        }

        private AudioSource GetPooledSource()
        {
            AudioSource source = pool.Find(p => !p.enabled);
            if (source == null)
            {
                source = new GameObject("AudioSource").AddComponent<AudioSource>();
                source.outputAudioMixerGroup = mixerGroup;
                pool.Add(source);
            }
            source.enabled = true;
            return source;
        }
        

        /// <summary>
        /// Play a 2D sound
        /// </summary>
        public PlayingSound PlaySound2D(AudioResource sound, float volume = 1, float pitch = 1)
        {
            PlayingSound playing = new PlayingSound(GetPooledSource());
            playing.source.spatialBlend = 0;
            playing.source.volume = volume;
            playing.source.pitch = pitch;
            playing.source.resource = sound;
            playing.source.Play();
            StartCoroutine(SoundCoroutine(playing));
            return playing;
        }

        /// <summary>
        /// Play a 3D sound at a position
        /// </summary>
        public PlayingSound PlaySound3D(AudioResource sound, Vector3 position, float radius, float volume = 1, float pitch = 1, float doppler = 0)
        {
            PlayingSound playing = new PlayingSound(GetPooledSource());
            playing.source.spatialBlend = 1;
            playing.source.transform.position = position;
            SetSpatialCurve(playing.source, radius);
            playing.source.volume = volume;
            playing.source.pitch = pitch;
            playing.source.resource = sound;
            playing.source.dopplerLevel = doppler;
            playing.source.Play();
            StartCoroutine(SoundCoroutine(playing));
            return playing;
        }
        
        /// <summary>
        /// Play a 3D sound that follows a transform
        /// </summary>
        public PlayingSound PlaySound3D(AudioResource sound, Transform follow, float radius, float volume = 1, float pitch = 1, float doppler = 0)
        {
            PlayingSound playing = new PlayingSound(GetPooledSource());
            playing.source.spatialBlend = 1;
            playing.source.transform.position = follow.position;
            playing.follow = follow;
            SetSpatialCurve(playing.source, radius);
            playing.source.volume = volume;
            playing.source.pitch = pitch;
            playing.source.resource = sound;
            playing.source.dopplerLevel = doppler;
            playing.source.Play();
            StartCoroutine(SoundCoroutine(playing));
            return playing;
        }

        private static readonly AnimationCurve RolloffCurve = new()
        {
            keys = new[]
            {
                new Keyframe(0.2f, 1, 0, -5),
                new Keyframe(0.5f, 0.3f, -1f, -1f),
                new Keyframe(1, 0, 0, 0),
            }
        };

        void SetSpatialCurve(AudioSource source, float radius)
        {
            source.maxDistance = radius;
            source.minDistance = radius * 0.2f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, RolloffCurve);
        }

        IEnumerator SoundCoroutine(PlayingSound sound)
        {
            while (sound.source.isPlaying && !sound.finished)
            {
                if (sound.follow) sound.source.transform.position = sound.follow.position;
                yield return null;
            }
            sound.Stop();
        }
        
        
    }
}
