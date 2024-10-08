using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace S3Operations
{
   public class MD5HashComputer
    {

        public static string CalculateMD5Hash(byte[] bytes)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
