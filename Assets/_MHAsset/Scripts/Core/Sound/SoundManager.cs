using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MH.Core.Sound
{
    /// <summary>
    /// Manages the audio system by using Object Pooling for better performance and memory management.
    /// This class supports frequent and one-time sound emitters with a max sound instance limit.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        #region ------------ Inspectors ------------

        [Header("Sound Emitter Prefab")] [SerializeField]
        SoundEmitter soundEmitterPrefab; // Prefab used to create new SoundEmitter instances

        [Header("Pooling Settings")]
        [Tooltip("Enable Collection Check to avoid releasing objects that are not from the pool.")]
        [SerializeField]
        bool collectionCheck = true; // If true, will perform safety checks when releasing objects back to the pool

        [Tooltip("Initial Pool Capacity")] [SerializeField]
        int defaultCapacity = 10; // Initial capacity of the sound emitter pool

        [Tooltip("Maximum Pool Size")] [SerializeField]
        int maxPoolSize = 100; // Maximum number of sound emitters that can be pooled

        [Header("Sound Limits")] [Tooltip("Maximum simultaneous frequent sounds allowed")] [SerializeField]
        int maxSoundInstances = 30; // Limit for frequent sounds playing at the same time

        #endregion

        #region ---------- Properties ------------

        // Pool that manages the lifecycle of SoundEmitter objects
        private IObjectPool<SoundEmitter> _soundEmitterPool;

        // List to store all active SoundEmitter instances (both frequent and one-time sounds)
        private readonly List<SoundEmitter> _activeSoundEmitters = new();

        /// <summary>
        /// Stores frequent sounds (like footsteps, button clicks) that are played repeatedly.
        /// Using LinkedList for faster removal operations compared to List or Queue.
        /// </summary>
        public readonly LinkedList<SoundEmitter> SoundEmitterCounter = new();

        #endregion

        #region ----------------- Unity Methods ------------

        void Start()
        {
            InitializePool();
        }

        #endregion

        #region ------------ Public Methods ---------

        /// <summary>
        /// Creates a new SoundBuilder instance for building sound playback requests.
        /// </summary>
        public SoundBuilder CreateSoundBuilder() => new SoundBuilder(this);

        /// <summary>
        /// Checks if a sound can be played based on its type and the max instance limit.
        /// </summary>
        /// <param name="data">Sound data to be checked</param>
        /// <returns>True if the sound can be played, false otherwise</returns>
        public bool CanPlaySound(SoundData data)
        {
            // Non-frequent sounds are always allowed
            if (!data.frequentSound) return true;

            // Check if the maximum limit of frequent sounds has been reached
            if (SoundEmitterCounter.Count >= maxSoundInstances)
            {
                try
                {
                    // Stop the oldest sound (FIFO - First In First Out) to make room for the new sound
                    SoundEmitterCounter.First.Value.Stop();
                    return true;
                }
                catch
                {
                    Debug.Log("SoundEmitter is already released");
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a SoundEmitter instance from the pool.
        /// </summary>
        /// <returns>SoundEmitter instance</returns>
        public SoundEmitter Get()
        {
            return _soundEmitterPool.Get();
        }

        /// <summary>
        /// Returns a SoundEmitter back to the pool.
        /// </summary>
        /// <param name="soundEmitter">The SoundEmitter instance to return</param>
        public void ReturnToPool(SoundEmitter soundEmitter)
        {
            _soundEmitterPool.Release(soundEmitter);
        }

        /// <summary>
        /// Stops all currently playing sounds and clears all active sound emitters.
        /// </summary>
        public void StopAll()
        {
            foreach (var soundEmitter in _activeSoundEmitters)
            {
                soundEmitter.Stop();
            }

            // Clear frequent sound emitters linked list
            SoundEmitterCounter.Clear();
        }

        #endregion

        #region ------------ Private Methods ---------

        /// <summary>
        /// Initializes the object pool with custom creation and return methods.
        /// </summary>
        void InitializePool()
        {
            _soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter, // Object Creation
                OnTakeFromPool, // On Take (Activate)
                OnReturnedToPool, // On Return (Deactivate)
                OnDestroyPoolObject, // On Destroy
                collectionCheck, // Enable collection safety checks
                defaultCapacity, // Initial capacity
                maxPoolSize); // Maximum pool size
        }

        /// <summary>
        /// Instantiates a new SoundEmitter prefab.
        /// </summary>
        SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.SetUp(_soundEmitterPool);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        /// <summary>
        /// Called when a SoundEmitter is taken from the pool.
        /// </summary>
        void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
            _activeSoundEmitters.Add(soundEmitter);
        }

        /// <summary>
        /// Called when a SoundEmitter is returned to the pool.
        /// </summary>
        void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            // Remove soundEmitter from SoundEmitterCounter if it's playing frequent sound
            if (soundEmitter.Node != null)
            {
                SoundEmitterCounter.Remove(soundEmitter.Node);
                soundEmitter.Node = null;
            }

            soundEmitter.gameObject.SetActive(false);
            _activeSoundEmitters.Remove(soundEmitter);
        }

        /// <summary>
        /// Called when a SoundEmitter is destroyed (not returned to the pool).
        /// </summary>
        void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            Destroy(soundEmitter.gameObject);
        }

        #endregion
    }
}