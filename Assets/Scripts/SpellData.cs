using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spells/Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public float manaCost;
    public float cooldown;
    public float castTime;
    public GameObject spellPrefab;
    public Sprite spellIcon;
}
