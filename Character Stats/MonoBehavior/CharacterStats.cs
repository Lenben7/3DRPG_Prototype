using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;

    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;

    void Awake()
    {
        if(templateData != null)
        {
            characterData = Instantiate(templateData);
        }
    }

    #region Read from Data_SO
    public int maxHealth
    {
        get{ if(characterData != null) {return characterData.maxHealth;} else {return 0;} }
        set{ characterData.maxHealth = value; }
    }
    public int currentHealth
    {
        get{ if(characterData != null) {return characterData.currentHealth;} else {return 0;} }
        set{ characterData.currentHealth = value; }
    }
    public int baseDefence
    {
        get{ if(characterData != null) {return characterData.baseDefence;} else {return 0;} }
        set{ characterData.baseDefence = value; }
    }
    public int currentDefence
    {
        get{ if(characterData != null) {return characterData.currentDefence;} else {return 0;} }
        set{ characterData.currentDefence = value; }
    }
    #endregion

    #region Character Combat

    public void TakeDamage(CharacterStats attacker, CharacterStats defener) 
    {
        int damage = Mathf.Max(attacker.CurrentDamage()-defener.currentDefence, 0);
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("hit");
        }
        
        // Update UI
        UpdateHealthBarOnAttack?.Invoke(currentHealth, maxHealth);
        // 经验update
        if(currentHealth <= 0)
        {
            attacker.characterData.UpdateExp(characterData.killPoint);
        }
    }

    public void TakeDamage(int damage, CharacterStats defener)
    {
        int currentDamage = Mathf.Max(damage - defener.currentDefence, 0);
        currentHealth = (int) MathF.Max(currentHealth - currentDamage ,0);
        UpdateHealthBarOnAttack?.Invoke(currentHealth, maxHealth);

        if(currentHealth <= 0)
        {
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
        }
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamge, attackData.maxDamage);

        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }

        return (int)coreDamage;
    }

    #endregion
}
