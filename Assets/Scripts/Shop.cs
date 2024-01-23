using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;

    public AudioSource buySound;
    public AudioSource deniedSound;

    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public string[] talkData;
    public Text talkText;

    Player enterPlayer;

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero; // 화면 정가운데로 오도록 설정
    }
    
    public void Exit()
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000; // 화면 바깥으로 가도록 설정
    }

    public void Buy(int index)
    {
        int price = itemPrice[index];

        if(price > enterPlayer.coin)
        {
            StopCoroutine(NoPriceTalk());
            StartCoroutine(NoPriceTalk());
            return;
        }

        enterPlayer.coin -= price;
        buySound.Play();

        StopCoroutine(BuyTalk());
        StartCoroutine(BuyTalk());

        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                            + Vector3.forward * Random.Range(-3, 3);
        Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);
    }

    IEnumerator NoPriceTalk()
    {
        deniedSound.Play();
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2f);
        talkText.text = talkData[0];
    }

    IEnumerator BuyTalk()
    {
        talkText.text = talkData[2];
        yield return new WaitForSeconds(2f);
        talkText.text = talkData[0];
    }
}
