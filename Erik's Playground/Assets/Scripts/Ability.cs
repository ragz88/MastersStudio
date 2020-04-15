using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Inventory/Ability")]
public class Ability : ScriptableObject
{
    /// <summary>
    /// Stores the category that this ability falls under.
    /// Note: 'Synergised' are passives that appear with specific combinations of abilities.
    /// </summary>
    public enum AbilityCategory
    {
        Jump,
        Melee,
        Ranged,
        Defense,
        Synergised
    }

    /// <summary>
    /// Stores the type of ability - either Primary, Secondary or a hybrid
    /// of the two. Hybrids can be equipped in either a primary or secondary slot.
    /// </summary>
    public enum AbilityType
    {
        Primary,
        Secondary,
        Hybrid
    }

    public string abilityName = "New Ability";                        // The name of the ability in menus and gameplay
    public Sprite icon = null;                                        // The ability's icon to show in the inventory menu

    public AbilityCategory abilityCategory = AbilityCategory.Jump;    // The ability's core category, 
                                                                      // defining which menu it appears in

    public AbilityType abilityType = AbilityType.Primary;             // The ability's type, defining which of the slots within
                                                                      // its core category it can be equipped to

    /// <summary>
    /// Links the abilities functionality to the correct control set, allowing the character to
    /// use the ability.
    /// </summary>
    public virtual void EquipAbility()
    {

    }

}
