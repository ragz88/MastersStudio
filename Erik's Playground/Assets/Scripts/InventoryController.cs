using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public List<Ability> equippedAbilities;
    public List<Ability> storedAbilities;


    bool showEquippedAbilitiesInInventory = false;


    // The singleton ensures that only one instance of this script will ever exist at a given time. This makes the script easy to reference.
    #region Singleton
    public static InventoryController inventoryInstance;

    private void Awake()
    {
        // We check if another instance of this exists - if it does, we destroy this. 
        // This ensures that only one of these objects can ever exist at a time.
        if (inventoryInstance == null)
        {
            inventoryInstance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
