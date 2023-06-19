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
        if(isCurved)
        {
            gameManager.moneyTx.transform.parent.GetComponent<Animator>().SetTrigger("Trig");
        }
        gameManager.IncreaseMoneyCount(multiplier);
        GameObject risingTx = Instantiate(incomeTxAsset, incomeTx.transform.position, Quaternion.identity, pipeCanvas.transform);
        risingTx.GetComponent<Text>().text = "$" + ((int)(multiplier * gameManager.moneyPerCoin)).ToString();
        Destroy(risingTx, 0.6f);
    }
    public void IncreseSpeed(float amount)
    {
        GetComponent<Animator>().speed = amount;
    }

    public void SetMultiplier()
    {
        multiplier *= 4;
        multiplierTx.text = "X" + multiplier.ToString(); 
        switch (multiplier)
        {
            case 1:
                transform.GetChild(0).GetComponent<Renderer>().material = gameManager.pipeLevelColors[0];
                transform.GetChild(1).GetComponent<Renderer>().material = gameManager.pipeLevelColors[0];
                break;
            case 4:
                transform.GetChild(0).GetComponent<Renderer>().material = gameManager.pipeLevelColors[1];
                transform.GetChild(1).GetComponent<Renderer>().material = gameManager.pipeLevelColors[1];
                break;
            case 16:
                transform.GetChild(0).GetComponent<Renderer>().material = gameManager.pipeLevelColors[2];
                transform.GetChild(1).GetComponent<Renderer>().material = gameManager.pipeLevelColors[2];
                break;
            case 32:
                transform.GetChild(0).GetComponent<Renderer>().material = gameManager.pipeLevelColors[3];
                transform.GetChild(1).GetComponent<Renderer>().material = gameManager.pipeLevelColors[3];
                break;
        }
    }
}
