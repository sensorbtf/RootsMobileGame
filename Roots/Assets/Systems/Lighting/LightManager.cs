using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class LightManager : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private Light2D _globalLight;
    [SerializeField] private float _baseIntensity;
    
    [SerializeField] private float _maxIntensity;
    [SerializeField] private float _minIntensity;    
    [SerializeField] private float _maxFlashDuration;
    [SerializeField] private float _minFlashDuration;
    [SerializeField] private float _stormDuration;
    [SerializeField] private float _maxInterval;
    [SerializeField] private float _minInterval;

    public void MakeStormEffect()
    {
        StartCoroutine(StormCoroutine());
    }

    private IEnumerator StormCoroutine()
    {
        var startTime = Time.time;

        while (Time.time - startTime < _stormDuration)
        {
            _audioManager.PlayThunderstormSoundEffect();
            _globalLight.intensity = Random.Range(_minIntensity, _maxIntensity);
            var flashingDuration = Random.Range(_minFlashDuration, _maxFlashDuration);

            yield return new WaitForSeconds(flashingDuration);

            _globalLight.intensity = _baseIntensity;
            
            var interval = Random.Range(_minInterval, _maxInterval);
            yield return new WaitForSeconds(interval);
        }
        
        _globalLight.intensity = _baseIntensity;
    }
}
