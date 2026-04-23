using System.Collections.Generic;
using UnityEngine;

public class WeaponDamageTable : MonoBehaviour
{
    public Dictionary<WeaponType, Dictionary<MaterialType, int>> damageTable = new Dictionary<WeaponType, Dictionary<MaterialType, int>>();

    private bool initialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        damageTable[WeaponType.Semi] = new Dictionary<MaterialType, int>
        {
            { MaterialType.Wood, 5 },
            { MaterialType.Metal, 5 },
            { MaterialType.Barrel, 8 },
            { MaterialType.Skin, 10 },
            { MaterialType.Stone, 10 },
            { MaterialType.Wall, 3 }
        };

        damageTable[WeaponType.Auto] = new Dictionary<MaterialType, int>
        {
            { MaterialType.Wood, 3 },
            { MaterialType.Metal, 3 },
            { MaterialType.Barrel, 4 },
            { MaterialType.Skin, 6 },
            { MaterialType.Stone, 4 },
            { MaterialType.Wall, 2 }
        };

        damageTable[WeaponType.Laser] = new Dictionary<MaterialType, int>
        {
            { MaterialType.Wood, 99 },
            { MaterialType.Metal, 99 },
            { MaterialType.Barrel, 33 },
            { MaterialType.Skin, 99 },
            { MaterialType.Stone, 33 },
            { MaterialType.Wall, 20 }
        };
    }

    public int GetDamage(WeaponType weaponType, MaterialType materialType)
    {
        EnsureInitialized();

        if (damageTable.ContainsKey(weaponType) && damageTable[weaponType].ContainsKey(materialType))
        {
            return damageTable[weaponType][materialType];
        }

        return 0;
    }
}
