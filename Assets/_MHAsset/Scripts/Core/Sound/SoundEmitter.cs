using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace MH.Core.Sound
{
    

    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour {

        #region ------------ Properties  --------------

        public SoundData Data { get; private set; }
        public LinkedListNode<SoundEmitter> Node { get; set; }

        IObjectPool<SoundEmitter> _pool;
        AudioSource audioSource;
        Coroutine playingCoroutine;

        #endregion

        #region --------- Unity Methods ----------

        void Awake() { 
            
        }

        #endregion

        #region ------------ Public Methods ----------

        public void SetUp(IObjectPool<SoundEmitter> pool)
        {
            if(audioSource == null) audioSource = GetComponent<AudioSource>();
            _pool = pool;
        }
            
        
        public void Initialize(SoundData data) {
            Data = data;
            audioSource.clip = data.clip;
            audioSource.outputAudioMixerGroup = data.mixerGroup;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;
            
            audioSource.mute = data.mute;
            audioSource.bypassEffects = data.bypassEffects;
            audioSource.bypassListenerEffects = data.bypassListenerEffects;
            audioSource.bypassReverbZones = data.bypassReverbZones;
            
            audioSource.priority = data.priority;
            audioSource.volume = data.volume;
            audioSource.pitch = data.pitch;
            audioSource.panStereo = data.panStereo;
            audioSource.spatialBlend = data.spatialBlend;
            audioSource.reverbZoneMix = data.reverbZoneMix;
            audioSource.dopplerLevel = data.dopplerLevel;
            audioSource.spread = data.spread;
            
            audioSource.minDistance = data.minDistance;
            audioSource.maxDistance = data.maxDistance;
            
            audioSource.ignoreListenerVolume = data.ignoreListenerVolume;
            audioSource.ignoreListenerPause = data.ignoreListenerPause;
            
            audioSource.rolloffMode = data.rolloffMode;
        }

        public void Play() {
            if (playingCoroutine != null) {
                StopCoroutine(playingCoroutine);
            }
            
            audioSource.Play();
            playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }
        
        public void Stop() {
            if (playingCoroutine != null) {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }
            
            audioSource.Stop();
            
            // to-do: return to pool
            //soundManager.Instance.ReturnToPool(this);
            _pool.Release(this);
        }

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f) {
            audioSource.pitch += Random.Range(min, max);
        }
        
        
        #endregion

        IEnumerator WaitForSoundToEnd() {
            yield return new WaitWhile(() => audioSource.isPlaying);
            Stop();
        }

        
    }
}
