using UnityEngine;
using TMPro;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    //Bools 
    bool shooting, readyToShoot, reloading;

    //Reference
    public PlayerMovement playerMovement;
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask IsEnemy, IsEnvironment;

    //Graphics
    public GameObject muzzleFlash, bulletHoleEnemyGraphic, bulletHoleEnvironmentGraphic;

    public TextMeshProUGUI text;

    private void Awake()
    {
        attackPoint = this.gameObject.transform.GetChild(0);
        fpsCam = Camera.main;
        text = GameObject.Find("Canvas").transform.Find("Bullets").GetComponent<TMPro.TextMeshProUGUI>();
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    private void Update()
    {
        if (playerMovement.PlayerAlive)
            MyInput();

        //SetText
        text.SetText(bulletsLeft + " / " + magazineSize);
    }
    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
            Reload();

        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // calculate direction with spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        //RayCast
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, IsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            if (rayHit.collider.CompareTag("Enemy"))
                rayHit.collider.GetComponent<EnemyBehaviour>().TakeDamage(damage);


            //Graphics
            Instantiate(bulletHoleEnemyGraphic, rayHit.point, Quaternion.FromToRotation(Vector3.forward, rayHit.normal));
        }
        else if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, IsEnvironment))
        {
            //Graphics
            Instantiate(bulletHoleEnvironmentGraphic, rayHit.point, Quaternion.FromToRotation(Vector3.forward, rayHit.normal));
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }


        bulletsLeft--;
        bulletsShot--;

        if (!IsInvoking("ResetShot") && !readyToShoot)
            Invoke("ResetShot", timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }
    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}