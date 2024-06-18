using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public GameObject PlayerToTp;
    public GameObject PlayerSpawn;

    [SerializeField] TextMeshProUGUI TpTimerText;
    [SerializeField] int tpTimer;
    float tpCount;

    Coroutine coroutine;


    void Start()
    {
        TpTimerText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            tpCount = tpTimer + 1;

            TpTimerText.gameObject.SetActive(true);
            coroutine = StartCoroutine(Teleport());
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tpCount -= Time.deltaTime;
            TpTimerText.SetText(Mathf.FloorToInt(tpCount).ToString());
        }
    }

    void OnTriggerExit(Collider other)
    {
        TpTimerText.gameObject.SetActive(false);
        StopCoroutine(coroutine);
    }

    IEnumerator Teleport()
    {
        yield return new WaitForSeconds(tpTimer);
        PlayerToTp.gameObject.transform.position = PlayerSpawn.transform.position;
    }
}
