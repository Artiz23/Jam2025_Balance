using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

[Serializable]
public class MusicThreshold
{
    public float balanceThreshold;
    public AudioClip musicClip;
    public string description;
}

public class BalanceBarController : MonoBehaviour
{
    public Image greenBar; 
    public Image redBar;   
    public Image indicator; 
    public float maxWidth = 800f; 
    public VolumeColorController volumeColorController;
    public float captureSpeed = 0.5f;
    public TextMeshProUGUI balanceText;

    [SerializeField] private float balanceValue = 0f;
    [SerializeField] private bool loopMusic = true;

    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioSource audioSource3;
    public MusicThreshold[] musicThresholds;
    public float crossFadeDuration = 1f;

    private float lastBalanceValue;
    private AudioSource currentAudioSource;
    private AudioSource[] audioSources;
    private int currentSourceIndex;
    private bool isFading = false;

    void Start()
    {
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource3 = gameObject.AddComponent<AudioSource>();
        
        audioSource1.loop = loopMusic;
        audioSource2.loop = loopMusic;
        audioSource3.loop = loopMusic;
        
        audioSources = new AudioSource[] { audioSource1, audioSource2, audioSource3 };
        currentSourceIndex = 0;
        currentAudioSource = audioSources[currentSourceIndex];
        lastBalanceValue = balanceValue;
        
        balanceValue = Mathf.Clamp(balanceValue, -1f, 1f);
        
        UpdateBar(balanceValue);
        PlayMusicBasedOnBalance();
        
        Debug.Log($"Start: balanceValue = {balanceValue}");
    }

    public void UpdateBar(float value)
    {
        balanceValue = Mathf.Clamp(value, -1f, 1f);

        float greenWidth = maxWidth * (1f + balanceValue) / 2f; 
        float redWidth = maxWidth * (1f - balanceValue) / 2f;   

        greenBar.rectTransform.sizeDelta = new Vector2(greenWidth, greenBar.rectTransform.sizeDelta.y);
        redBar.rectTransform.sizeDelta = new Vector2(redWidth, redBar.rectTransform.sizeDelta.y);

        float indicatorPos = maxWidth * balanceValue / 2f;
        indicator.rectTransform.anchoredPosition = new Vector2(indicatorPos, 0f);

        if (volumeColorController != null)
        {
            volumeColorController.UpdateColor(balanceValue);
        }

        if (balanceText != null)
        {
            balanceText.text = $"Balance: {balanceValue:F2}";
        }

        if (Mathf.Abs(balanceValue - lastBalanceValue) > 0.01f && !isFading)
        {
            PlayMusicBasedOnBalance();
            lastBalanceValue = balanceValue;
        }
    }

    private void PlayMusicBasedOnBalance()
    {
        AudioClip newClip = GetMusicClipForBalance(balanceValue);

        if (newClip == null)
        {
            Debug.LogWarning("No music clip found for balance: " + balanceValue);
            return;
        }

        Debug.Log($"Playing clip: {newClip.name} for balance: {balanceValue}");

        if (currentAudioSource.clip != newClip)
        {
            StartCoroutine(CrossFadeMusic(newClip));
        }
        else if (!currentAudioSource.isPlaying)
        {
            currentAudioSource.clip = newClip;
            currentAudioSource.Play();
        }
    }

    private AudioClip GetMusicClipForBalance(float balance)
    {
        if (musicThresholds == null || musicThresholds.Length == 0)
        {
            Debug.LogError("MusicThresholds array is empty or null!");
            return null;
        }

        foreach (var threshold in musicThresholds)
        {
            if (balance <= threshold.balanceThreshold)
            {
                return threshold.musicClip;
            }
        }
        return musicThresholds[musicThresholds.Length - 1].musicClip;
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        isFading = true;

        int nextSourceIndex = (currentSourceIndex + 1) % audioSources.Length;
        AudioSource nextAudioSource = audioSources[nextSourceIndex];

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (i != currentSourceIndex && audioSources[i].isPlaying)
            {
                audioSources[i].Stop();
                audioSources[i].volume = 0f;
            }
        }

        nextAudioSource.clip = newClip;
        nextAudioSource.volume = 0f;
        nextAudioSource.Play();

        float timer = 0f;
        float startVolumeCurrent = currentAudioSource.volume;
        float startVolumeNext = nextAudioSource.volume;

        while (timer < crossFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossFadeDuration;
            
            currentAudioSource.volume = Mathf.Lerp(startVolumeCurrent, 0f, t);
            nextAudioSource.volume = Mathf.Lerp(startVolumeNext, 1f, t);
            
            yield return null;
        }

        currentAudioSource.Stop();
        currentAudioSource.volume = 1f;
        
        currentSourceIndex = nextSourceIndex;
        currentAudioSource = nextAudioSource;

        isFading = false;
    }

    public float GetBalanceValue()
    {
        return balanceValue;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            UpdateBar(balanceValue - Time.deltaTime * captureSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            UpdateBar(balanceValue + Time.deltaTime * captureSpeed);
        }
    }
}