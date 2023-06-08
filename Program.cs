using System;
using System.IO;

public class ImageEncryption
{
    private const int Rounds = 16;
    private const int BlockSize = 8;

    private static readonly byte[][] SubKeys = new byte[Rounds][];

    private static void GenerateSubKeys(byte[] key)
    {
        byte[] permutationKey = new byte[BlockSize];
        byte[] workingKey = new byte[BlockSize];
        byte[] subKey;

        Array.Copy(key, permutationKey, BlockSize);
        Array.Copy(key, 0, workingKey, 0, BlockSize);

        for (int round = 0; round < Rounds; round++)
        {
            subKey = new byte[BlockSize];
            Array.Copy(workingKey, subKey, BlockSize);
            SubKeys[round] = subKey;

            ShiftLeft(workingKey);
        }
    }

    private static void ShiftLeft(byte[] data)
    {
        byte carry = 0;

        for (int i = 0; i < BlockSize; i++)
        {
            byte nextCarry = (byte)(data[i] >> 7);
            data[i] = (byte)((data[i] << 1) | carry);
            carry = nextCarry;
        }
    }

    private static void EncryptImage(string imagePath, string encryptedImagePath, byte[] key)
    {
        GenerateSubKeys(key);

        using (FileStream inputFileStream = new FileStream(imagePath, FileMode.Open))
        {
            using (FileStream encryptedFileStream = new FileStream(encryptedImagePath, FileMode.Create))
            {
                byte[] block = new byte[BlockSize];
                byte[] encryptedBlock = new byte[BlockSize];

                while (inputFileStream.Read(block, 0, BlockSize) > 0)
                {
                    EncryptBlock(block, encryptedBlock);
                    encryptedFileStream.Write(encryptedBlock, 0, BlockSize);
                }
            }
        }
    }

    private static void DecryptImage(string encryptedImagePath, string decryptedImagePath, byte[] key)
    {
        GenerateSubKeys(key);

        using (FileStream encryptedFileStream = new FileStream(encryptedImagePath, FileMode.Open))
        {
            using (FileStream decryptedFileStream = new FileStream(decryptedImagePath, FileMode.Create))
            {
                byte[] block = new byte[BlockSize];
                byte[] decryptedBlock = new byte[BlockSize];

                while (encryptedFileStream.Read(block, 0, BlockSize) > 0)
                {
                    DecryptBlock(block, decryptedBlock);
                    decryptedFileStream.Write(decryptedBlock, 0, BlockSize);
                }
            }
        }
    }

    private static void EncryptBlock(byte[] block, byte[] encryptedBlock)
    {
        Array.Copy(block, encryptedBlock, BlockSize);

        for (int round = 0; round < Rounds; round++)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                encryptedBlock[i] ^= SubKeys[round][i];
            }
        }
    }

    private static void DecryptBlock(byte[] block, byte[] decryptedBlock)
    {
        Array.Copy(block, decryptedBlock, BlockSize);

        for (int round = Rounds - 1; round >= 0; round--)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                decryptedBlock[i] ^= SubKeys[round][i];
            }
        }
    }

    public static void Main()
    {
        // Ruta de la imagen original
        string imagePath = "c:\\proyectos\\perfil.png";

        // Ruta donde se guardará la imagen cifrada
        string encryptedImagePath = "c:\\proyectos\\perfilCIFRADO.png";

        // Ruta donde se guardará la imagen descifrada
        string decryptedImagePath = "c:\\proyectos\\perfilDESCIFRADO.png";

        // Clave para el cifrado (debe tener 8 bytes)
        byte[] key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

        // Cifrar la imagen
        EncryptImage(imagePath, encryptedImagePath, key);

        // Descifrar la imagen
        DecryptImage(encryptedImagePath, decryptedImagePath, key);

        Console.WriteLine("Imagen descifrada guardada en: " + decryptedImagePath);
    }
}
