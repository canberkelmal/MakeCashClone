using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    private GameObject coinObj;
    private GameManager gameManager;
    private bool mining = false;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        coinObj = gameManager.coinObj;
    }

    private void Update()
    {
        if(gameManager.isMining && !mining)
        {
            mining = true;
            GetComponent<Animator>().SetTrigger("SlideCoin");
        }
        else if (!gameManager.isMining && mining)
        {
            mining = false;
            GetComponent<Animator>().SetTrigger("StopCoin");
        }
    }

    public void InstantiateCoin()
    {
        Instantiate(coinObj, transform.Find("Mouth").position, Quaternion.Euler(90, 0, 0));
    }
    public void IncreseSpeed(float amount)
    {
        GetComponent<Animator>().speed += amount;
    }
}
