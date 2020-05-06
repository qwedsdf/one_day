using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [SerializeField]
    private List<AudioClip> _clipList = new List<AudioClip>();

    [SerializeField]
    private AudioSource _audioSource;

    public void Play(string clipName) {
        var clip = _clipList.FirstOrDefault(c => c.name == clipName);

        if(clip == null) {
            Debug.LogError($"対象のサウンドが見つかりませんでした。clip名：{clipName}");
            return;
        }

        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
