using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityReferenceHolder : Base_ReferenceHolder
{

    #region MaternitySystems
    public MaternityAISpawner MaternityAI;
    public override void Initialize()
    {
        base.Initialize();
    }
    
    #endregion
    public Camera maternityCamera;

}
