using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cardObj;
    public GameObject card;
    public GameObject coinObj;
    public GameObject heatWarningUI;
    public Color heatCardColor;
    public Transform pipes;
    public Image heatBarFill;
    public Text moneyTx;
    public float moneyPerCoin = 1f;
    public float heatSensivity;
    public float addSpeedAmount = 0.1f;

    [NonSerialized]
    public bool isMining = false;

    private int pipeCount = 1;
    private float moneyCount = 0f;
    private bool overHeat = false;
    private Color defCardColor;


    private void Awake()
    {
        defCardColor = card.GetComponent<Renderer>().material.color;
        moneyCount = PlayerPrefs.GetFloat("moneyCount", 0);
    }

    private void Start()
    {
        IncreaseMoneyCount(0);
    }
    // Update is called once per frame
    void Update()
    {


        InputController();
    }

    private void InputController()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && !overHeat)
            {
                StartMining();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StopMining();
            }
        }
    }

    public void IncreaseMoneyCount(int multiplier)
    {
        float income = moneyPerCoin * multiplier;
        moneyCount += income;
        PlayerPrefs.SetFloat("moneyCount", moneyCount);
        moneyTx.text = "$" + moneyCount.ToString();
        Debug.Log("Money: " + moneyCount);
    }

    private void StartMining()
    {
        CancelInvoke("DecreaseHeatBar");
        InvokeRepeating("IncreaseHeatBar", 0, Time.fixedDeltaTime);
        isMining = true;
        cardObj.GetComponent<CardController>().SetFanSpeed(true);
    }

    private void StopMining()
    {
        CancelInvoke("IncreaseHeatBar");
        InvokeRepeating("DecreaseHeatBar", 0, Time.fixedDeltaTime);
        isMining = false;
        cardObj.GetComponent<CardController>().SetFanSpeed(false);
    }

    private void IncreaseHeatBar()
    {
        if(heatBarFill.fillAmount < 1)
        {
            heatBarFill.fillAmount += heatSensivity * Time.deltaTime;
        }
        else
        {
            overHeat = true;
            StopMining();
            Debug.Log("overheat fillAmount: " + heatBarFill.fillAmount);
        }

        ChangeCardColor(heatBarFill.fillAmount);
    }
    private void DecreaseHeatBar()
    {
        if (heatBarFill.fillAmount > 0)
        {
            heatBarFill.fillAmount -= 2 * heatSensivity * Time.deltaTime;
        }
        else
        {
            overHeat = false;
            CancelInvoke("DecreaseHeatBar");
            Debug.Log("cooled fillAmount: " + heatBarFill.fillAmount);
        }

        ChangeCardColor(heatBarFill.fillAmount);
    }

    private void ChangeCardColor(float amount)
    {
        card.GetComponent<Renderer>().material.color = Color.Lerp(defCardColor, heatCardColor, amount*1.1f);
        heatWarningUI.SetActive(amount > 0.75f);
    }

    public void IncreaseSpeed(bool sign)
    {
        float add = sign ? addSpeedAmount : -addSpeedAmount;
        for(int i = 0; i < pipes.childCount; i++)
        {
            pipes.GetChild(i).GetComponent<PipeController>().IncreseSpeed(add);
        }
    }

    public void AddPipe()
    {
        if(pipeCount < pipes.childCount)
        {
            pipes.GetChild(pipeCount).gameObject.SetActive(true);
            pipeCount++;
        }
        else
        {
            for(int i = 1; i < pipes.childCount; i++)
            {
                pipes.GetChild(i).gameObject.SetActive(false);
            }
            pipeCount = 1;
        }
    }
}
