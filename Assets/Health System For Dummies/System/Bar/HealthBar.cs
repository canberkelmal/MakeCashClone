using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FollowCameraRotation))]
public class HealthBar : MonoBehaviour
{
    [SerializeField] bool isBillboarded = true;
    [SerializeField] bool shouldShowHealthNumbers = true;

    float finalValue;
    float animationSpeed = 0.1f;
    float leftoverAmount = 0f;

    // Caches
    HealthSystemForDummies healthSystem;
    Image image;
    Text text;
    FollowCameraRotation followCameraRotation;
    public float maxHp = 100;
    public float currentHp = 100;

    private void Start()
    {
        healthSystem = GetComponentInParent<HealthSystemForDummies>();
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<Text>();
        followCameraRotation = GetComponent<FollowCameraRotation>();
        healthSystem.OnCurrentHealthChanged.AddListener(ChangeHealthFill);
        animationSpeed = healthSystem.AnimationDuration;
    }

    void Update()
    {
        animationSpeed = healthSystem.AnimationDuration;

        if (!healthSystem.HasAnimationWhenHealthChanges)
        {
            image.fillAmount = healthSystem.CurrentHealthPercentage / 100;
        }

        text.text = $"{healthSystem.CurrentHealthPercentage}/100";

        text.enabled = shouldShowHealthNumbers;

        followCameraRotation.enabled = isBillboarded;
    }

    private void ChangeHealthFill(CurrentHealth currentHealth)
    {
        if (!healthSystem.HasAnimationWhenHealthChanges) return;

        StopAllCoroutines();
        StartCoroutine(ChangeFillAmount(currentHealth));
    }

    private IEnumerator ChangeFillAmount(CurrentHealth currentHealth)
    {
        finalValue = currentHealth.percentage / 100;

        float cacheLeftoverAmount = this.leftoverAmount;

        float timeElapsed = 0;

        while (timeElapsed < animationSpeed)
        {
            float leftoverAmount = Mathf.Lerp((currentHealth.previous / healthSystem.MaximumHealth) + cacheLeftoverAmount, finalValue, timeElapsed / animationSpeed);
            this.leftoverAmount = leftoverAmount - finalValue;
            image.fillAmount = leftoverAmount;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        this.leftoverAmount = 0;
        image.fillAmount = finalValue;
    }
}