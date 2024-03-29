using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private LayerMask m_layerSol;

    [SerializeField]
    private LayerMask m_layerCoco;

    [SerializeField]
    public Rigidbody2D m_rb;

    [SerializeField]
    private GameObject m_cocoLaunch;

    [SerializeField]
    private SpriteRenderer m_sprite;
    
    [SerializeField]
    private Transform m_rightSlot;
    
    [SerializeField]
    private Transform m_stunPicture;
    
    [SerializeField]
    private Transform m_leftSlot;

    [SerializeField]
    private Transform m_currentCoco;

    [SerializeField]
    private Transform m_lookMonster;

    private Transform m_currentTargetMonster;

    [SerializeField]
    public Score  m_score;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private SpawnersManager m_spawnerManager;

    [SerializeField]
    private Monster m_monster;

    [SerializeField]
    private GameManager m_gameManager;

    [SerializeField]
    private List<Transform> m_listPointEatMonster;

    [SerializeField]
    private bool m_isPlayerOne;

    [SerializeField]
    private float m_SpeedMovement;

    [SerializeField]
    private Vector2 m_projectionSpeed;

    [SerializeField]
    private Vector2 m_jumpSpeed;

    [SerializeField]
    private float m_collideGravityValue;
    
    [SerializeField]
    private AudioSource m_stunSound;

    private float m_throwValue;
    private float m_speedMultiplier = 1;

    private Vector3 m_collideGravity;

    private int m_HP = 3;

    private int m_moveHash;
    private int m_idleHash;
    private int m_jumpHash;
    private int m_2degatHash;
    private int m_1degatHash;
    private int m_lancerHash;

    public bool m_pause;
    private bool m_rotateStun;

    private bool m_canJump = true;
    private Coroutine m_coroutineCooldown;

    private Vector2 m_speed = new Vector2(0,0);

    private Coroutine m_stunRoutine;
    private float m_compteur;
    private Coroutine m_coroutineJump;

    private void Awake()
    {
        m_collideGravity.y = m_collideGravityValue;
        m_stunPicture.gameObject.SetActive(false);

        if (m_pause) m_rb.simulated = false;

        m_moveHash = Animator.StringToHash("Moving");
        m_idleHash = Animator.StringToHash("Idle");
        m_jumpHash = Animator.StringToHash("Jump");
        m_2degatHash = Animator.StringToHash("1degat");
        m_1degatHash = Animator.StringToHash("2degat");
        m_lancerHash = Animator.StringToHash("Lancer");

        m_lookMonster.gameObject.SetActive(false);
}

    private void Update()
    {

        if (m_pause) return;

        //m_speed.x = Input.GetAxisRaw("Horizontal");
        m_lookMonster.up = (m_monster.transform.position - transform.position).normalized;

        if (m_rotateStun) m_stunPicture.Rotate(Vector3.forward);


        if (m_isPlayerOne)
        {
            if (Input.GetKey(KeyCode.Q)) { MovePlayer(-1); };
            if (Input.GetKeyUp(KeyCode.Q)) { MovePlayer(0); };
            if (Input.GetKey(KeyCode.D)) { MovePlayer(1); };
            if (Input.GetKeyUp(KeyCode.D)) { MovePlayer(0); };

            if (Input.GetKeyDown(KeyCode.Z)) { JumpPlayer(); };

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(m_coroutineCooldown != null) m_lookMonster.gameObject.SetActive(true);
                m_throwValue = Time.time;
                m_coroutineCooldown = StartCoroutine(FeedBackMonsterShot());
            }
            if (Input.GetKeyUp(KeyCode.Space)) { ThrowCoco(); };
            return;
        }

        if (Input.GetKey(KeyCode.RightArrow)) { MovePlayer(1); };
        if (Input.GetKeyUp(KeyCode.RightArrow)) { MovePlayer(0); };
        if (Input.GetKey(KeyCode.LeftArrow)) { MovePlayer(-1); };
        if (Input.GetKeyUp(KeyCode.LeftArrow)) { MovePlayer(0); };

        if (Input.GetKeyDown(KeyCode.UpArrow)) { JumpPlayer(); };

        if (Input.GetKeyDown(KeyCode.RightControl)) 
        {

            if (m_coroutineCooldown != null) m_lookMonster.gameObject.SetActive(true);
            m_throwValue = Time.time;
            m_coroutineCooldown = StartCoroutine(FeedBackMonsterShot());
        }
        if (Input.GetKeyUp(KeyCode.RightControl)) { ThrowCoco(); };

        
    }

    private void ThrowCoco()
    {
        
        if (m_currentCoco == null) return;

        if(m_coroutineCooldown != null)StopCoroutine(m_coroutineCooldown);

        float timeValue = Time.time - m_throwValue;
        
        m_animator.SetTrigger(m_lancerHash);
        
        if (timeValue > 0.5f)
        {
            m_lookMonster.gameObject.SetActive(false);
            ProjectForMonster();
            m_throwValue = 0;
            return;
        }
        m_lookMonster.gameObject.SetActive(false);
        Debug.Log("Donner a manger au monstre" + timeValue);
        m_spawnerManager.GetComponent<SpawnersManager>().fruitsC -= 1;
        ProjectForPlayer();

        m_throwValue = 0;
    }

    IEnumerator FeedBackMonsterShot()
    {
        yield return new WaitForSeconds(0.01f);

        if (m_currentCoco != null && m_currentCoco.transform.localScale.x < 0.2f)
        {
            m_currentCoco.transform.localScale += Vector3.one * Time.deltaTime;
            StartCoroutine(FeedBackMonsterShot());
        }
    }

    private void ProjectForMonster()
    {

        List<Transform> m_currentTargetMonsterList = new List<Transform>();
        Transform transformCoco = m_listPointEatMonster[Random.Range(0, m_listPointEatMonster.Count - 1)];
        
        m_currentCoco.parent = null;

        Vector3 dirCoco = (transformCoco.position - m_currentCoco.position).normalized;
        
        GameObject go = Instantiate(m_cocoLaunch);
        go.transform.position = m_currentCoco.position;

        m_currentCoco.gameObject.SetActive(false);
        m_currentCoco = null;

        Rigidbody2D rbCoco = go.GetComponent<Rigidbody2D>();

        rbCoco.simulated = true;

        rbCoco.gravityScale = 0;

        rbCoco.AddRelativeForce(dirCoco * 100);

        if (m_monster.PasDeBouche.activeSelf == true)
        {
            m_score.RemovePoint(100);
            return;
        }
        m_score.AddScore();

    }

    private void ProjectForPlayer()
    {
        GameObject go = Instantiate(m_cocoLaunch);
        go.transform.position = m_currentCoco.position;
        go.layer = gameObject.layer;
        m_currentCoco.gameObject.SetActive(false);
        m_currentCoco = null;

        go.GetComponent<Coco>().SetLayerThrowPlayer(gameObject.layer, transform);

        Rigidbody2D rbCoco = go.GetComponent<Rigidbody2D>();
        rbCoco.simulated = true;
        rbCoco.AddRelativeForce(m_projectionSpeed);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        
        if((m_layerCoco.value & ( 1 << other.transform.gameObject.layer )) > 0 && m_currentCoco == null)
        {
            m_currentCoco = other.transform.GetComponent<GetCoco>().m_coco;
            m_currentCoco.parent = null;

            m_currentCoco.transform.GetComponent<CircleCollider2D>().enabled = false;

            m_currentCoco.transform.SetParent(transform);

            if (!m_sprite.flipX)
            {
                m_currentCoco.transform.position = m_leftSlot.position;
            }

            m_currentCoco.transform.position = m_rightSlot.position;
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_canJump = true;
        m_animator.ResetTrigger(m_jumpHash);
    }

    private void FixedUpdate()
    {
        m_rb.AddForce(m_speed * Time.fixedDeltaTime, ForceMode2D.Impulse);
    }

    public void LaunchStun()
    {
        if (m_stunRoutine != null)
        {
            m_rotateStun = false;
            m_stunPicture.gameObject.SetActive(false);
            m_speedMultiplier = 1;
            m_jumpSpeed.y *= 2;
            StopCoroutine(m_stunRoutine);
        }
        m_stunRoutine = StartCoroutine(Stun());
    }

    public IEnumerator Stun()
    {
        m_stunSound.Play();
        m_rotateStun = true;
        m_stunPicture.gameObject.SetActive(true);
        m_speedMultiplier = 0.5f;
        m_jumpSpeed.y /= 2;
        yield return new WaitForSeconds(3f);
        m_stunRoutine = null;
        m_rotateStun = false;
        m_stunPicture.gameObject.SetActive(false);
        m_speedMultiplier = 1;
        m_jumpSpeed.y *= 2;
    }

    private void MovePlayer(float p_dir)
    {
        m_speed.x = p_dir * m_SpeedMovement * m_speedMultiplier;

        m_animator.SetTrigger(m_idleHash);

        if (p_dir == 0)
        {

            m_animator.ResetTrigger(m_moveHash);
            m_animator.SetTrigger(m_idleHash);
            return;
        }

        m_animator.ResetTrigger(m_idleHash);
        m_animator.SetTrigger(m_moveHash);

        m_projectionSpeed.x *= p_dir;

        if (p_dir > 0)
        {
            m_projectionSpeed.x = Mathf.Abs(m_projectionSpeed.x);

            m_sprite.flipX = true;
            SetPosCoco(m_rightSlot);
            return;
        }

        m_sprite.flipX = false;
        SetPosCoco(m_leftSlot);

        if (m_projectionSpeed.x < 0) return;
        m_projectionSpeed.x *= p_dir;
    }

    private void SetPosCoco(Transform p_trans)
    {
        if (m_currentCoco == null) return;
        m_currentCoco.position = p_trans.position;
    }

    public void GetDamage(int p_less)
    {
        m_HP -= p_less;

        if(m_HP == 1) m_animator.SetTrigger(m_1degatHash);
        if(m_HP == 2) m_animator.SetTrigger(m_2degatHash);

        if (m_HP <= 0)
        {
            Debug.Log("End Game");
            DisplayWinScreen();
        }
    }

    private void DisplayWinScreen()
    {
        m_pause = true;
        m_monster.canEat = false;

        m_gameManager.Display(1);
        //Display
    }

    private void JumpPlayer()
    {
        if (m_coroutineJump == null) m_coroutineJump = StartCoroutine(m_jumpCoroutine());
        
    }

    private void Jump()
    {
        m_animator.SetTrigger(m_jumpHash);
        m_canJump = false;
        m_rb.velocity = m_jumpSpeed;
    }

    IEnumerator m_jumpCoroutine()
    {
        m_compteur = 0;
        while(m_compteur < 0.2f && m_canJump == false)
        {
            m_compteur += Time.deltaTime;
            yield return null;
        }
        m_coroutineJump = null;
        if (m_canJump) Jump();
    }
}