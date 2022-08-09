using System.Security.Cryptography;
using System.Text;

namespace AuthService.Security
{
    public class Encryption
    {
        //Variables para cifrado de la contraseña
        private static string palapaso = "WS_CAP";
        private static string valorSalt = "WS_CAP_2022";
        private static string encrip = "SHA256";
        private static string vector = "1234567891234567";
        private static int ite = 22;
        private static int tam_clave = 256;

        //Encriptar
        public static string Encrypt(string textoCifrar)
        {
            try
            {
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(valorSalt);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(textoCifrar);

                PasswordDeriveBytes password =
                    new PasswordDeriveBytes(palapaso, saltValueBytes,
                        encrip, ite);
                byte[] keyBytes = password.GetBytes(tam_clave / 8);
                RijndaelManaged symmetricKey = new();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor =
                    symmetricKey.CreateEncryptor(keyBytes, InitialVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                string textoCifradoFinal = Convert.ToBase64String(cipherTextBytes);
                return textoCifradoFinal;
            }
            catch
            {
                return "";
            }
        }

        //Desencruptar la contraseña
        public static string Decrypt(string textoCifrado)
        {
            try
            {
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(valorSalt);
                byte[] cipherTextBytes = Convert.FromBase64String(textoCifrado);

                PasswordDeriveBytes password =
                    new PasswordDeriveBytes(palapaso, saltValueBytes,
                        encrip, ite);
                byte[] keyBytes = password.GetBytes(tam_clave / 8);
                RijndaelManaged symmetricKey = new();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, InitialVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                string textoDescifradoFinal = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                return textoDescifradoFinal;
            }
            catch
            {
                return "";
            }
        }

        public static string SanitizeString(string dirtyString)
        {
            HashSet<char> removeChars = new HashSet<char>(" ?&^$#@!()+-,:;<>’\'-_*");
            StringBuilder result = new StringBuilder(dirtyString.Length);
            foreach (char c in dirtyString)
                if (!removeChars.Contains(c))
                    result.Append(c);
            return result.ToString();
        }
    }
}