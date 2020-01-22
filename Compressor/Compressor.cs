namespace Compressor
{
	public interface ICompressor
	{
		byte[] Compress(byte[] data);
		byte[] Decompress(byte[] data);
	}
}
