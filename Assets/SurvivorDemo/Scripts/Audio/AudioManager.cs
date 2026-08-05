using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 音效管理单例。维护一个 AudioSource 池用于多通道混播，
    /// 从 Resources/SFX/ 加载 AudioClip，通过 PlaySFX(name) 播放。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        /// <summary>AudioSource 池大小</summary>
        private const int PoolSize = 8;

        /// <summary> AudioSource 池</summary>
        private AudioSource[] sfxSources;

        /// <summary>当前轮转索引</summary>
        private int sourceIndex;

        /// <summary>已加载的音效缓存</summary>
        private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSources = new AudioSource[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject obj = new GameObject($"SFX_Source_{i}");
                obj.transform.SetParent(transform);
                sfxSources[i] = obj.AddComponent<AudioSource>();
                sfxSources[i].playOnAwake = false;
                sfxSources[i].spatialBlend = 0f; // 2D
            }

            // 从 Resources/SFX/ 加载所有已知音效
            string[] clipNames = { "hit", "crit_hit", "death", "shoot", "player_hurt" };
            foreach (string name in clipNames)
                LoadClip(name);
        }

        private void LoadClip(string name)
        {
            AudioClip clip = Resources.Load<AudioClip>($"SFX/{name}");
            if (clip != null)
                clips[name] = clip;
        }

        /// <summary>
        /// 播放音效。从池中轮转取一个 AudioSource 播放，支持重叠。
        /// </summary>
        /// <param name="name">音效名（对应 Resources/SFX/ 下的文件名）</param>
        /// <param name="volume">音量 0~1</param>
        /// <param name="pitch">音调倍率，默认 1</param>
        public void PlaySFX(string name, float volume = 1f, float pitch = 1f)
        {
            if (!clips.TryGetValue(name, out AudioClip clip))
                return;

            AudioSource source = sfxSources[sourceIndex];
            sourceIndex = (sourceIndex + 1) % PoolSize;

            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        /// <summary>该音效是否已加载</summary>
        public bool HasClip(string name)
        {
            return clips.ContainsKey(name);
        }
    }
}
