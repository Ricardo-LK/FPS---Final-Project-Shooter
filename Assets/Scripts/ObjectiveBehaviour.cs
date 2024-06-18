using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveBehaviour : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float amplitude = 0.5f;
    [SerializeField] float frequency = 1f;
    [SerializeField] float degreesPerSecond = 15.0f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    void Start()
    {
        // store the starting position and rotation
        posOffset = transform.position;
    }

    //n fui eu q fiz
    void Update()
    {
        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

        // float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;
    }

    //esse foi
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene(1);
        }
    }

}
