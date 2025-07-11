using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VitaminDropAnimationController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    List<Animator> vitaminAnimatorSystems;
    [SerializeField]
    float distanceUnit = .3f;
    List<Transform> animatorPositions;
    [SerializeField]
    List<TextMeshPro> amountToShow;
#pragma warning restore 0649

    private void Start()
    {
        animatorPositions = new List<Transform>();
        for (int i = 0; i < vitaminAnimatorSystems.Count; i++)
        {
            animatorPositions.Add(vitaminAnimatorSystems[i].gameObject.GetComponent<Transform>());
        }
    }

    internal void ShowAnimation(Vector3 position, MedicineRef vitamin, int amount, int amountOfUnlockedCollectors)
    {
        this.gameObject.transform.position = new Vector3(position.x, 1.8f, position.z);
        amountToShow[vitamin.id].text = "+" + amount;
        if (amountOfUnlockedCollectors > 0)
        {
            float minimum = -((amountOfUnlockedCollectors - 1) * distanceUnit);
            for (int i = 0; i < amountOfUnlockedCollectors; i++)
            {
                float positionX = minimum + i * distanceUnit * 2;
                float positionZ = -positionX;
                animatorPositions[i].localPosition = new Vector3(positionX, 0, positionZ);
            }
            vitaminAnimatorSystems[vitamin.id].SetTrigger("Drip");
            SoundsController.Instance.PlayPanaceaBubble();
        }
    }
}
