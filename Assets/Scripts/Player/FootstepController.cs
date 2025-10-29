using System.Collections;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("音效设置")]
    public AudioSource audioSource;
    public AudioClip footstepClip;
    private bool isWalking = false;
    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.clip = footstepClip;
        }
    }

    private void PlayFootstepSound()
    {
        if (audioSource == null || footstepClip == null)
            return;

        audioSource.Play();
    }
    
    private void StopFootstepSound()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
    }
    
    public void SetWalkingState(bool walking)
    {
        bool wasWalking = isWalking;
        isWalking = walking;

        if (wasWalking && !walking)
        {
            //停止走路
            StopFootstepSound();
        }
        else if (!wasWalking && walking)
        {
            //开始走路时播放音效
            PlayFootstepSound();
        }
    }
}