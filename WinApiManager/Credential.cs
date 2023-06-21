using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WinApiManager
{
    public class Credential
    {
        private readonly string _applicationName;
        private readonly string _userName;
        private readonly string _comment;
        private readonly SecureString _password;
        private readonly CredentialType _credentialType;

        public Credential(CredentialType credentialType, string applicationName, string userName, SecureString password, string comment)
        {
            _applicationName = applicationName;
            _userName = userName;
            _password = password;
            _credentialType = credentialType;
            _comment = comment;
        }

        public CredentialType CredentialType
        {
            get { return _credentialType; }
        }

        public string ApplicationName
        {
            get { return _applicationName.ToLower(); }
        }

        public string UserName
        {
            get { return _userName; }
        }
        public string Comment
        {
            get { return _comment; }
        }

        public SecureString Password
        {
            get { return _password; }
        }
    }
}
