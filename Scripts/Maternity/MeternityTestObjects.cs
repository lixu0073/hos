using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeternityTestObjects : MonoBehaviour {

    public Button levelUpButton;

    public void AddExperienceWithAnimation()
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(
            GiftType.Exp, levelUpButton.transform.position, 5,0.2f,1,Vector3.one
            ,Vector3.one,null,null,
            () =>
            {
                MaternityGameState.Get().AddExperience(DeveloperParametersController.Instance().parameters.expDevIncrease, EconomySource.ProbeTable, true);
            }
            );
    }
    public void LevelUp()
    {
        MaternityGameState.Get().LevelUp();
    }
}
