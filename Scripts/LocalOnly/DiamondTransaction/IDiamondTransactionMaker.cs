using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDiamondTransactionMaker
{
    void InitializeID();
    Guid GetID();
    void EraseID();
}
