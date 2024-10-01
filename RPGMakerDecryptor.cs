namespace rpgmaker_mv_decoder;

public static class RPGMakerDecryptor
{
	private const string OctetStream = "application/octet-stream";
	private const string RPG_MAKER_MV_MAGIC = "5250474d560000000003010000000000";

	/// <summary>
	/// Perform XOR encryption/decryption on a byte array using a given key.
	/// </summary>
	/// <param name="data">The byte array to encrypt/decrypt.</param>
	/// <param name="key">The key used for the XOR operation.</param>
	/// <exception cref="ArgumentNullException">Data or key is null</exception>
	/// <returns>The encrypted/decrypted byte array.</returns>
	/// <remarks>The logic in this code will always return big endian bytes.</remarks>
	public static byte[] IntXor(byte[] data, byte[] key)
	{
		// Check for null or empty data
		ArgumentNullException.ThrowIfNull(data);
		ArgumentNullException.ThrowIfNull(key);

		// Ensure the key is as long as data (truncate if necessary)
		var adjustedKey = new byte[data.Length];
		Array.Copy(key, adjustedKey, Math.Min(key.Length, data.Length));

		// Convert byte array to BigInteger for XOR operation
		var intData = new System.Numerics.BigInteger(data);
		var intKey = new System.Numerics.BigInteger(adjustedKey);

		// Perform XOR operation
		var intEnc = intData ^ intKey;

		// Convert the result back to a byte array
		var result = intEnc.ToByteArray();

		// Ensure the result length matches the original input length
		if (result.Length < data.Length)
		{
			// If result is shorter, pad with leading zeros
			var paddedResult = new byte[data.Length];
			Array.Copy(result, 0, paddedResult, data.Length - result.Length, result.Length);
			return paddedResult;
		}
		else if (result.Length > data.Length)
		{
			// If result is longer, trim the extra bytes
			Array.Resize(ref result, data.Length);
		}

		return result;
	}

	/// <summary>
	/// Get decrypted header
	/// </summary>
	/// <param name="fileContent">File content</param>
	/// <param name="key">Key</param>
	/// <returns>Decrypted header</returns>
	/// <exception cref="ArgumentException">File content is invalid</exception>
	public static byte[] GetDecryptedHeader(byte[] fileContent, byte[] key)
	{
		// Ensure the input fileContent has enough bytes
		if (fileContent.Length < 32)
		{
			throw new ArgumentException("File content must be at least 32 bytes long.");
		}

		// Extract the first 32 bytes for ID and header
		var idBytes = fileContent.Take(16).ToArray();
		var headerBytes = fileContent.Skip(16).Take(16).ToArray();

		// Validate ID against the RPG Maker MV magic number
		if (string.Compare(
			BitConverter.ToString(idBytes).Replace("-", ""),
			RPG_MAKER_MV_MAGIC,
			StringComparison.OrdinalIgnoreCase) != 0)
		{
			throw new ArgumentException("First 16 bytes look wrong on this file", nameof(key));
		}

		return IntXor(headerBytes, key);
	}

	/// <summary>
	/// Update source path
	/// </summary>
	/// <param name="srcPath">Source</param>
	/// <returns>New source path</returns>
	/// <exception cref="ArgumentException">srcPath is invalid</exception>
	public static string UpdateSrcPath(string srcPath)
	{
		var srcInfo = new DirectoryInfo(srcPath);

		if (Directory.Exists(Path.Combine(srcPath, "img")))
		{
			Console.WriteLine("Found 'img' in source path, using parent directory name");
			srcPath = srcInfo.Parent!.Parent!.FullName; // Move two directories up
		}
		else if (Directory.Exists(Path.Combine(srcPath, "www")))
		{
			Console.WriteLine("Found 'www' in source path, using parent directory name");
			srcPath = srcInfo.Parent!.FullName; // Move one directory up
		}
		else
		{
			throw new ArgumentException("Source path is invalid (not contain RPG Maker project)", nameof(srcPath));
		}

		return srcPath;
	}
}
