using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunSelectionBehaviour : MonoBehaviour
{
    [SerializeField] List<GameObject> GunType;
    [SerializeField] List<GameObject> GunObject;
    [SerializeField] TextMeshProUGUI GunSelection;


    void Awake()
    {
        GunSelection.gameObject.SetActive(false);
        foreach (GameObject gun in GunType)
        {
            gun.SetActive(true);
        }

        foreach (GameObject gun in GunObject)
        {
            gun.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GunSelection.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                string tag = gameObject.tag;

                switch (tag)
                {
                    case "ShotgunPickup":
                        GunObject[0].SetActive(true);

                        GunType[1].SetActive(false);
                        GunType[2].SetActive(false);
                        break;

                    case "MachinegunPickup":
                        GunObject[1].SetActive(true);

                        GunType[0].SetActive(false);
                        GunType[2].SetActive(false);
                        break;

                    case "BurstgunPickup":
                        GunObject[2].SetActive(true);

                        GunType[0].SetActive(false);
                        GunType[1].SetActive(false);
                        break;
                }
            }

        }
    }

    void OnTriggerExit(Collider other)
    {
        GunSelection.gameObject.SetActive(false);
    }
}
