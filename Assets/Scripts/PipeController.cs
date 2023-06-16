using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PipeController : MonoBehaviour
{
    public GameObject incomeTxAsset;

    private GameObject pipeCanvas;
    private GameObject coinObj;
    private Transform incomeTx;
    private GameManager gameManager;
    private Text multiplierTx;
    private bool mining = false;

    public int multiplier = 1;
    public bool isCurved;


    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        coinObj = gameManager.coinObj;

        pipeCanvas = transform.Find("PipeCanvas").gameObject;
        multiplierTx = pipeCanvas.transform.Find("MultiplierTx").GetComponent<Text>();
        incomeTx = pipeCanvas.transform.Find("IncomeTx");
    }

    public void TrigOtherPipes()
    {
        gameManager.TrigShortPipes();
    }

    public void InstantiateCoin()
    {
        Instantiate(coinObj, transform.Find("Mouth").position, Quaternion.Euler(90, 0, 0));
        gameManager.IncreaseMoneyCount(multiplier);
        GameObject risingTx = Instantiate(incomeTxAsset, incomeTx.transform.position, Quaternion.identity, pipeCanvas.transform);
        risingTx.GetComponent<Text>().text = "$" + (multiplier * gameManager.moneyPerCoin).ToString();
        Destroy(risingTx, 0.6f);
    }
    public void IncreseSpeed(float amount)
    {
        GetComponent<Animator>().speed += amount;
    }

    public void SetMultiplier()
    {
        multiplier *= 4;
        multiplierTx.text = "X" + multiplier.ToString(); 
    }
}
