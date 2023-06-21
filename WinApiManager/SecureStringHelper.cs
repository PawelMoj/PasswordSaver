using System.Runtime.InteropServices;
using System.Security;

namespace WinApiManager
{
    public class SecureStringHelper
    {
        public static string FromSecureString(SecureString ss)
        {
            var bstr = Marshal.SecureStringToBSTR(ss);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        public static SecureString FromString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var result = new SecureString();
            foreach (var c in s)
            {
                result.AppendChar(c);
            }

            return result;
        }

        public static bool CompareSecureStrings(SecureString s1, SecureString s2)
        {
            if (s1.Length != s2.Length)
            {
                return false;
            }
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(s1);
                bstr2 = Marshal.SecureStringToBSTR(s2);

                for (int x = 0; x < s1.Length; ++x)
                {
                    int offset = x * sizeof(short);
                    if (Marshal.ReadInt16(bstr1, offset) != Marshal.ReadInt16(bstr2, offset))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }

    }
}
