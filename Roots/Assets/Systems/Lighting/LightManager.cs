using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
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

    [SerializeField] private Color _morningColor;
    [SerializeField] private Color _dayColor;
    [SerializeField] private Color _eveningColor;
    
    private float _halfTimeOfDay;
    private float _oneDayTimerDurationInSeconds;
    
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

    public void UpdateLighting(float p_time)
    {
        if (p_time > _halfTimeOfDay) 
        {
            _globalLight.color = Color.Lerp(_morningColor, _dayColor, (3600f - p_time) / 1800f);
        }
        else
        {
            _globalLight.color = Color.Lerp(_dayColor, _eveningColor, (1800f - p_time) / 1800f);
        }
    }

    public void SetMorningColorInstantly()
    {

        _globalLight.color = _morningColor;
    }
    
    public void SetEveningColorInstantly()
    {
        _globalLight.color = _eveningColor;
    }

    public void SetTimers(float p_halfTimeOfDay, int p_oneDayTimerDurationInSeconds)
    {
        _halfTimeOfDay = p_halfTimeOfDay;
        _oneDayTimerDurationInSeconds = p_oneDayTimerDurationInSeconds;
    }
}
