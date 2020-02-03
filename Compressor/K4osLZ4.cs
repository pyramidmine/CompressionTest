using System;
using System.IO;

namespace Compressor
{
	public class K4osLZ4 : ICompressor
	{
		public byte[] Compress(byte[] data)
		{
			byte[] result = null;
			using (var input = new MemoryStream(data))
			{
				using (var target = new MemoryStream())
				{
					using (var stream = K4os.Compression.LZ4.Streams.LZ4Stream.Encode(target))
					{
						// input -> stream(Compress -> target)
						input.CopyTo(stream);
					}
					target.Flush();
					result = target.ToArray();
				}
			}
			return result;
		}

		public byte[] Decompress(byte[] data)
		{
			byte[] result = null;
			try
			{
				using (var input = new MemoryStream(data))
				{
					using (var target = new MemoryStream())
					{
						using (var stream = K4os.Compression.LZ4.Streams.LZ4Stream.Decode(input))
						{
							// stream(input -> Decompress) -> target
							stream.CopyTo(target);
						}
						target.Flush();
						result = target.ToArray();
					}
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
