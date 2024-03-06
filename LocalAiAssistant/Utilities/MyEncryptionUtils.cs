using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LocalAiAssistant.Utilities
{
    internal class MyEncryptionUtils
    {
        internal static string RSAEncrypt(string plainText, string key)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(key);
                var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                var cipherBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA512);
                return Convert.ToBase64String(cipherBytes);
            }
        }

        internal static string RSADecrypt(string cipherText, string key)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(key);
                var cipherBytes = Convert.FromBase64String(cipherText);
                var plainBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA512);
                return System.Text.Encoding.UTF8.GetString(plainBytes);
            }
        }
        internal static class MyRSAEncryption
        {
            internal static bool IsValidRsaKey(string key, int KeySize = 2048)
            {

                using (var rsa = new RSACryptoServiceProvider())
                {
                    try
                    {
                        rsa.FromXmlString(key);
                    }
                    catch
                    {
                        return false;
                    }
                    return rsa.KeySize == KeySize;
                }
            }
            internal static Task<string> Encrypt(string plainText, out string key, string keyToUse = "", int keySize = 2048)
            {
                if (string.IsNullOrWhiteSpace(plainText))
                {
                    throw new Exception("Plain Text provided to Encrypt is null or empty");
                }

                string cipherText = string.Empty;

                using (var rsa = new RSACryptoServiceProvider(keySize))
                {
                    if (!string.IsNullOrWhiteSpace(keyToUse))
                    {
                        if (!IsValidRsaKey(keyToUse, keySize))
                        {
                            throw new Exception($"The provided key does not match the expected format for an RSA {keySize} key.");
                        }
                        rsa.FromXmlString(keyToUse);
                    }

                    key = rsa.ToXmlString(includePrivateParameters: true);

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        throw new Exception("Key is null or empty");
                    }

                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[]? encryptedChunks = EncryptChunks(plainBytes, rsa);

                    if (encryptedChunks != null)
                    {
                        cipherText = Convert.ToBase64String(encryptedChunks);
                    }
                    else
                    {
                        throw new Exception("Encrypted Data received from Encrypt is null or empty");
                    }
                }

                return Task.FromResult(cipherText);
            }
            internal static Task<string> Decrypt(string cipherText, string key)
            {
                if (string.IsNullOrWhiteSpace(cipherText))
                {
                    throw new Exception("Cipher Text provided to Decrypt is null or empty");
                }
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new Exception("Key provided to Decrypt is null or empty");
                }
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.FromXmlString(key);
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    var decryptedChunks = DecryptChunks(cipherBytes, rsa);
                    return Task.FromResult(Encoding.UTF8.GetString(decryptedChunks));
                }
            }
            internal static byte[] EncryptChunks(byte[] data, RSACryptoServiceProvider rsa, int ChunkSize = 200)
            {
                int dataSize = data.Length;
                int iterations = dataSize / ChunkSize;
                int lastChunkSize = dataSize % ChunkSize;
                if (lastChunkSize > 0)
                {
                    iterations++;
                }
                byte[] encryptedData = Array.Empty<byte>();
                for (int i = 0; i < iterations; i++)
                {
                    int chunkSize = (i == iterations - 1 && lastChunkSize > 0) ? lastChunkSize : ChunkSize;
                    byte[] chunkData = new byte[chunkSize];
                    Array.Copy(data, i * ChunkSize, chunkData, 0, chunkSize);
                    byte[] encryptedChunk = rsa.Encrypt(chunkData, RSAEncryptionPadding.Pkcs1);
                    encryptedData = encryptedData.Concat(encryptedChunk).ToArray();
                }
                return encryptedData;
            }
            internal static byte[] DecryptChunks(byte[] data, RSACryptoServiceProvider rsa)
            {
                int dataSize = data.Length;
                int iterations = dataSize / 256;
                int lastChunkSize = dataSize % 256;
                if (lastChunkSize > 0)
                {
                    iterations++;
                }
                byte[] decryptedData = Array.Empty<byte>();
                for (int i = 0; i < iterations; i++)
                {
                    int chunkSize = (i == iterations - 1 && lastChunkSize > 0) ? lastChunkSize : 256;
                    byte[] chunkData = new byte[chunkSize];
                    Array.Copy(data, i * 256, chunkData, 0, chunkSize);
                    byte[] decryptedChunk = rsa.Decrypt(chunkData, RSAEncryptionPadding.Pkcs1);
                    decryptedData = decryptedData.Concat(decryptedChunk).ToArray();
                }
                return decryptedData;
            }
            internal static async Task<string> TestEncription(string plainText)
            {
                string retStr = string.Empty;
                // Encryption
                Stopwatch stopWatch = new();
                stopWatch.Start();
                string cipherText = await MyRSAEncryption.Encrypt(plainText, out string key);
                stopWatch.Stop();
                long encryptionTime = stopWatch.ElapsedMilliseconds;
                retStr += ($"Encrypted text: {cipherText}\n");
                retStr += ($"Time taken for encryption: {encryptionTime} ms\n");
                retStr += ($"Valid Key Used: {IsValidRsaKey(key)}\n");

                // Decryption
                stopWatch.Restart();
                string decryptedText = await MyRSAEncryption.Decrypt(cipherText, key);
                stopWatch.Stop();
                long decryptionTime = stopWatch.ElapsedMilliseconds;

                retStr += ($"Decrypted text: {decryptedText}\n");
                retStr += ($"Time taken for decryption: {decryptionTime} ms\n");
                return retStr;
            }
            internal static readonly string DefaultRSAkey = @"<RSAKeyValue><Modulus>2/g0uy9tRwRVdCHM1ANR404/d+QjsB0X0jYCddpQPDSPHntkpg3Ugbn2SDfgXHbDYT/0qAQ6L/MXJOkVISJyIc" +
                @"IbTQ5HhipIjkthou9JRSne9o7ODufW2KB4BSvdUE2jIvM9ispSH58Dw5ASSIQpf9kaznl75jr+3tZn/KulVkLWfghoSY4lKcWPYJVWxcnNBpEMQQ8m4pfPNNVAxFUEPCtK83EiH2fFZde4azEJn+UKSvJ" +
                @"0FRZZ0Iu5tUucjP5XQrMX3yGfxFaNZy8g+grwViP2Xb0xNWd27xvL1N/rA3TZl+ooL+pazlsHhqVeTMwgbrlYGo3ThRWg8CcLi0aPjQ==</Modulus><Exponent>AQAB</Exponent><P>80CJTV0M3P" +
                @"WxZiD7zMu1EJIWYMUs2y6z/t9qerx3eLrJN2rvVhEYFEceo4DwVFlXcryKn2mwXiHZHIJH6gGMfOBgCqYsiY3gmMG36jKKLYz/NAdRk4g+i80gHX5Sl94EF3ZyRR6tYclHYb4dm/aJjeATKXdooEAfGFi" +
                @"lYVmkOac=</P><Q>539Py3qlOxtjZaADib1bD4G0N/lo94yLDLuBLKwzjCU0OMd94rqHPoc0NBWf9cOy02jsoJlZYRi/KI1KU+y2k7kUznWpL469x7QExr6Eor4DITPoUFG4eu5g5gWL93b9jRPatfnq4" +
                @"xX5AEh/p+zlrUzCCdJw4lJZM9rE+naTK6s=</Q><DP>RzdGu2qZoHsiA8d2UL/287IBcTVo64ah1eWs8+AHjkYJMPtStLzucslbk/zk350EeCfw4bj8lKKOQMV3rm/jpI/ALn32HhN2hOJ6KMvBHpLgCG" +
                @"h4HpxNy3ozg87++U1bn07jJ49SrYVTK6+QVZ/5J5tJoOM/s2Lyd5tEV9r5tJM=</DP><DQ>whuWJN/pu+3zIjtRvCTyPcZb/rM6uJc34K04WuTpNBHd/94GSB5vWJa1xxZ60fAN+gZ4oxDySOAfKTmPoV" +
                @"9Sl/sQ3yz5d88QgmsHtj35qRv6M9T6bp6XOTy4MydjfVRgtfJ36S5tLYw3BW3E3GFfFDjQWrwBu0OhWEGP+RODw5E=</DQ><InverseQ>OKEib/FOZC4l+HD6qeHEy2OMaglyVwcB1d1sZJQfWk/Vwb/V" +
                @"NF/3DdWLlCiv9qm7RnVmg5SkTrZaKfa3+LR1Jj1ubFQLwwp7rOiICHG1aYtZ1X5kpf4Al/HJ2vCSLcZ38g+/glM8i0JhcEN0LRKjJ1jzJeSO03n+9/s8aUB7yKk=</InverseQ><D>y/dV5e/vMze4iSv" +
                @"CQyEk8FGhFmmMEgSYDwqXs3IQjhLqVohyZmtSjkvCK2rsdrCFMIreMGFjFw0ge9BJEGVhUR4stIG2HpjcmF0blrqsxR4zKYp3VWjyVgJN5/WpjInA6GqFuUkrZzjKg6721Rj/ZhYI/kW1dmFj7XsAB1Gd" +
                @"yOIOzGYQE3LJDgjyC+tKIwTEcoVDnEsmlil/jhsGYfG64F2AhxM6ar7mSssisuavTgXOdTbNrKLVQI/zrKu7KhJbThlylyKtKP3lZqkHa71ldQzk3hzEQTJ8R82RS1OoEJWxLwN8STCLOCAMMDDTVLmXt" +
                @"0Z2QLPCj4UFitCnXhuMvQ==</D></RSAKeyValue>";
        }
    }
}