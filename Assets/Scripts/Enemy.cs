using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;

    public int maxHealth;
    public int curHealth;
    public int score;
    public GameManager manager;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;
    public BoxCollider boxCollider;

    public AudioSource deadSound;
    public AudioSource thrustAttackSound;
    public AudioSource rushAttackSound;
    public AudioSource missileAttackSound;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>(); // Material은 다음과 같이 가져와야 함
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();

        if (enemyType != Type.D)
            Invoke("ChaseStart", 2f);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position); // SetDestination : 도착할 목표 위치 지정 함수
            nav.isStopped = !isChase;
        }
    }

    void FreezeVelocity() // 물리 충돌 방지
    {
        if (isChase || enemyType == Type.D)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void Targeting()
    {
        if (!isChase)
            return;

        float targetRadius = 0f;
        float targetRange = 0f;

        switch(enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
            case Type.B:
                targetRadius = 1f;
                targetRange = 20f;
                break;
            case Type.C:
                targetRadius = 0.5f;
                targetRange = 25f;
                break;
        }

        RaycastHit[] rayHits =
            Physics.SphereCastAll(transform.position,
                               targetRadius,
                               transform.forward,
                               targetRange,
                               LayerMask.GetMask("Player"));

        if(rayHits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch(enemyType)
        {
            case Type.A:
                if (isDead)
                    break;
                yield return new WaitForSeconds(0.7f);
                meleeArea.enabled = true;
                thrustAttackSound.Play();
                yield return new WaitForSeconds(0.5f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(0.65f);
                break;
            case Type.B:
                if (isDead)
                    break;
                yield return new WaitForSeconds(0.1f);

                rigid.AddForce(transform.forward * 300, ForceMode.Impulse);
                rushAttackSound.Play();
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                anim.SetBool("isAttack", false);
                anim.SetBool("isWalk", false);

                yield return new WaitForSeconds(3f);
                anim.SetBool("isWalk", true);
                break;
            case Type.C:
                if (isDead)
                    break;
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                missileAttackSound.Play();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity(); 
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;

        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if(curHealth > 0)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else 
        {
            if (!isDead)
            {

                foreach (MeshRenderer mesh in meshs)
                    mesh.material.color = Color.gray;

                gameObject.layer = 12;
                isDead = true;
                isChase = false;
                nav.enabled = false;
                anim.SetTrigger("doDie");
                deadSound.Play();
                Player player = target.GetComponent<Player>();
                player.score += score;
                int ranCoin = Random.Range(0, 3);
                Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

                switch (enemyType)
                {
                    case Type.A:
                        manager.enemyCntA--;
                        break;
                    case Type.B:
                        manager.enemyCntB--;
                        break;
                    case Type.C:
                        manager.enemyCntC--;
                        break;
                    case Type.D:
                        manager.enemyCntD--;
                        break;
                }

                if (isGrenade)
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up * 3;

                    rigid.freezeRotation = false;
                    rigid.AddForce(reactVec * 50, ForceMode.Impulse);
                    rigid.AddTorque(reactVec * 150, ForceMode.Impulse);
                }
                else
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up;

                    rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                }

                Destroy(gameObject, 3.5f);
            }
        }
    }
}
