using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactItem : MonoBehaviour
{
    internal enum ArtifactType { NULL, WATER, FIRE, MAGIC }
    [SerializeField] internal ArtifactType type;
    [SerializeField] GameObject pickupItemSound;

    GameObject PickupLetter;

    private void Start()
    {
        PickupLetter = transform.GetChild(0).gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        bool isPlayer = other.tag.Equals("Player");

        if (isPlayer)
            PickupLetter.SetActive(true);
        if(isPlayer && Input.GetKey(KeyCode.E))
        {
            //pickup item and play sound/effects
            Instantiate(pickupItemSound);
            PlaceItem(other.GetComponent<Inventory>());
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
            PickupLetter.SetActive(false);
    }

    void PlaceItem(Inventory itemType)
    {
        switch(type)
        {
            case ArtifactType.WATER:
                itemType.waterGem = true;

                break;
            case ArtifactType.FIRE:
                itemType.fireGem = true;

                break;
            case ArtifactType.MAGIC:
                itemType.magicGem = true;

                break;
        }
    }
}