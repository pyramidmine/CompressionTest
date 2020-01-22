using System.IO;
using System.IO.Compression;

namespace Compressor
{
	public class DeflateCompressor : ICompressor
	{
		public DeflateCompressor()
		{
		}

		public byte[] Compress(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream(data.Length))
			{
				using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
				{
					ds.Write(data, 0, data.Length);
				}
				return ms.ToArray();
			}
		}

		public byte[] Decompress(byte[] data)
		{
			using (MemoryStream decompressedStream = new MemoryStream())
			{
				using (MemoryStream ms = new MemoryStream(data))
				{
					using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
					{
						ds.CopyTo(decompressedStream);
					}
				}
				return decompressedStream.ToArray();
			}
		}
	}
}
