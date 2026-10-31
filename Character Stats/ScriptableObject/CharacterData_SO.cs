using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuff;

    public float LevelMultiplier
    {
        get{ return  1 + (currentLevel - 1) * levelBuff; }
    }
    public void UpdateExp(int point)
    {
        currentExp += point;

        if (currentExp >= baseExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // 所有更新内容
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int) (baseExp * LevelMultiplier);

        maxHealth += (int) (maxHealth * LevelMultiplier);
        currentHealth = maxHealth;
        
        // 提升防御力太高会导致玩家不掉血
        // baseDefence += (int) (baseDefence * LevelMultiplier);
        // currentDefence = baseDefence;
    }

}
