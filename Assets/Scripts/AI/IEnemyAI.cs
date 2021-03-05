using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAI
{
    void TakeDamage(int _damage, Spell.SpellType _spellType);
}
