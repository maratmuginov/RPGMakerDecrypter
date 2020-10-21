﻿using System.IO;
using System.Text;
using System.Collections.Generic;
using RPGMakerDecrypter.Decrypter.Exceptions;

namespace RPGMakerDecrypter.Decrypter
{
    /// <summary>
    /// Represents RGSSAD format used in RPG Maker XP and VX.
    /// </summary>
    public class RGSSADv1 : RGSSAD
    {
        public RGSSADv1(string filePath) : base(filePath)
        {
            if (GetVersion() != Constants.RGASSDv1)
                throw new InvalidArchiveException("Archive is in invalid format.");

            ReadRGSSAD();
        }

        /// <summary>
        /// Reads the contents of RGSSAD archive and populates ArchivedFiles property.
        /// </summary>
        private void ReadRGSSAD()
        {
            uint key = Constants.RGASSADv1Key;

            ArchivedFiles = new List<ArchivedFile>();

            BinaryReader.BaseStream.Seek(8, SeekOrigin.Begin);
            while (true)
            {
                int length = DecryptInteger(BinaryReader.ReadInt32(), ref key);
                var archivedFile = new ArchivedFile
                {
                    Name = DecryptFilename(BinaryReader.ReadBytes(length), ref key),
                    Size = DecryptInteger(BinaryReader.ReadInt32(), ref key),
                    Offset = BinaryReader.BaseStream.Position,
                    Key = key
                };
                ArchivedFiles.Add(archivedFile);

                BinaryReader.BaseStream.Seek(archivedFile.Size, SeekOrigin.Current);
                if (BinaryReader.BaseStream.Position == BinaryReader.BaseStream.Length)
                    break;
            }
        }
        /// <summary>
        /// Decrypts integer from given value.
        /// Proceeds key forward by calculating new value.
        /// </summary>
        /// <param name="value">Encrypted value</param>
        /// <param name="key">Key</param>
        /// <returns>Decrypted integer</returns>
        private int DecryptInteger(int value, ref uint key)
        {
            long result = value ^ key;

            key *= 7;
            key += 3;

            return (int)result;
        }

        /// <summary>
        /// Decrypts file name from given bytes using given key.
        /// Proceeds key forward by calculating new value.
        /// </summary>
        /// <param name="encryptedName">Encrypted filename</param>
        /// <param name="key">Key</param>
        /// <returns>Decrypted filename</returns>
        private string DecryptFilename(byte[] encryptedName, ref uint key)
        {
            byte[] decryptedName = new byte[encryptedName.Length];

            for (int i = 0; i <= encryptedName.Length - 1; i++)
            {
                decryptedName[i] = (byte)(encryptedName[i] ^ (key & 0xff));

                key *= 7;
                key += 3;
            }

            string result = Encoding.UTF8.GetString(decryptedName);

            return result;
        }
    }
}
