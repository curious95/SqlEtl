using System;

namespace SqlEtl.Enums
{
    [Serializable]
    public enum ScriptType
    {
        None,
        Recovery,
        Truncate,
        DisableConstraint,
        Select
    }
}