using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace  MH.Core.Sound
{
    public class SoundExam : MonoBehaviour
    {
        public SoundManager soundManager;
        public SoundData soundData;

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                for (int i=0; i<20; i++)
                {
                    soundManager.CreateSoundBuilder().WithPosition(transform.position).Play(soundData);
                }
            
            }
        }
    }
}

