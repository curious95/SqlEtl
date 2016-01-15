using System;

namespace SqlEtl.Enums
{
    [Serializable]
    public enum RecoveryModel
    {
        Full,
        BulkLogged,
        Simple
    }
}