using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondTransaction
{
    public Action onConfirmationSuccess;
    public Guid ID;

    public DiamondTransaction(Action onConfirmationSuccess, Guid ID)
    {
        this.onConfirmationSuccess = onConfirmationSuccess;
        this.ID = ID;
    }

    public void FinalizeTransaction()
    {
        onConfirmationSuccess?.Invoke();
    }

    public override bool Equals(object obj)
    {
        if (obj==null || obj.GetType() != GetType())
        {
            return false;
        }
        DiamondTransaction objAsDiaTrans = obj as DiamondTransaction;
        return objAsDiaTrans.ID == ID;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
