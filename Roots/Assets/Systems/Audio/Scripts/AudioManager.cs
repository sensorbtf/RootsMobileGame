using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _effectSource;
        [SerializeField] private AudioSource _buttonSource;
        [SerializeField] private AudioSource _typingSource;

        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private AudioClip _avaiableButtonClicked;
        [SerializeField] private AudioClip _unavaiableButtonClicked;
        [SerializeField] private AudioClip[] _thunderstormsSounds;

        public void CustomStart()
        {
            _musicSource.clip = _backgroundMusic;
            _musicSource.Play();
        }

        public void PlaySpecificSoundEffect(AudioClip p_audioClip)
        {
            _effectSource.clip = p_audioClip;
            _effectSource.Play();
        }
        
        public void CreateNewAudioSource(AudioClip p_audioClip)
        {
            StartCoroutine(GenerateNewAudioSource(p_audioClip));
        }

        public void PlayButtonSoundEffect(bool p_isInteractable)
        {
            _buttonSource.clip = p_isInteractable ? _avaiableButtonClicked : _unavaiableButtonClicked;
            _buttonSource.Play();
        }
        
        public void PlayThunderstormSoundEffect()
        {
            StartCoroutine(GenerateNewAudioSource(_thunderstormsSounds[Random.Range(0, _thunderstormsSounds.Length)]));
        }

        private IEnumerator GenerateNewAudioSource(AudioClip p_clip)
        {
            var newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = p_clip;
            newSource.Play();

            yield return new WaitForSeconds(newSource.clip.length);
            
            Destroy(newSource);
        }

        public void MuteMusic(bool p_isOn)
        {
            _musicSource.mute = !p_isOn;
        }

        public void MuteEffects(bool p_isOn)
        {
            _effectSource.mute = !p_isOn;
            _buttonSource.mute = !p_isOn;
        }

        public void TryToPlayWritingEffect(AudioClip p_audioClip)
        {
            if (_typingSource.isPlaying)
                return;

            _typingSource.clip = p_audioClip;
            _typingSource.Play();
        }

        public void MuteWritingEffect()
        {
            _typingSource.Stop();
        }
    }
}