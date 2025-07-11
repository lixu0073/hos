using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPopupInicializer
{
    void Initialize(Action onSuccess, Action onFailure);
    void DeInitialize();
}
