using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class Weapon : MonoBehaviourPunCallbacks
{
    #region Variables
    public GunScriptable[] loadOut;
    private int currentIndex;
    public Transform weaponParent;
    private GameObject currentWeapon;
    public GameObject effect;
    public GameObject wallEffect;
    public GameObject smokeBomb;
    public float recoil;
    private float currentCooldown = 0f;
    public bool canShoot;
    public Transform shootEffect;
    public bool full_Auto;
    private bool isReloading;
    private bool canReload;

    private Image hitmarkerImage;
    private float hitmarkerWait;
    public AudioClip hitmarkerSound;
    public AudioSource sfx;
    public AudioSource shootSound;
    #endregion

    #region CallBacks

    void Start()
    {
        
        canReload = true;
        foreach (GunScriptable a in loadOut) a.Initialize();
        hitmarkerImage = GameObject.Find("HUD/HitMarker/Hit").GetComponent<Image>();
        hitmarkerImage.color = new Color(1, 1, 1, 0);
        canShoot = true;
        Equip(0);

        if (!photonView.IsMine)
        {
            gameObject.layer = 11;
        }
    }

    // Update is called once per frame
    void Update()
    {
    
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
                full_Auto = false;
                
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                photonView.RPC("Equip", RpcTarget.All, 1);
                full_Auto = true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                photonView.RPC("Equip", RpcTarget.All, 2);
                full_Auto = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                photonView.RPC("Equip", RpcTarget.All, 3);
                full_Auto = false;

            }
            if (currentWeapon != null)
            {
                Aim(Input.GetMouseButton(1));
            }
            if (Input.GetMouseButtonUp(0) && full_Auto)
            {
                shootSound.Stop();
            }
        }
        if (photonView.IsMine)
        { 
           
            if(full_Auto == true) { 
            if (Input.GetMouseButton(0) && currentCooldown <= 0 && canReload)

            {
                if (loadOut[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                else StartCoroutine(Reload(loadOut[currentIndex].reload));

            }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && canReload)
                {
                    if (loadOut[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                    else StartCoroutine(Reload(loadOut[currentIndex].reload));


                }
            }
            if (Input.GetKeyDown(KeyCode.R) && canReload)
            {
                photonView.RPC("ReloadRpc", RpcTarget.All);
            }



            //cooldown
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            //weapon position elasticity
            currentWeapon.transform.localRotation = Quaternion.Lerp(currentWeapon.transform.localRotation, Quaternion.identity, Time.deltaTime * 4f);

            

            }

            if (photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {

                    photonView.RPC("SmokeBombF", RpcTarget.All);

                }

            }



        if (photonView.IsMine)
        {
            if(hitmarkerWait > 0)
            {
                hitmarkerWait -= Time.deltaTime;
            }
            else if(hitmarkerImage.color.a > 0)
            {
                hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, new Color(1, 1, 1, 0), Time.deltaTime * 5f);
            }
        }




     
        }



    #endregion

    #region Methods
    
    IEnumerator Reload(float wait)
    {
        canReload = false;

        isReloading = true;

        if (currentWeapon.GetComponent<Animator>()) 
            currentWeapon.GetComponent<Animator>().Play("MitrailletReload", 0, 0);       
        else       
            currentWeapon.SetActive(false);
        
        yield return new WaitForSeconds(wait);

        loadOut[currentIndex].Reload();
        currentWeapon.SetActive(true);
        isReloading = false;
        canReload = true;

    }




    #endregion

    #region Rpc
    [PunRPC]
    private void ReloadRpc()
    {
        StartCoroutine(Reload(loadOut[currentIndex].reload));
    }


    [PunRPC]
    void Equip(int ind)
    {

        if (currentWeapon != null)
        {
            if (isReloading)
            {
                StopCoroutine("Reload");

            }
            Destroy(currentWeapon);

        }

        currentIndex = ind;
        GameObject newWeapon = Instantiate(loadOut[ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;
        currentWeapon = newWeapon;

    }

    void Aim(bool isAiming)
    {
        Transform anchor = currentWeapon.transform.Find("Anchor");
        Transform state_ads = currentWeapon.transform.Find("States/ADS");
        Transform state_hip = currentWeapon.transform.Find("States/Hip");

        if (isAiming)
        {
            //Aim
            anchor.position = Vector3.Lerp(anchor.position, state_ads.position, Time.deltaTime * loadOut[currentIndex].aimSpeed);
            recoil = loadOut[currentIndex].recoil / 2;
        }
        else
        {
            //hip
            anchor.position = Vector3.Lerp(anchor.position, state_hip.position, Time.deltaTime * loadOut[currentIndex].aimSpeed);
            recoil = loadOut[currentIndex].recoil;
        }

    }

    [PunRPC]
    void Shoot()
    {
      
        


        Transform spawn = transform.Find("Camera/Main Camera");
        Transform shootPos = currentWeapon.transform.Find("Anchor/Ressources/shootPos");

        Transform t_shootEf = Instantiate(shootEffect, shootPos.transform.position, transform.rotation);
        Destroy(t_shootEf.gameObject, 2f);
        //cooldown
        currentCooldown = loadOut[currentIndex].fireRate;
        for (int i = 0; i < Mathf.Max(1, loadOut[currentIndex].pellets); i++)
        {

        
        //bloom 
        Vector3 bloom = spawn.position + spawn.forward * 1000f;
        bloom += Random.Range(-loadOut[currentIndex].bloom, loadOut[currentIndex].bloom) * spawn.up;
        bloom += Random.Range(-loadOut[currentIndex].bloom, loadOut[currentIndex].bloom) * spawn.right;
        bloom -= spawn.position;
        bloom.Normalize();

        //Raycast
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(spawn.position, bloom, out hit, 1000f))
        {
            if (photonView.IsMine)
            {
                if (hit.transform.tag == "Player" && hit.transform.gameObject.layer != 12)
                {
                    hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadOut[currentIndex].Damage);
                    GameObject effectApply = Instantiate(effect, hit.point + Vector3.up * 2, Quaternion.identity);
                    Destroy(effectApply, 2f);
                        //HitMarker
                        hitmarkerImage.color = Color.white;
                        sfx.PlayOneShot(hitmarkerSound);
                        hitmarkerWait = 0.2f;

                    }
                    if (hit.transform.tag == "other")
                    {
                        GameObject hitWall = Instantiate(wallEffect, hit.point, Quaternion.identity);
                        Destroy(hitWall, 2f);

                    }
                   

                }
                
            }
            if (photonView.IsMine)
            {
                photonView.RPC("ShootSound", RpcTarget.All);
            }
    }

        
        //gun fx
        currentWeapon.transform.Rotate(-recoil, 0, 0);

        //cooldown


    }
    [PunRPC]
    public void ShootSound()
    {
        shootSound.clip = loadOut[currentIndex].audioClip;
        shootSound.Play();
    }
    [PunRPC]
    private void TakeDamage(int damage)
    {
        GetComponent<PlayerMovement>().TakeDamage(damage);
    }
   /* public void RefreshAmmo(Text text)
    {
        int t_clip = loadOut[currentIndex].GetClip();
        int t_stache = loadOut[currentIndex].GetStash();

        text.text = t_clip.ToString("D2") + " / " + t_stache.ToString("D2");
    }
    */
    [PunRPC]
    void SmokeBombF()
    {
        GameObject smokeBombD = Instantiate(smokeBomb, transform.position, Quaternion.identity);
        Destroy(smokeBombD, 5f);

    }

  
}


#endregion