using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cardObj;
    public GameObject Lv1Card, Lv2Card;
    public GameObject coinObj;
    public GameObject heatWarningUI;
    public GameObject pipePrefab;
    public GameObject curvedPipePrefab;
    public GameObject SettingsPanel;
    public Color heatCardColor;
    public Transform pipes;
    public Image heatBarFill;
    public Text moneyTx;
    public GameObject speedButton;
    public GameObject pipeButton;
    public GameObject mergePipeButton;
    public GameObject incomeButton;
    public GameObject buffParticle;
    public float moneyPerCoin = 1f;
    public float heatSensivity;
    public float addSpeedAmount = 0.1f;
    public float speedCost = 0.1f;
    public float incomeMultiplier = 0.1f;
    public float incomeCost = 0.1f;
    public float pipeCost = 50f;
    public float costMultiplier = 1.05f;
    public float pipeCostMultiplier = 1.7f;
    public float mergeAnimSensivity = 1f;
    public int pipeCount = 0;
    public Material[] pipeLevelColors = new Material[4];

    [NonSerialized]
    public bool isMining = false;

    private GameObject[] pipesArray = new GameObject[0];
    private GameObject[] mergeablePipesArray = new GameObject[0];
    public GameObject mergedPipe;
    private bool control = true;
    private bool overHeat = false;
    private bool mergeable = false;
    private bool mergePhase1 = false;
    private bool mergePhase2 = false;
    private bool mergePhase3 = false;
    private bool isCurvedMerged = false;
    private bool areMergedDeleted = false;
    public float moneyCount = 0f;
    private Vector3 curvedLocalPos;
    private int speedLevel = 1;
    private int incomeLevel = 1;
    private int pipeLevel = 1;



    private void Awake()
    {
        //defCardColor = card.GetComponent<Renderer>().material.color;
        moneyCount = PlayerPrefs.GetFloat("moneyCount", 0);
    }

    private void Start()
    {
        IncreaseMoneyCount(0);

        for(int i = 0; i < pipes.childCount; i++)
        {
            AddRemoveToPipesArray(true, pipes.GetChild(i).gameObject);
        }

        curvedLocalPos = pipes.GetChild(0).localPosition;

        SetUIs();
    }
    // Update is called once per frame
    void Update()
    {


        InputController();
    }

    private void InputController()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && control)
        {
            if (Input.GetMouseButtonDown(0) && !overHeat && !isMining)
            {
                StartMining();
            }
            else if (Input.GetMouseButtonUp(0) && !overHeat && isMining)
            {
                StopMining();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    public void IncreaseMoneyCount(int multiplier)
    {
        float income = moneyPerCoin * multiplier;
        moneyCount += income;
        PlayerPrefs.SetFloat("moneyCount", moneyCount);
        moneyTx.text = "$" + ConvertNumberToUIText(moneyCount);
        Debug.Log("Money: " + moneyCount);
        CheckUIs();
    }
    public void DecreaseMoneyCount(float amount)
    {
        moneyCount -= amount;
        PlayerPrefs.SetFloat("moneyCount", moneyCount);
        moneyTx.text = "$" + ConvertNumberToUIText(moneyCount);
        Debug.Log("Money: " + moneyCount);
    }

    private String ConvertNumberToUIText(float number)
    {
        String UITx = ">B";
        float operatedNumber;
        if(number > 1000000000) // Billion
        {
            operatedNumber = (int)(number / 100000000);
            UITx = (operatedNumber / 10).ToString() + "B";
        }
        else if (number > 1000000) // Million
        {
            operatedNumber = (int)(number / 100000);
            UITx = (operatedNumber / 10).ToString() + "M";
        }
        else if (number > 1000) // Thousand
        {
            operatedNumber = (int)(number / 100);
            UITx = (operatedNumber / 10).ToString() + "K";
        }
        else
        {
            operatedNumber = (int)(number * 10);
            UITx = (operatedNumber / 10).ToString();
        }
        return UITx; 
    }

    private void StartMining()
    {
        CancelInvoke("DecreaseHeatBar");
        InvokeRepeating("IncreaseHeatBar", 0, Time.fixedDeltaTime);
        isMining = true;

        pipes.transform.GetChild(0).GetComponent<Animator>().SetTrigger("SlideCoin");

        cardObj.GetComponent<CardController>().SetFanSpeed(true);
    }

    private void StopMining()
    {
        Debug.Log("stopMining");
        CancelInvoke("IncreaseHeatBar");
        InvokeRepeating("DecreaseHeatBar", 0, Time.fixedDeltaTime);
        isMining = false;

        for(int i = 0; i < pipes.transform.childCount; i++)
        {
            pipes.transform.GetChild(i).GetComponent<Animator>().SetTrigger("StopCoin");
            Debug.Log(pipes.transform.GetChild(i) + " stopped");
        }

        cardObj.GetComponent<CardController>().SetFanSpeed(false);
    }

    public void TrigShortPipes()
    {
        for(int i = 1; i<pipes.childCount; i++)
        {
            pipes.transform.GetChild(i).GetComponent<Animator>().SetTrigger("SlideCoin");
        }
    }

    private void IncreaseHeatBar()
    {
        ChangeCardColor(heatBarFill.fillAmount);
        if (heatBarFill.fillAmount < 1)
        {
            heatBarFill.fillAmount += heatSensivity * Time.deltaTime;
        }
        else
        {
            overHeat = true;
            Debug.Log("overheat fillAmount: " + heatBarFill.fillAmount);
            StopMining();
            CancelInvoke("IncreaseHeatBar");
        }

    }
    private void DecreaseHeatBar()
    {
        ChangeCardColor(heatBarFill.fillAmount);
        if (heatBarFill.fillAmount > 0)
        {
            heatBarFill.fillAmount -= 2 * heatSensivity * Time.deltaTime;
        }
        else
        {
            overHeat = false;
            Debug.Log("cooled fillAmount: " + heatBarFill.fillAmount);
            CancelInvoke("DecreaseHeatBar");
        }
    }

    private void ChangeCardColor(float amount)
    {
        cardObj.GetComponent<CardController>().HeatCard(amount);
        heatWarningUI.SetActive(amount > 0.75f);
    }

    public void IncreaseSpeed(bool sign)
    {
        Destroy(Instantiate(buffParticle, cardObj.transform.position - Vector3.forward, Quaternion.identity), 1.5f);
        DecreaseMoneyCount(speedCost);
        speedLevel++;
        PlayerPrefs.SetInt("speedLevel", speedLevel);
        speedButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + speedLevel;

        speedCost *= costMultiplier;
        PlayerPrefs.SetFloat("speedCost", speedCost);
        speedButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(speedCost);

        float add = sign ? addSpeedAmount : -addSpeedAmount;
        float speed = 1 + add * speedLevel;
        for(int i = 0; i < pipes.childCount; i++)
        {
            pipes.GetChild(i).GetComponent<PipeController>().IncreseSpeed(speed);
        }

        CheckUIs();
    }
    public void AddPipe()
    {
        DecreaseMoneyCount(pipeCost);
        pipeLevel++;
        PlayerPrefs.SetInt("pipeLevel", pipeLevel);
        pipeButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + pipeLevel;

        pipeCost *= pipeCostMultiplier;
        PlayerPrefs.SetFloat("pipeCost", pipeCost);
        pipeButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(pipeCost);


        GameObject addedPipe = Instantiate(pipePrefab, pipes);
        addedPipe.transform.localPosition = Vector3.right * (pipes.childCount-1);
        AddRemoveToPipesArray(true, addedPipe);

        CheckUIs();
    }

    public void IncreaseIncome()
    {
        Destroy(Instantiate(buffParticle, cardObj.transform.position - Vector3.forward, Quaternion.identity), 1.5f);
        DecreaseMoneyCount(incomeCost);
        incomeLevel++;
        PlayerPrefs.SetInt("incomeLevel", incomeLevel);
        incomeButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + ConvertNumberToUIText(incomeLevel);

        incomeCost *= costMultiplier;
        PlayerPrefs.SetFloat("incomeCost", incomeCost);
        incomeButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(incomeCost);

        moneyPerCoin = 1 + incomeLevel * incomeMultiplier;

        CheckUIs();
    }

    public void NewCard()
    {
        control = false;
        cardObj = Lv2Card;
        Lv1Card.SetActive(false);
        Lv2Card.SetActive(true);
        control = true;
    }

    public void Settings(bool open)
    {
        if(open)
        {
            control = false;
            SettingsPanel.SetActive(true);
        }
        else
        {
            SettingsPanel.SetActive(false);
            control = true;
        }
    }

    public void MergePipes()
    {
        if (mergeable)
        {
            mergeable = false;
            mergedPipe = mergeablePipesArray[1].gameObject;

            // Check if the curved pipe is merged
            isCurvedMerged = mergeablePipesArray[0].GetComponent<PipeController>().isCurved ? true : false;
            areMergedDeleted = false;
            InvokeRepeating("MergePipesLoop", 0, Time.fixedDeltaTime);
        }

        CheckUIs();
    }
    public void MergePipesLoop()
    {
        control = false;
        if (!mergePhase1)
        {
            mergedPipe.transform.Find("SlidingCoin").gameObject.SetActive(false);
            mergedPipe.transform.localPosition = Vector3.MoveTowards(mergedPipe.transform.localPosition, mergedPipe.transform.localPosition + Vector3.forward * -0.5f, mergeAnimSensivity*Time.deltaTime);
            if (mergedPipe.transform.localPosition.z <= -0.5f)
            {
                mergePhase1 = true;
            }
        }
        else if (!mergePhase2)
        {
            Vector3 targetPoint = new Vector3(mergedPipe.transform.localPosition.x, mergedPipe.transform.localPosition.y, 0);
            mergeablePipesArray[0].transform.Find("SlidingCoin").gameObject.SetActive(false);
            mergeablePipesArray[2].transform.Find("SlidingCoin").gameObject.SetActive(false);
            mergeablePipesArray[0].transform.localPosition = Vector3.MoveTowards(mergeablePipesArray[0].transform.localPosition, targetPoint, mergeAnimSensivity * Time.deltaTime);
            mergeablePipesArray[2].transform.localPosition = Vector3.MoveTowards(mergeablePipesArray[2].transform.localPosition, targetPoint, mergeAnimSensivity * Time.deltaTime);
            if (mergeablePipesArray[0].transform.localPosition.x == targetPoint.x)
            {
                if (isCurvedMerged)
                {
                    mergeablePipesArray[0].GetComponent<PipeController>().SetMultiplier();
                }
                else
                {
                    mergedPipe.GetComponent<PipeController>().SetMultiplier();
                }
                mergePhase2 = true;
            }
        }
        else if (!mergePhase3)
        {
            if (!areMergedDeleted && !isCurvedMerged)
            {
                AddRemoveToPipesArray(false, mergeablePipesArray[0]);
                AddRemoveToPipesArray(false, mergeablePipesArray[2]);
                areMergedDeleted = true;
            }
            else if (!areMergedDeleted && isCurvedMerged)
            {
                AddRemoveToPipesArray(false, mergeablePipesArray[1]);
                AddRemoveToPipesArray(false, mergeablePipesArray[2]);
                mergedPipe = mergeablePipesArray[0];
                areMergedDeleted = true;
            }

            Vector3 targetPoint = new Vector3(mergedPipe.transform.localPosition.x, mergedPipe.transform.localPosition.y, 0);
            mergedPipe.transform.localPosition = Vector3.MoveTowards(mergedPipe.transform.localPosition, targetPoint, mergeAnimSensivity * Time.deltaTime); 
            if(mergedPipe.transform.localPosition.z == 0)
            {
                mergePhase3 = true;
            }
        }
        else
        {
            Vector3 targetPoint = isCurvedMerged ? curvedLocalPos : Vector3.right * (pipes.childCount - 1);
            mergedPipe.transform.localPosition = Vector3.MoveTowards(mergedPipe.transform.localPosition, targetPoint, mergeAnimSensivity * Time.deltaTime);
            if(mergedPipe.transform.localPosition.x == targetPoint.x)
            {
                mergedPipe.transform.Find("SlidingCoin").gameObject.SetActive(true);
                mergePhase1 = false;
                mergePhase2 = false;
                mergePhase3 = false;
                isCurvedMerged = false;
                mergeablePipesArray = new GameObject[0];
                CheckMergeable();
                control = true;
                CancelInvoke("MergePipesLoop");
            }
        }

    }
    private void AddRemoveToPipesArray(bool add, GameObject pipe)
    {
        if(add)
        {
            pipeCount++;
            int newSize = (pipesArray != null) ? pipesArray.Length + 1 : 1;
            Array.Resize(ref pipesArray, newSize);

            pipesArray[newSize - 1] = pipe;
            CheckMergeable();
        }
        else
        {
            if (pipesArray != null && pipesArray.Length > 0)
            {
                for (int i = 0; i < pipesArray.Length; i++)
                {
                    if (pipesArray[i] == pipe)
                    {
                        for (int j = i; j < pipesArray.Length - 1; j++)
                        {
                            pipesArray[j] = pipesArray[j + 1];
                        }

                        Array.Resize(ref pipesArray, pipesArray.Length - 1);

                        Destroy(pipe);
                        pipeCount--;

                        break;
                    }
                }
            }
        }
    }

    private void CheckMergeable()
    {
        mergeablePipesArray = new GameObject[0];
        int tempMult = pipesArray[0].GetComponent<PipeController>().multiplier;
        int sameMultiplierCount = 1;
        bool isFirstSet = false;
        for(int i = 1;i < pipesArray.Length;i++)
        {
            GameObject pipe = pipesArray[i];
            PipeController pipeController = pipe.GetComponent<PipeController>();
            if (pipeController.multiplier == tempMult)
            {
                sameMultiplierCount++;
                if(!isFirstSet)
                {
                    isFirstSet = true;
                    AddRemoveToMergeableArray(true, pipesArray[i-1]);                    
                }
                AddRemoveToMergeableArray(true, pipe);
            }
            else
            {
                isFirstSet = false;
                sameMultiplierCount = 1;
                mergeablePipesArray = new GameObject[0];
                tempMult = pipeController.multiplier;
            }

            if(sameMultiplierCount == 3)
            {
                break;
            }
        }
        mergeable = sameMultiplierCount > 2 ? true : false;

        CheckUIs();
    }

    private void AddRemoveToMergeableArray(bool add, GameObject pipe)
    {

        if (add)
        {
            int newSize = (mergeablePipesArray != null) ? mergeablePipesArray.Length + 1 : 1;
            Array.Resize(ref mergeablePipesArray, newSize);

            mergeablePipesArray[newSize - 1] = pipe;
        }
        else
        {
            if (mergeablePipesArray != null && mergeablePipesArray.Length > 0)
            {
                for (int i = 0; i < mergeablePipesArray.Length; i++)
                {
                    if (mergeablePipesArray[i] == pipe)
                    {
                        for (int j = i; j < mergeablePipesArray.Length - 1; j++)
                        {
                            mergeablePipesArray[j] = mergeablePipesArray[j + 1];
                        }

                        Array.Resize(ref mergeablePipesArray, mergeablePipesArray.Length - 1);

                        Destroy(pipe);

                        break;
                    }
                }
            }
        }
    }

    private void SetUIs()
    {
        incomeLevel = PlayerPrefs.GetInt("incomeLevel", 1);
        incomeButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + ConvertNumberToUIText(incomeLevel);

        incomeCost =  PlayerPrefs.GetFloat("incomeCost", 1);
        incomeButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(incomeCost);
        //----------------------------------------------
        speedLevel = PlayerPrefs.GetInt("speedLevel", 1);
        speedButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + ConvertNumberToUIText(speedLevel);

        speedCost = PlayerPrefs.GetFloat("speedCost", 1);
        speedButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(speedCost);
        //-----------------------------------------------
        pipeLevel = PlayerPrefs.GetInt("pipeLevel", 1);
        pipeButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + pipeLevel;

        pipeCost = PlayerPrefs.GetFloat("pipeCost", 50);
        pipeButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(pipeCost);

        float speed = 1 + addSpeedAmount * speedLevel;
        for (int i = 0; i < pipes.childCount; i++)
        {
            pipes.GetChild(i).GetComponent<PipeController>().IncreseSpeed(speed);
        }

        moneyPerCoin = 1 + incomeLevel * incomeMultiplier;

        CheckUIs();
    }

    private void CheckUIs()
    {
        incomeButton.GetComponent<Button>().interactable = moneyCount > incomeCost;
        speedButton.GetComponent<Button>().interactable = moneyCount > speedCost;
        pipeButton.GetComponent<Button>().interactable = moneyCount > pipeCost;
        mergePipeButton.GetComponent<Button>().interactable = moneyCount > pipeCost && mergeable;
        mergePipeButton.transform.Find("Cost").GetComponent<Text>().text = "$" + ConvertNumberToUIText(pipeCost);
    }
    public void ResetCosts()
    {
        speedLevel = 1;
        PlayerPrefs.SetInt("speedLevel", speedLevel);
        speedButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + ConvertNumberToUIText(speedLevel);

        speedCost = 1;
        PlayerPrefs.SetFloat("speedCost", speedCost);
        speedButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(speedCost);

        //----------------

        incomeLevel = 1;
        PlayerPrefs.SetInt("incomeLevel", incomeLevel);
        incomeButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + ConvertNumberToUIText(incomeLevel);

        incomeCost = 1;
        PlayerPrefs.SetFloat("incomeCost", incomeCost);
        incomeButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(incomeCost);

        //-----------------------------------------------

        pipeLevel = 1;
        PlayerPrefs.SetInt("pipeLevel", pipeLevel);
        pipeButton.transform.Find("LevelTx").GetComponent<Text>().text = "Level " + pipeLevel;

        pipeCost = 50f;
        PlayerPrefs.SetFloat("pipeCost", 50);
        pipeButton.transform.Find("CostTx").GetComponent<Text>().text = "$" + ConvertNumberToUIText(pipeCost);

        float speed = 1 + addSpeedAmount * speedLevel;
        for (int i = 0; i < pipes.childCount; i++)
        {
            pipes.GetChild(i).GetComponent<PipeController>().IncreseSpeed(speed);
        }

        moneyPerCoin = 1 + incomeLevel * incomeMultiplier;
        CheckUIs();
    }

    public void ResetMoney()
    {
        moneyCount = 0;
        PlayerPrefs.SetFloat("moneyCount", moneyCount);
        moneyTx.text = "$" + moneyCount;
        Debug.Log("Money: " + moneyCount);
    }

    // Reload the current scene to restart the game
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /*
    public void AddPipe()
    {
        if(pipeCount < pipes.childCount)
        {
            pipes.GetChild(pipeCount).gameObject.SetActive(true);
            AddRemoveToPipesArray(true, pipes.GetChild(pipeCount).gameObject);
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
    }*/
}
