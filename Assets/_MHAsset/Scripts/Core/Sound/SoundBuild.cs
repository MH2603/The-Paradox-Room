using UnityEngine;

namespace  MH.Core.Sound
{
    /// <summary>
    /// The SoundBuilder class follows the Builder design pattern to construct and play sound emitters with optional configurations.
    /// It provides a fluent API to set sound properties before playing them.
    /// </summary>
    public class SoundBuilder {
        // Reference to the soundManager that controls sound playback and pooling.
        readonly SoundManager soundManager;
        
        // Position where the sound will be played.
        Vector3 position = Vector3.zero;
        
        // Flag to determine if the sound should have a random pitch applied.
        bool randomPitch;

        /// <summary>
        /// Constructor to initialize the SoundBuilder with a soundManager reference.
        /// </summary>
        /// <param name="soundManager">Reference to the soundManager instance.</param>
        public SoundBuilder(SoundManager soundManager) {
            this.soundManager = soundManager;
        }

        /// <summary>
        /// Sets the position where the sound will be played.
        /// </summary>
        /// <param name="position">World position for the sound emitter.</param>
        /// <returns>Returns the builder instance for method chaining.</returns>
        public SoundBuilder WithPosition(Vector3 position) {
            this.position = position;
            return this;
        }

        /// <summary>
        /// Enables random pitch modification for the sound emitter.
        /// </summary>
        /// <returns>Returns the builder instance for method chaining.</returns>
        public SoundBuilder WithRandomPitch() {
            this.randomPitch = true;
            return this;
        }

        /// <summary>
        /// Plays the specified sound data with the configured properties.
        /// </summary>
        /// <param name="soundData">SoundData containing audio clip and playback settings.</param>
        public SoundEmitter Play(SoundData soundData) {
            if (soundData == null) {
                Debug.LogError("SoundData is null");
                return null;
            }
            
            // Check if the sound can be played based on soundManager's rules.
            if (!soundManager.CanPlaySound(soundData)) return null;
            
            // Get an available sound emitter from the pool.
            SoundEmitter soundEmitter = soundManager.Get();
            
            // Initialize the emitter with the provided sound data.
            soundEmitter.Initialize(soundData);
            
            // Set emitter position and parent to the sound manager's transform for better hierarchy management.
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = soundManager.transform;
            
            // Apply random pitch if the option was enabled.
            if (randomPitch) {
                soundEmitter.WithRandomPitch();
            }

            // Add the emitter to the frequent sound list if the sound is marked as frequent.
            if (soundData.frequentSound) {
                soundEmitter.Node = soundManager.SoundEmitterCounter.AddLast(soundEmitter);
            }
            
            // Add the emitter to the frequent sound list
            // soundEmitter.Node = soundManager.SoundEmitterCounter.AddLast(soundEmitter);
            
            // Play the sound emitter.
            soundEmitter.Play();

            return soundEmitter;
        }
    }
}


