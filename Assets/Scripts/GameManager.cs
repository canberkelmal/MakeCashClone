using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cardObj;
    public GameObject coinObj;
    public Transform pipes;
    public Image heatBarFill;
    public float heatSensivity;
    public float addSpeedAmount = 0.1f;

    [NonSerialized]
    public bool isMining = false;

    private int pipeCount = 1;
    // Update is called once per frame
    void Update()
    {


        InputController();
    }

    private void InputController()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
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
    }

    private void StartMining()
    {
        CancelInvoke("DecreaseHeatBar");
        InvokeRepeating("IncreaseHeatBar", 0, Time.deltaTime);
        isMining = true;
        cardObj.GetComponent<CardController>().SetFanSpeed(true);
    }

    private void StopMining()
    {
        CancelInvoke("IncreaseHeatBar");
        InvokeRepeating("DecreaseHeatBar", 0, Time.deltaTime);
        isMining = false;
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
