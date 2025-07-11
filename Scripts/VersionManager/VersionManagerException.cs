using UnityEngine;
using System.Collections;

namespace Hospital
{
    public class VersionManagerException : System.Exception
    {
        internal VersionManagerException(string message) : base(message)
        {

        }
    }
}