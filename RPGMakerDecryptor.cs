using System.Numerics;

namespace rpgmaker_mv_decoder;

public static class RPGMakerDecryptor
{
	public static readonly byte[] RPG_MAKER_MV_MAGIC = new byte[] { 0x52, 0x50, 0x47, 0x4d, 0x56 };

	/// <summary>
	/// Decryption using XOR operation
	/// </summary>
	/// <param name="data">Data</param>
	/// <param name="key">Key</param>
	/// <returns>Decrypted data</returns>
	/// <exception cref="ArgumentNullException">Data or key is null</exception>
	/// <exception cref="ArgumentException">Key is empty</exception>
	/// <remarks>Performing the XOR operation byte by byte, which is simpler and more efficient, and it avoids the need to handle endianness.</remarks>
	public static byte[] IntXor(byte[] data, byte[] key)
	{
		if (data == null || key == null)
		{
			throw new ArgumentNullException(data == null ? nameof(data) : nameof(key), "Data or key cannot be null");
		}

		if (key.Length == 0)
		{
			throw new ArgumentException("Key must not be empty", nameof(key));
		}

		var result = new byte[data.Length];

		for (var i = 0; i < data.Length; i++)
		{
			// i % key.Length is a trick to ensure the index always smaller than key.Length
			result[i] = (byte)(data[i] ^ key[i % key.Length]);
		}

		return result;
	}

	/// <summary>
	/// Decryption using XOR operation
	/// </summary>
	/// <param name="data">Data</param>
	/// <param name="key">Key</param>
	/// <returns>Decrypted data</returns>
	/// <exception cref="ArgumentNullException">Data or key is null</exception>
	/// <exception cref="ArgumentException">Key is empty</exception>
	/// <remarks>Original version of performing the XOR operation from Python. Just take a note that C# will use littile endian while Python is using big endian</remarks>
	public static byte[] OriginalIntXor(byte[] data, byte[] key)
	{
		if (data == null || key == null)
		{
			throw new ArgumentNullException(data == null ? nameof(data) : nameof(key), "Data or key cannot be null");
		}

		if (key.Length == 0)
		{
			throw new ArgumentException("Key must not be empty", nameof(key));
		}

		// Ensure key is as long as var
		key = key.Take(data.Length).ToArray();

		// Convert byte arrays to BigInteger (reverse for big-endian)
		var intVar = new BigInteger(data.Reverse().ToArray());
		var intKey = new BigInteger(key.Reverse().ToArray());

		// Perform XOR operation (big-endian)
		var intEnc = intVar ^ intKey;

		// Convert back to byte array and reverse again
		var result = intEnc.ToByteArray().Reverse().ToArray();

		return result;
	}
}
