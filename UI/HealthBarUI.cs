using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;

    public Transform barPoint;
    public bool alwaysVisible;

    public float visibleTime;
    private float timeLeft;

    Image healthSlider;
    Transform UIbar;
    Transform cam;

    CharacterStats currentStats;

    void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    void OnEnable()
    {
        cam = Camera.main.transform;

        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace && canvas.gameObject.name == "HealthBar Canvas")
            {
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        UIbar.gameObject.SetActive(true);
        timeLeft = visibleTime;

        float sliderPercent = (float) currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    void LateUpdate()
    {
        if(UIbar != null)
        {
            UIbar.position = barPoint.position;
            UIbar.forward = - cam.forward;

            if(timeLeft <= 0 && !alwaysVisible)
            {
                UIbar.gameObject.SetActive(false);
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }

    void OnDisable()
    {
        currentStats.UpdateHealthBarOnAttack -= UpdateHealthBar;
        UIbar.gameObject.SetActive(false);
    }
}
