using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MH.Core.Sound
{
    [Serializable]
    public class SoundData 
    {
        // Audio clip chứa âm thanh cần phát
        public AudioClip clip;

        // AudioMixerGroup để trộn âm thanh vào các group như SFX, BGM,...
        public AudioMixerGroup mixerGroup;

        // Có nên phát lặp lại âm thanh hay không (Loop)
        public bool loop;

        // Có nên tự động phát âm thanh khi AudioSource được enable không
        public bool playOnAwake;

        // Xác định âm thanh này có được phát thường xuyên hay không (dùng để tối ưu giới hạn âm thanh)
        public bool frequentSound;

        // Âm thanh này có bị mute mặc định hay không
        public bool mute;

        // Bỏ qua các hiệu ứng trong AudioSource (Reverb, Filter,...)
        [HideInInspector] 
        public bool bypassEffects;

        // Bỏ qua các hiệu ứng của AudioListener (Volume, Echo,...)
        [HideInInspector] 
        public bool bypassListenerEffects;

        // Bỏ qua hiệu ứng Reverb Zone trong môi trường 3D
        [HideInInspector] 
        public bool bypassReverbZones;

        // Độ ưu tiên của âm thanh (0 là cao nhất, 256 là thấp nhất)
        // Âm thanh có Priority thấp hơn sẽ được phát trước
        public int priority = 128;

        // Âm lượng của âm thanh (0 - 1)
        [Range(0, 1)]
        public float volume = 1f;

        // Cao độ (Pitch) của âm thanh (0.1 -> 3f)
        // Thường dùng để tạo các hiệu ứng tốc độ nhanh/chậm cho âm thanh
        [Range(0.1f, 3f)]
        public float pitch = 1f;

        // Điều chỉnh âm thanh nổi (Stereo Pan) (-1 là trái, 1 là phải, 0 là trung tâm)
        [HideInInspector] 
        public float panStereo;

        [HideInInspector] 
        // Blend giữa âm thanh 2D và 3D (0 = 2D, 1 = 3D)
        public float spatialBlend;

        // Mức độ Reverb Zone ảnh hưởng đến âm thanh (0 là không ảnh hưởng, 1 là tối đa)
        [HideInInspector] 
        public float reverbZoneMix = 1f;

        // Hiệu ứng Doppler (ảnh hưởng bởi tốc độ di chuyển của đối tượng)
        // Giá trị càng cao thì hiệu ứng Doppler càng rõ
        [HideInInspector] 
        public float dopplerLevel = 1f;

        // Góc lan rộng của âm thanh 3D (0 là không lan, 360 là lan toàn bộ)
        [HideInInspector] 
        public float spread;

        // Khoảng cách tối thiểu âm thanh phát to nhất
        public float minDistance = 1f;

        // Khoảng cách tối đa âm thanh sẽ ngừng phát
        public float maxDistance = 500f;

        [HideInInspector] 
        // Bỏ qua việc điều chỉnh âm lượng từ AudioListener
        public bool ignoreListenerVolume;

        // Bỏ qua việc tạm dừng từ AudioListener (Pause)
        [HideInInspector] 
        public bool ignoreListenerPause;

        // Chế độ suy giảm âm thanh theo khoảng cách (Linear hoặc Logarithmic)
        [HideInInspector] 
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }
}


