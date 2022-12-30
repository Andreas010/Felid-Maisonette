using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCheck : MonoBehaviour
{

    enum TypeToCheckFor {Tag, Layer, Both};
    [SerializeField] TypeToCheckFor type;

    [SerializeField] string tagToCheckFor;
    [SerializeField] LayerMask layerToCheckFor;

    public bool overlapping;

    private void OnTriggerStay2D(Collider2D collision)
    {
        switch(type)
        {
            case TypeToCheckFor.Tag:
                {
                    if (collision.gameObject.CompareTag(tagToCheckFor)) overlapping = true;
                    break;
                }
            case TypeToCheckFor.Layer:
                {
                    if (IsInLayerMask(collision.gameObject.layer, layerToCheckFor)) overlapping = true;
                    break;
                }
            case TypeToCheckFor.Both:
                {
                    if (collision.gameObject.CompareTag(tagToCheckFor) || IsInLayerMask(collision.gameObject.layer, layerToCheckFor)) overlapping = true;
                    break;
                }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (type)
        {
            case TypeToCheckFor.Tag:
                {
                    if (collision.gameObject.CompareTag(tagToCheckFor)) overlapping = false;
                    break;
                }
            case TypeToCheckFor.Layer:
                {
                    if (IsInLayerMask(collision.gameObject.layer, layerToCheckFor)) overlapping = false;
                    break;
                }
            case TypeToCheckFor.Both:
                {
                    if (collision.gameObject.CompareTag(tagToCheckFor) || IsInLayerMask(collision.gameObject.layer, layerToCheckFor)) overlapping = false;
                    break;
                }
        }
    }

    public static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return ((1 << layer) & mask) != 0;
    }
}
