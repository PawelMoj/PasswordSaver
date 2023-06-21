using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace WinApiManager
{
    public static class CredentialManager
    {
        public static bool TryGetStoredCredentials(string appShortName,string applicationName, out Credential credential)
        {
            credential = null;

            applicationName = applicationName.RemovePrefixAndSuffix(appShortName);
            credential = EnumerateCrendentials(appShortName)?.FirstOrDefault(x => string.Equals(applicationName, x.ApplicationName.RemovePrefixAndSuffix(appShortName), StringComparison.OrdinalIgnoreCase));
            return credential != null;
        }

        private static string RemovePrefixAndSuffix(this string appName, string appShortName)
        {
            string http = appShortName + ":http";
            if (string.IsNullOrEmpty(appName) || !appName.StartsWith(http, StringComparison.OrdinalIgnoreCase))
            {
                return appName ?? "";
            }

            if (appName.EndsWith("/"))
            {
                appName = appName.Remove(appName.Length - 1);
            }

            if (appName.Length > http.Length && char.ToLower(appName[http.Length]) == 's')
            {
                return appName.Substring(http.Length + 1);
            }
            return appName.Substring(http.Length);
        }

        public static Credential ReadCredential(string applicationName)
        {
            IntPtr nCredPtr;
            bool read = CredRead(applicationName, CredentialType.Generic, 0, out nCredPtr);
            if (read)
            {
                using (CriticalCredentialHandle critCred = new CriticalCredentialHandle(nCredPtr))
                {
                    CREDENTIAL cred = critCred.GetCredential();
                    return ReadCredential(cred);
                }
            }

            return null;
        }

        private static Credential ReadCredential(CREDENTIAL credential)
        {
            string applicationName = Marshal.PtrToStringUni(credential.TargetName);
            string userName = Marshal.PtrToStringUni(credential.UserName);
            string comment = Marshal.PtrToStringUni(credential.Comment);
            SecureString secret = null;
            if (credential.CredentialBlob != IntPtr.Zero)
            {
                secret = SecureStringHelper.FromString(Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2));
            }

            return new Credential(credential.Type, applicationName, userName, secret, comment);
        }

        public static void WriteCredential(string applicationName, string userName, SecureString secret, string comment = null)
        {
            // In order to get byte[] of SecretString a messy method with pointers would be neccesary.
            // Luckly UTF-16 allocates exactly 2 byte per each char in string. https://learn.microsoft.com/en-us/previous-versions/windows/desktop/automat/bstr
            uint secretByteLength = secret == null ? 0 : (uint)secret.Length * 2;

            if (secretByteLength != 0 && secretByteLength > 512 * 5)
            {
                throw new ArgumentOutOfRangeException(nameof(secret), "The secret message has exceeded 2560 bytes.");
            }

            CREDENTIAL credential = new CREDENTIAL();
            try
            {
                credential.AttributeCount = 0;
                credential.Attributes = IntPtr.Zero;
                credential.Comment = string.IsNullOrEmpty(comment) ? IntPtr.Zero
                                                           : Marshal.StringToCoTaskMemUni(comment);
                credential.TargetAlias = IntPtr.Zero;
                credential.Type = CredentialType.Generic;
                credential.Persist = (uint)CredentialPersistence.LocalMachine;
                credential.CredentialBlobSize = secretByteLength;
                credential.TargetName = Marshal.StringToCoTaskMemUni(applicationName);
                credential.CredentialBlob = Marshal.SecureStringToCoTaskMemUnicode(secret);
                credential.UserName = Marshal.StringToCoTaskMemUni(userName ?? Environment.UserName);

                if (!CredWrite(ref credential, 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot write to credentialManager.", ex);
            }
            finally
            {
                Marshal.FreeCoTaskMem(credential.TargetName);
                Marshal.FreeCoTaskMem(credential.CredentialBlob);
                Marshal.FreeCoTaskMem(credential.UserName);
            }

        }

        public static bool DeleteCredential(string appShortName, string applicationName, out string errorMessage)
        {
            Credential toDelete = EnumerateCrendentials(appShortName)?.FirstOrDefault(x => x.ApplicationName.StartsWith(applicationName));

            if (toDelete == null)
            {
                errorMessage = "No Credential have been found";
                return false;
            }

            if (!CredDelete(toDelete.ApplicationName, toDelete.CredentialType, 0))
            {
                errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                return false;
            }

            errorMessage = null;
            return true;
        }

        public static IReadOnlyList<Credential> EnumerateCrendentials(string appShortName)
        {
            List<Credential> result = new List<Credential>();

            int count;
            IntPtr pCredentials;
            if (CredEnumerate(appShortName + ":*", 0, out count, out pCredentials))
            {
                for (int n = 0; n < count; n++)
                {
                    IntPtr credential = Marshal.ReadIntPtr(pCredentials, n * Marshal.SizeOf(typeof(IntPtr)));
                    result.Add(ReadCredential((CREDENTIAL)Marshal.PtrToStructure(credential, typeof(CREDENTIAL))));
                }
            }
            else if (Marshal.GetLastWin32Error() != 1168 && Marshal.GetLastWin32Error() != 1004)
            {
                throw new Win32Exception(); //ERROR_NOT_FOUND - 1168 or ERROR_INVALID_FLAGS - 1004
            }

            return result;
        }

        public static string GetServerNameFormatedToString(string appShortName, string serverName)
        {
            return appShortName + ":" + serverName;
        }


        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] UInt32 flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        static extern bool CredDelete(string target, CredentialType credentialType, int flag);

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CredEnumerate(string filter, int flag, out int count, out IntPtr pCredentials);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        static extern bool CredFree([In] IntPtr cred);



        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public CredentialType Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            public CREDENTIAL GetCredential()
            {
                if (!IsInvalid)
                {
                    CREDENTIAL credential = (CREDENTIAL)Marshal.PtrToStructure(handle, typeof(CREDENTIAL));
                    return credential;
                }

                throw new InvalidOperationException("Invalid CriticalHandle!");
            }

            protected override bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    CredFree(handle);
                    SetHandleAsInvalid();
                    return true;
                }

                return false;
            }
        }
    }
}
