using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class PianoKeySound : MonoBehaviour,
    IPointerEnterHandler,
    IPointerDownHandler      
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;  
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void PlayNote()
    {
        audioSource.Stop();
        audioSource.Play();
    }

    public void OnPointerEnter(PointerEventData eventData) => PlayNote();

    public void OnPointerDown(PointerEventData eventData) => PlayNote();
}