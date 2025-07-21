using System;
using System.Collections.Generic;

namespace M_SAVA_INF.Environment
{
    public interface ILocalEnvironment
    {
        string GetValue(string key);
        byte[] GetSigningKeyBytes();
    }
}
