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


    private float fanSpeed = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        fanSpeed = defaultFanSpeed;
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
}
