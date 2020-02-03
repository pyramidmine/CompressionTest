using Snappy;
using System;

namespace Compressor
{
	public class OzoneSnappy : ICompressor
	{
		public byte[] Compress(byte[] data)
		{
			byte[] result = null;
			try
			{
				result = SnappyCodec.Compress(data);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
			}
			return result;
		}

		public byte[] Decompress(byte[] data)
		{
			byte[] result = null;
			try
			{
				result = SnappyCodec.Uncompress(data);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
			}
			return result;
		}
	}
}
