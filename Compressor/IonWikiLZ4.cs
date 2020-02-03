using System;
using System.IO;

namespace Compressor
{
	public class IonWikiLZ4 : ICompressor
	{
		readonly int DECOMPRESS_BUFFER_SIZE_FACTOR = 3;

		public byte[] Compress(byte[] data)
		{
			byte[] result = null;
			using (MemoryStream ms = new MemoryStream(data.Length))
			{
				using (lz4.LZ4Stream lz4Stream = lz4.LZ4Stream.CreateCompressor(ms, lz4.LZ4StreamMode.Write, lz4.LZ4FrameBlockMode.Linked, lz4.LZ4FrameBlockSize.Max64KB, lz4.LZ4FrameChecksumMode.Content))
				{
					lz4Stream.Write(data, 0, data.Length);
				}
				ms.Flush();
				result = ms.ToArray();
			}
			return result;
		}

		public byte[] Decompress(byte[] data)
		{
			byte[] result = null;
			try
			{
				using (MemoryStream ms = new MemoryStream(data.Length * DECOMPRESS_BUFFER_SIZE_FACTOR))
				{
					using (lz4.LZ4Stream lz4Stream = lz4.LZ4Stream.CreateDecompressor(ms, lz4.LZ4StreamMode.Write))
					{
						lz4Stream.Write(data, 0, data.Length);
					}
					ms.Flush();
					result = ms.ToArray();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
			}
			return result;
		}
	}
}
