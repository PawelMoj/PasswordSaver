using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApiManager
{
    public enum CredentialType
    {
        Generic = 1,
        DomainPassword,
        DomainCertificate,
        DomainVisiblePassword,
        GenericCertificate,
        DomainExtended,
        Maximum,
        MaximumEx = Maximum + 1000,
    }
}
