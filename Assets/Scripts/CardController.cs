using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [NonSerialized]
    public Transform cardFan;
    public float defaultFanSpeed = 1f;
    public float poweredFanSpeed = 1f;


    private float fanSpeed = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        cardFan = transform.Find("Fan");
        fanSpeed = defaultFanSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        RotateFan(fanSpeed);
    }



    private void RotateFan(float rotationSpeed)
    {
        cardFan.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    public void SetFanSpeed(bool boost)
    {
        fanSpeed = boost ? poweredFanSpeed : defaultFanSpeed;
    }
}
