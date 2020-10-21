using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using RPGMakerDecrypter.Decrypter.Exceptions;

namespace RPGMakerDecrypter.Decrypter
{
    /// <summary>
    /// Represents RGSSAD format used in RPG Maker VX Ace.
    /// </summary>
    public class RGSSADv3 : RGSSAD
    {
        public RGSSADv3(string filePath) : base(filePath)
        {
            if (GetVersion() != Constants.RGASSDv3)
                throw new InvalidArchiveException("Archive is in invalid format.");

            ReadRGSSAD();
        }

        /// <summary>
        /// Reads the contents of RGSSAD archive and populates ArchivedFiles property.
        /// </summary>
        private void ReadRGSSAD()
        {
            BinaryReader.BaseStream.Seek(8, SeekOrigin.Begin);

            uint key = Convert.ToUInt32(BinaryReader.ReadInt32());
            key *= 9;
            key += 3;

            ArchivedFiles = new List<ArchivedFile>();

            while (true)
            {
                var archivedFile = new ArchivedFile
                {
                    Offset = DecryptInteger(BinaryReader.ReadInt32(), key),
                    Size = DecryptInteger(BinaryReader.ReadInt32(), key),
                    Key = Convert.ToUInt32(DecryptInteger(BinaryReader.ReadInt32(), key))
                };

                int length = DecryptInteger(BinaryReader.ReadInt32(), key);

                if (archivedFile.Offset == 0)
                    break;

                archivedFile.Name = DecryptFilename(BinaryReader.ReadBytes(length), key);

                ArchivedFiles.Add(archivedFile);
            }
        }

        /// <summary>
        /// Decrypts integer from given value.
        /// </summary>
        /// <param name="value">Encrypted value</param>
        /// <param name="key">Key</param>
        /// <returns>Decrypted integer</returns>
        private int DecryptInteger(int value, uint key) => Convert.ToInt32(value ^ key);

        /// <summary>
        /// Decrypts file name from given bytes using given key.
        /// </summary>
        /// <param name="encryptedName">Encrypted filename</param>
        /// <param name="key">Key</param>
        /// <returns>Decrypted filename</returns>
        private string DecryptFilename(byte[] encryptedName, uint key)
        {
            byte[] decryptedName = new byte[encryptedName.Length];

            byte[] keyBytes = BitConverter.GetBytes(key);

            int j = 0;
            for (int i = 0; i <= encryptedName.Length - 1; i++)
            {
                if (j == 4)
                    j = 0;
                decryptedName[i] = (byte)(encryptedName[i] ^ keyBytes[j]);
                j++;
            }

            return Encoding.UTF8.GetString(decryptedName);
        }
    }
}
