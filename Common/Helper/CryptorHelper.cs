using System;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace OpenEAP.Helper
{
    public static class CryptorHelper
    {
        #region 静态私有变量

        /// <summary>
        /// AES加密密钥
        /// Mode默认是CBC
        /// Padding默认是PKCS7
        /// Block默认是128
        /// </summary>
        private const string AesEncryptKey = "eZbrXs2vce3FjmTgRKZ2YbBQaB3oeqyE";

        /// <summary>
        /// AES加密向量
        /// </summary>
        private const string AesEncryptIv = "Fay6eZYpUjdu6bVP";

        /// <summary>
        /// DES加密密钥
        /// Mode默认是CBC
        /// Padding默认是PKCS7
        /// Block默认是64
        /// </summary>
        private const string DesEncryptKey = "JuMZq4Lc";

        /// <summary>
        /// DES加密向量
        /// </summary>
        private const string DesEncryptIv = "wG8eqcxr";

        /// <summary>
        /// MD5加密盐值
        /// </summary>
        private const string SaltValue = "yWfC9D";

        #endregion

        /// <summary>
        /// 使用MD5加密字符串
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <param name="salt">是否加盐（默认加盐）</param>
        public static string MD5(string str, bool salt = true)
        {
            var bytes = Encoding.UTF8.GetBytes(salt ? str + SaltValue : str);
            bytes = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
            return string.Join("", bytes.Select(it => it.ToString("x").PadLeft(2, '0')));
        }
        /// <summary>
        /// 16位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt16(string password)
        {
            var md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(password)), 4, 8);
            t2 = t2.Replace("-", string.Empty);
            return t2;
        }

        /// <summary>
        /// 32位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt32(string password = "")
        {
            string pwd = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(password) && !string.IsNullOrWhiteSpace(password))
                {
                    var md5 = System.Security.Cryptography.MD5.Create(); //实例化一个md5对像
                    // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
                    byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                    // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
                    foreach (var item in s)
                    {
                        // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                        pwd = string.Concat(pwd, item.ToString("X2"));
                    }
                }
            }
            catch
            {
                throw new Exception($"错误的 password 字符串:【{password}】");
            }
            return pwd;
        }

        /// <summary>
        /// 64位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt64(string password)
        {
            // 实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　            
            byte[] s = System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(s);
        }
        /// <summary>
        /// 使用SHA256加密字符串
        /// </summary>
        /// <param name="str">原始字符串</param>
        public static string SHA256(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            bytes = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 使用Base64加密字符串
        /// </summary>
        /// <param name="str">原始字符串</param>
        public static string Base64Encrypt(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 使用Base64解密字符串
        /// </summary>
        /// <returns>The encrypt.</returns>
        /// <param name="str">Base64加密的字符串</param>
        public static string Base64Decrypt(string str)
        {
            var bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 使用AES加密字符串
        /// </summary>
        /// <param name="str">原始字符串</param>
        public static string AESEncrypt(string str)
        {
            if (str == null)
            {
                return null;
            }
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var aes = new RijndaelManaged())
            {
                var encryptor = aes.CreateEncryptor(
                    Encoding.ASCII.GetBytes(AesEncryptKey),
                    Encoding.ASCII.GetBytes(AesEncryptIv));
                var result = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// 使用AES解密字符串
        /// </summary>
        /// <param name="base64Str">AES加密的字符串</param>
        public static string AESDecrypt(string base64Str)
        {
            if (base64Str==null)
            {
                return null;
            }
            var bytes = Convert.FromBase64String(base64Str);
            using (var aes = new RijndaelManaged())
            {
                var decryptor = aes.CreateDecryptor(
                    Encoding.ASCII.GetBytes(AesEncryptKey),
                    Encoding.ASCII.GetBytes(AesEncryptIv));
                var result = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(result);
            }
        }

        /// <summary>
        /// 使用DES加密字符串
        /// </summary>
        /// <param name="str">原始字符串</param>
        public static string DESEncrypt(string str)
        {
            if (str == null)
            {
                return null;
            }
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var des = new DESCryptoServiceProvider())
            {
                var encryptor = des.CreateEncryptor(
                    Encoding.ASCII.GetBytes(DesEncryptKey),
                    Encoding.ASCII.GetBytes(DesEncryptIv));
                var result = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// 使用DES解密字符串
        /// </summary>
        /// <param name="base64Str">DES加密的字符串</param>
        public static string DESDecrypt(string base64Str)
        {
            if (base64Str == null)
            {
                return null;
            }
            var bytes = Convert.FromBase64String(base64Str);
            using (var des = new DESCryptoServiceProvider())
            {
                var decryptor = des.CreateDecryptor(
                    Encoding.ASCII.GetBytes(DesEncryptKey),
                    Encoding.ASCII.GetBytes(DesEncryptIv));
                var result = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(result);
            }
        }
    }
}