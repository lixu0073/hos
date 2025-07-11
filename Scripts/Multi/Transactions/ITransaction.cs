using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Transactions
{

    public interface ITransaction<T>
    {

        string GetKey();
        string Stringify();
        T Parse(string unparsedData);

    }
}
