using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public Transform cardFan1, cardFan2;
    public float defaultFanSpeed = 1f;
    public float poweredFanSpeed = 1f;
    public bool isLv2 = false;
    public Material[] heatMateraials = new Material[0];
    public Color[] heatDefColors = new Color[0];

    private float fanSpeed = 1f;
    private bool isSmoked = false;
    private bool isFired = false;

    // Start is called before the first frame update
    void Awake()
    {
        fanSpeed = defaultFanSpeed;
    }

    private void Start()
    {
        SetDefColors();
    }
    // Update is called once per frame
    void Update()
    {
        RotateFan(fanSpeed);
    }



    private void RotateFan(float rotationSpeed)
    {
        if (!isLv2)
        {
            cardFan1.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        else
        {
            cardFan1.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            cardFan2.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetFanSpeed(bool boost)
    {
        fanSpeed = boost ? poweredFanSpeed : defaultFanSpeed;
    }

    private void SetDefColors()
    {
        heatDefColors = new Color[heatMateraials.Length];
        for(int i = 0; i<heatMateraials.Length; i++)
        {
            heatDefColors[i] = heatMateraials[i].color;
        }
    }

    public void HeatCard(float heatAmount)
    {        
        isFired = heatAmount >= 0.75f ? true : false;
        isSmoked = heatAmount >= 0.5f ? true : false;
        transform.Find("Smokes").gameObject.SetActive(isSmoked);
        transform.Find("Fire").gameObject.SetActive(isFired);

        for (int i = 0; i < heatMateraials.Length; i++)
        {
            heatMateraials[i].color = Color.Lerp(heatDefColors[i], Color.red, heatAmount);
        }
    }
}
