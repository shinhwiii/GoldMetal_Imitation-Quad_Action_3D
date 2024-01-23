using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public AudioSource attackSound;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴을 정지하는 함수
            attackSound.Play();
            StartCoroutine("Swing"); // 코루틴을 실행할 때 StartCoroutine을 사용함
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            attackSound.Play();
            StartCoroutine("Shot"); 
        }
    }

    IEnumerator Swing() // IEnumerator : 열거형 함수 클래스 (yield가 1개 이상 필요함)
    {   // yield 키워드를 여러 개 사용하여 시간 차 로직 작성 가능 (yield : 결과를 전달하는 키워드)

        // 1
        yield return new WaitForSeconds(0.1f); // 0.1초 대기
        meleeArea.enabled = true; // 콜라이더 활성화
        trailEffect.enabled = true; // 잔상 이펙트 활성화

        // 2
        yield return new WaitForSeconds(0.3f); // 0.3초 대기
        meleeArea.enabled = false;

        // 3
        yield return new WaitForSeconds(0.3f); // 0.3초 대기
        trailEffect.enabled = false;

    }

    // 일반 함수 : Use() 메인 루틴 -> Swing() 서브 루틴 -> Use() 메인 루틴
    // 코루틴 함수 : Use() 메인 루틴 + Swing() 코루틴 (Co-Op)

    IEnumerator Shot()
    {
        // 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;

        yield return null; // 1 프레임 대기

        // 탄피 배출
        GameObject instantBulletCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody bulletCaseRigid = instantBulletCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        bulletCaseRigid.AddForce(caseVec, ForceMode.Impulse);
        bulletCaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse); // AddTorque : 회전을 줌
    }
}
