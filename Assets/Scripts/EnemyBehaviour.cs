using UnityEngine;
using Image = UnityEngine.UI.Image;

public class EnemyBehaviour : MonoBehaviour
{

    //Enemy status 
    [SerializeField] public int Health;
    [SerializeField] int enemyDamage;
    [SerializeField] int enemySpeed;
    [SerializeField] float attackCooldown;

    //Enemy type
    [SerializeField] bool flight;
    [SerializeField] bool explosive;

    int lifeSteal;
    float enemyHeight;

    //Reference
    Rigidbody rb;
    GameObject target;
    LayerMask isGround;

    //Bools
    bool grounded;
    bool readyToAttack;

    //Graphics
    [SerializeField] GameObject explosionEffect;
    public Image HealthBar;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        target = GameObject.FindGameObjectWithTag("Player");

        // finds healthbar image
        foreach (var image in GameObject.FindGameObjectWithTag("Canvas").GetComponentsInChildren<Image>(true))
        {
            if (image.name == "Health")
            {
                HealthBar = image;
            }
        };
        enemyHeight = gameObject.transform.localScale.y;
    }

    void Start()
    {
        lifeSteal = Mathf.FloorToInt(Health / 4);
        readyToAttack = true;

        if (flight)
            rb.useGravity = false;

    }

    void Update()
    {
        if (Health <= 0)
        {
            target.GetComponent<PlayerMovement>().PlayerHealth += lifeSteal;
            target.GetComponent<PlayerMovement>().PlayerHealth = Mathf.Clamp(target.GetComponent<PlayerMovement>().PlayerHealth, 0, target.GetComponent<PlayerMovement>().MaxPlayerHealth);

            HealthBar.fillAmount = (float)target.GetComponent<PlayerMovement>().PlayerHealth / target.GetComponent<PlayerMovement>().MaxPlayerHealth;

            Destroy(gameObject);
        }

        // check ground
        grounded = Physics.Raycast(transform.position, Vector3.down, enemyHeight * 0.5f + 0.2f, isGround);

        if (!grounded && !flight)
            rb.AddForce(Vector3.down * enemySpeed * 4f, ForceMode.Force);


        if (Vector3.Distance(transform.position, target.transform.position) > 1.15f)
        {
            MoveAi();
        }
        else
        {
            if (readyToAttack)
                AttackPlayer();
        }
    }

    void MoveAi()
    {
        float movement = enemySpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, movement);
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
    }

    public void AttackPlayer()
    {
        readyToAttack = false;


        if (explosive)
        {
            Invoke("Explode", attackCooldown);
        }
        else
        {
            target.GetComponent<PlayerMovement>().PlayerHealth -= enemyDamage;
            HealthBar.fillAmount = (float)target.GetComponent<PlayerMovement>().PlayerHealth / target.GetComponent<PlayerMovement>().MaxPlayerHealth;
            Invoke("ResetShot", attackCooldown);
        }


    }

    void ResetShot()
    {
        readyToAttack = true;
    }

    void Explode()
    {
        target.GetComponent<PlayerMovement>().PlayerHealth -= enemyDamage;
        HealthBar.fillAmount = (float)target.GetComponent<PlayerMovement>().PlayerHealth / target.GetComponent<PlayerMovement>().MaxPlayerHealth;
        Instantiate(explosionEffect, gameObject.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
