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
    public GameObject card;
    public GameObject coinObj;
    public GameObject heatWarningUI;
    public GameObject pipePrefab;
    public GameObject curvedPipePrefab;
    public Color heatCardColor;
    public Transform pipes;
    public Image heatBarFill;
    public Text moneyTx;
    public float moneyPerCoin = 1f;
    public float heatSensivity;
    public float addSpeedAmount = 0.1f;
    public float mergeAnimSensivity = 1f;
    public float incomeMultiplier = 0.1f;
    public int pipeCount = 0;

    [NonSerialized]
    public bool isMining = false;

    private GameObject[] pipesArray = new GameObject[0];
    private GameObject[] mergeablePipesArray = new GameObject[0];
    private GameObject mergedPipe;
    private bool control = true;
    private bool overHeat = false;
    private bool mergeable = false;
    private bool mergePhase1 = false;
    private bool mergePhase2 = false;
    private bool isCurvedMerged = false;
    private bool areMergedDeleted = false;
    private float moneyCount = 0f;
    private Color defCardColor;
    private Vector3 curvedLocalPos;


    private void Awake()
    {
        defCardColor = card.GetComponent<Renderer>().material.color;
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
        moneyTx.text = "$" + ((int)moneyCount).ToString();
        Debug.Log("Money: " + moneyCount);
    }

    private String ConvertNumberToUIText(float number)
    {
        String UITx = ">B";
        if(number > 1000000000)
        {
            
            UITx = ((int)(number / 1000000000)).ToString();
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
        //card.GetComponent<Renderer>().material.color = Color.Lerp(defCardColor, heatCardColor, amount*1.1f);
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
        GameObject addedPipe = Instantiate(pipePrefab, pipes);
        addedPipe.transform.localPosition = Vector3.right * (pipes.childCount-1);
        AddRemoveToPipesArray(true, addedPipe);
    }

    public void IncreaseIncome()
    {
        moneyPerCoin *= incomeMultiplier;
    }

    public void MergePipes()
    {
        if (mergeable)
        {
            mergedPipe = mergeablePipesArray[1].gameObject;

            // Check if the curved pipe is merged
            isCurvedMerged = mergeablePipesArray[0].GetComponent<PipeController>().isCurved ? true : false;
            areMergedDeleted = false;

            InvokeRepeating("MergePipesLoop", 0, Time.fixedDeltaTime);
        }
    }
    public void MergePipesLoop()
    {
        control = false;
        if (!mergePhase1)
        {
            mergedPipe.transform.localPosition = Vector3.MoveTowards(mergedPipe.transform.localPosition, mergedPipe.transform.localPosition + Vector3.forward * -0.5f, mergeAnimSensivity*Time.deltaTime);
            if (mergedPipe.transform.localPosition.z <= -0.5f)
            {
                mergePhase1 = true;
            }
        }
        else if (!mergePhase2)
        {
            Vector3 targetPoint = new Vector3(mergedPipe.transform.localPosition.x, mergedPipe.transform.localPosition.y, 0);
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
        else
        {
            if(!areMergedDeleted && !isCurvedMerged)
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

            Vector3 targetPoint = isCurvedMerged ? curvedLocalPos : Vector3.right * (pipes.childCount - 1);
            mergedPipe.transform.localPosition = Vector3.MoveTowards(mergedPipe.transform.localPosition, targetPoint, mergeAnimSensivity * Time.deltaTime);
            if(mergedPipe.transform.localPosition.x == targetPoint.x)
            {
                mergePhase1 = false;
                mergePhase2 = false;
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
