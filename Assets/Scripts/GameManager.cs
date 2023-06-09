using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cardObj;
    public Image heatBarFill;
    public float heatSensivity;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {


        InputController();
    }

    private void InputController()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartMining();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopMining();
        }
    }

    private void StartMining()
    {
        CancelInvoke("DecreaseHeatBar");
        InvokeRepeating("IncreaseHeatBar", 0, Time.deltaTime);
        cardObj.GetComponent<CardController>().SetFanSpeed(true);
    }

    private void StopMining()
    {
        CancelInvoke("IncreaseHeatBar");
        InvokeRepeating("DecreaseHeatBar", 0, Time.deltaTime);
        cardObj.GetComponent<CardController>().SetFanSpeed(false);
    }

    private void IncreaseHeatBar()
    {
        if(heatBarFill.fillAmount <= 100)
        {
            heatBarFill.fillAmount += heatSensivity * Time.deltaTime;
        }
        else
        {
            CancelInvoke("IncreaseHeatBar");
        }
    }
    private void DecreaseHeatBar()
    {
        if (heatBarFill.fillAmount >= 0)
        {
            heatBarFill.fillAmount -= 2 * heatSensivity * Time.deltaTime;
        }
        else
        {
            CancelInvoke("DecreaseHeatBar");
        }
    }
}
