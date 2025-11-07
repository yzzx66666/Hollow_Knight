using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(speed * Time.deltaTime, 0, 0, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        //TODO
    }
}
