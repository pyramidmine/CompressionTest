using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Compressor;

namespace DeflateSample
{
	class Program
	{
		static ICompressor compressor;

		enum StatColumn
		{
			TrialCount,
			CompressedSize,
			Encoding,
			Compression,
			Decompression,
			Decoding,
			MAX_VALUE
		}

		static readonly long TIME_RESOLUTION = 1000;    // milli seconds
		static readonly long TRIAL_FACTOR = 50;		// loop

		static void Main(string[] args)
		{
			Console.WriteLine("Start performance test...");
			Console.WriteLine($"Stopwatch.Frequency: {Stopwatch.Frequency}");

			// 샘플데이터
			SampleData sampleData = new SampleData();

			// 압축기 선택
			Program.compressor = new DeflateCompressor();
			//Program.compressor = new GZipCompressor();

			// 통계
			long[,] stat = new long[sampleData.Texts.Count, (int)StatColumn.MAX_VALUE];

			int maxDataSize = sampleData.Texts.Max((text) => text.Length);
			Stopwatch stopwatch = new Stopwatch();

			for (int i = 0; i < sampleData.Texts.Count; i++)
			{
				Console.WriteLine($"Sample data index: {i}");

				stat[i, (int)StatColumn.TrialCount] = (long)Math.Sqrt(maxDataSize / sampleData.Texts[i].Length) * TRIAL_FACTOR;
				bool equality = false;

				for (int j = 0; j < stat[i, (int)StatColumn.TrialCount]; j++)
				{
					// 인코딩
					stopwatch.Restart();
					byte[] encodedData = Encoding.UTF8.GetBytes(sampleData.Texts[i]);
					stopwatch.Stop();
					stat[i, (int)StatColumn.Encoding] += stopwatch.ElapsedTicks;
					
					// 압축
					stopwatch.Restart();
					byte[] compressedData = Program.compressor.Compress(encodedData);
					stopwatch.Stop();
					stat[i, (int)StatColumn.Compression] += stopwatch.ElapsedTicks;
					stat[i, (int)StatColumn.CompressedSize] += compressedData.Length;

					// 압축해제
					stopwatch.Restart();
					byte[] decompressedData = Program.compressor.Decompress(compressedData);
					stopwatch.Stop();
					stat[i, (int)StatColumn.Decompression] += stopwatch.ElapsedTicks;

					// 디코딩
					stopwatch.Restart();
					string decodedText = Encoding.UTF8.GetString(decompressedData);
					stopwatch.Stop();
					stat[i, (int)StatColumn.Decoding] += stopwatch.ElapsedTicks;

					// 전/후 데이터 비교 (샘플데이터마다 한 번만 수행)
					if (!equality)
					{
						// 원본 데이터 비교
						Console.WriteLine($"Original text length : {sampleData.Texts[i].Length}");
						Console.WriteLine($"Processed text length: {decodedText.Length}");
						equality = string.Compare(sampleData.Texts[i], decodedText) == 0;
						if (!equality)
						{
							Console.WriteLine($"Two texts are mismatch!");
							return;
						}

						// 인코딩 데이터 비교
						Console.WriteLine($"Encoded data length  : {encodedData.Length}");
						Console.WriteLine($"Processed data length: {decompressedData.Length}");
						equality = encodedData.SequenceEqual(decompressedData);
						if (!equality)
						{
							Console.WriteLine($"Two data are mismatch");
						}
					}
				}
			}

			// 스탯 출력
			Console.WriteLine("---------- Statistics ----------");
			Console.WriteLine("Original Length, Compressed Size, Compression Ratio, Trial Count, Encoding Time, Compression Time, Decompression Time, Decoding Time");
			for (int i = 0; i < sampleData.Texts.Count; i++)
			{
				Console.WriteLine(string.Format(@"{0}, {1}, ,{2}, {3:F4}, {4:F4}, {5:F4}, {6:F4}",
					sampleData.Texts[i].Length,
					stat[i, (int)StatColumn.CompressedSize] / stat[i, (int)StatColumn.TrialCount],
					stat[i, (int)StatColumn.TrialCount],
					((double)stat[i, (int)StatColumn.Encoding] / (Stopwatch.Frequency / TIME_RESOLUTION)) / stat[i, (int)StatColumn.TrialCount],
					((double)stat[i, (int)StatColumn.Compression] / (Stopwatch.Frequency / TIME_RESOLUTION)) / stat[i, (int)StatColumn.TrialCount],
					((double)stat[i, (int)StatColumn.Decompression] / (Stopwatch.Frequency / TIME_RESOLUTION)) / stat[i, (int)StatColumn.TrialCount],
					((double)stat[i, (int)StatColumn.Decoding] / (Stopwatch.Frequency / TIME_RESOLUTION)) / stat[i, (int)StatColumn.TrialCount]
				));
			}
		}
	}
}
