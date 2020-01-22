using System.IO;
using System.IO.Compression;

namespace Compressor
{
	public class GZipCompressor : ICompressor
	{
		public byte[] Compress(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
				{
					gs.Write(data, 0, data.Length);
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
					using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
					{
						gs.CopyTo(decompressedStream);
					}
				}
				return decompressedStream.ToArray();
			}
		}
	}
}
