using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnerBehaviour : MonoBehaviour
{
    //Collider
    public PlayerMovement playerMovement;


    [SerializeField] float spawnRate;
    [SerializeField] List<GameObject> Enemy;
    [SerializeField] List<GameObject> SealedDoors;

    //enemy spawn
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] int startingEnemys;
    [SerializeField] int enemysToKill;

    //boll
    [SerializeField] bool colliding;

    void Awake()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    void Start()
    {
        colliding = false;

        foreach (GameObject door in SealedDoors)
        {
            door.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject door in SealedDoors)
            {
                door.SetActive(true);
            }

            colliding = true;
            for (int i = 0; i < startingEnemys; i++)
                Spawn();


            StartCoroutine(SpawnCooldown());

        }
    }

    void Update()
    {
        if (enemysToKill <= 0)
        {
            foreach (GameObject door in SealedDoors)
            {
                door.SetActive(false);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            colliding = false;
        }
    }

    void Spawn()
    {
        int randomPos = Random.Range(0, spawnPoints.Count);
        int enemyType = Random.Range(0, 3);
        Debug.Log(enemyType);

        Instantiate(Enemy[enemyType], spawnPoints[randomPos].position, Quaternion.identity);
    }

    IEnumerator SpawnCooldown()
    {
        Debug.Log("ieee");
        while (playerMovement.PlayerAlive && colliding && enemysToKill > 0)
        {
            yield return new WaitForSeconds(spawnRate);
            enemysToKill -= 1;
            Spawn();
        }
    }
}
