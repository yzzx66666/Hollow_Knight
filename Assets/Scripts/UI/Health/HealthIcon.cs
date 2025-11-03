using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIcon : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float time_interval = 3f;

    private void Start()
    {
        animator = GetComponent<Animator>();

        InvokeRepeating("PlayHealthIconAnim", time_interval, time_interval);
    }

    //每隔一段时间播放一次UI动画
    private void PlayHealthIconAnim()
    {
        animator.SetTrigger("play_anim");
    }

}
