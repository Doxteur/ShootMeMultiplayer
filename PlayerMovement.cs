using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Variables
    public float speed = 10.0f;
    //Movement
    private float translation;
    private float straffe;
    bool jetpack = false;
    public GameObject myCam;
    public CharacterController chara;
    //Jetpack
    public float jetForce = 15f;
    public float jetWait;
    public float jetRecovery;
    public float max_Fuel;
    private Transform ui_FuelBar;
    public float current_fuel;
    public float curent_recovery;
    private bool canJet;

    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    public float jumpHeight;
    public GameObject[] spawnPoints;
    public int RealHp;
    public int maxHealth;
    public GameObject jumpEffect;
    public TMP_Text txt;
    //public Animator anim;
    [SerializeField] private HealthBar healthBar;
    public Manager manager;
    private Weapon weapon;
    Vector3 velocity;
    bool isGrounded;
    private Transform Ui_HealthBar;
    private Text UI_Ammo;
    private Text Ui_Username;
    public Transform weaponParent;
    [HideInInspector] public ProfileData playerProfile;
    public TextMeshPro playerUsername;
    private float aimAngle;
    private float x, y;
    #endregion

    #region CallBacks
   
    void Start()
    {
        weapon = GetComponent<Weapon>();
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        RealHp = maxHealth;
        current_fuel = max_Fuel;

        // turn off the cursor
        Cursor.lockState = CursorLockMode.Locked;
        chara = GetComponent<CharacterController>();
        if (photonView.IsMine)
        {
            Ui_HealthBar = GameObject.Find("HUD/Health/Background/Bar").transform;

            ui_FuelBar = GameObject.Find("HUD/Fuel/Background/Bar").transform;
            Ui_Username = GameObject.Find("HUD/Name").GetComponent<Text>();
            

            RefreshHealthBar();

            Ui_Username.text = Launcher.myProfile.username;

            photonView.RPC("SyncProfile", RpcTarget.All,Launcher.myProfile.username,Launcher.myProfile.level,Launcher.myProfile.xp);

        }
       

    }
   
    void Update()
    {

        if (!photonView.IsMine)
        {
            RefreshMultiPlayerState();
            return;
        }
        if (photonView.IsMine)
        {
            //Jetpack Input
            if (Input.GetKey(KeyCode.F) && current_fuel > 0)
            {
                chara.Move(Vector3.up * jetForce * Time.deltaTime);
                current_fuel -= Time.deltaTime;
                jetpack = true;

            }
            else
            {
                jetpack = false;
            }
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            gameObject.layer = 12;
            PhotonView photonView = PhotonView.Get(this);

            myCam.SetActive(true);
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

           
             x = Input.GetAxisRaw("Horizontal");
             y = Input.GetAxisRaw("Vertical");

            //Gravity
            if(jetpack == false) { 

            velocity.y += gravity * Time.deltaTime * 2;

            }

            chara.Move(velocity * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = 16f;

            }
            else
            {
                speed = 12f;
            }
            if (Input.GetKeyDown("escape"))
            {
                // turn on the cursor
                Cursor.lockState = CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                velocity.y = Mathf.Sqrt(4f * -2f * gravity);
                curent_recovery = 0f;
            }
            if (isGrounded)
            {
                current_fuel = Mathf.Min(max_Fuel, current_fuel + Time.deltaTime * jetRecovery);
                if (curent_recovery < jetWait)
                {
                    curent_recovery = Mathf.Min(jetWait, curent_recovery + Time.deltaTime);
                }
               
            }
            ui_FuelBar.localScale = new Vector3(current_fuel / max_Fuel, 1, 1);
             
            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(5);
            //Animation
            //anim.SetFloat("xValue", x , 0.1f,Time.deltaTime);
            //anim.SetFloat("yValue", y, 0.1f, Time.deltaTime);

        }

        if (photonView.IsMine) {
            photonView.RPC("Health", RpcTarget.All);
            txt.text = RealHp.ToString();

            if (RealHp <= 0)
        {
            manager.Spawn();
            PhotonNetwork.Destroy(gameObject);
            Debug.Log("Dead !!!");

        }
        }
        

        //Jump
        if (Input.GetKeyDown(KeyCode.E) && isGrounded)
        {
            photonView.RPC("JumpEffectF", RpcTarget.All);
            velocity.y = Mathf.Sqrt(20f * -2f * gravity);
        }


       // weapon.RefreshAmmo(UI_Ammo);
        RefreshHealthBar();
    }
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
         
            Vector3 moveDirection = (transform.right * x + transform.forward * y).normalized;

            chara.Move(moveDirection * speed * Time.fixedDeltaTime);
        }
    }
    #endregion


    #region Methods

    public void RefreshHealthBar()
    {
        float health_ratio = (float)RealHp / (float)maxHealth;
        Ui_HealthBar.localScale = Vector3.Lerp(Ui_HealthBar.localScale,new Vector3(health_ratio, 1, 1), Time.deltaTime * 8f);
    }
    public void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            RealHp -= damage;
            RefreshHealthBar();

            Debug.Log(RealHp);
        }
    }

    void RefreshMultiPlayerState()
    {
        float cacheEuly = weaponParent.localEulerAngles.y;
        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = cacheEuly;

        weaponParent.localEulerAngles = finalRotation;

    }
    #endregion

    #region Enumerator 



    #endregion

    #region PUNRPC

    [PunRPC]
    private void SyncProfile(string p_username, int p_level, int p_xp)
    {

     
        playerProfile = new ProfileData(p_username, p_level, p_xp);
        playerUsername.text = playerProfile.username;
    
    }

    [PunRPC]
    void JumpEffectF() 
    {

        GameObject jumpEffectD = Instantiate(jumpEffect, transform.position, Quaternion.identity);
        Destroy(jumpEffectD, 2f);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo message)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
            

        }
        else
        {
            aimAngle = (int)stream.ReceiveNext() / 100f;
           
        }

    }




    #endregion
}

