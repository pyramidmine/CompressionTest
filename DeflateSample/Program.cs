﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Compressor;

namespace CompressBenchmark
{
	class Program
	{
		enum StatColumn
		{
			TrialCount,
			EncodedSize,
			CompressedSize,
			Encoding,
			Compression,
			Decompression,
			Decoding,
			MAX_VALUE
		}

		static readonly long TIME_RESOLUTION = 1000;    // milli seconds
		static readonly long TRIAL_FACTOR =	100;

		static void Main(string[] args)
		{
			Console.WriteLine("Start performance test...");
			Console.WriteLine($"Stopwatch.Frequency: {Stopwatch.Frequency}");

			// 샘플데이터
			SampleData sampleData = new SampleData();

			// 압축기 리스트
			var compressors = new List<ICompressor>()
			{
				new DeflateCompressor(),
				new GZipCompressor(),
//				new SevenZip.Compression.LZMA.SevenZipHelper()	// 압축률은 좋은데 너무 느려서 못쓰겠음
				new IonWikiLZ4(),
				new K4osLZ4(),
				new OzoneSnappy()
			};

			foreach (var compressor in compressors)
			{
				// 통계
				long[,] stat = new long[sampleData.Texts.Count, (int)StatColumn.MAX_VALUE];

				int maxDataSize = sampleData.Texts.Max((text) => text.Length);
				Stopwatch stopwatch = new Stopwatch();

				for (int i = 0; i < sampleData.Texts.Count; i++)
				{
					Console.WriteLine($"Sample data index: {i}");

					// 수행횟수 계산
					double basicCount = (double)maxDataSize / sampleData.Texts[i].Length;
					double scaledCount = Math.Sqrt(basicCount);
					double scaledFactor = Math.Max(1.0d, Math.Log(basicCount));

					stat[i, (int)StatColumn.TrialCount] = (long)(scaledCount * scaledFactor * scaledFactor * TRIAL_FACTOR);
					bool equality = false;

					for (int j = 0; j < stat[i, (int)StatColumn.TrialCount]; j++)
					{
						// 인코딩
						stopwatch.Restart();
						byte[] encodedData = Encoding.UTF8.GetBytes(sampleData.Texts[i]);
						stopwatch.Stop();
						stat[i, (int)StatColumn.Encoding] += stopwatch.ElapsedTicks;
						stat[i, (int)StatColumn.EncodedSize] += encodedData.Length;

						// 압축
						stopwatch.Restart();
						byte[] compressedData = compressor.Compress(encodedData);
						stopwatch.Stop();
						stat[i, (int)StatColumn.Compression] += stopwatch.ElapsedTicks;
						stat[i, (int)StatColumn.CompressedSize] += compressedData.Length;

						// 압축해제
						stopwatch.Restart();
						byte[] decompressedData = compressor.Decompress(compressedData);
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
								return;
							}
						}
					}
				}

				// 스탯 출력
				Console.WriteLine("---------- " + compressor.GetType().Name + "Statistics ----------");
				Console.WriteLine("Original Length, Encoded Size, Compressed Size, Compression Ratio, Trial Count, Encoding Time, Compression Time, Decompression Time, Decoding Time");
				for (int i = 0; i < sampleData.Texts.Count; i++)
				{
					Console.WriteLine(string.Format(@"{0}, {1}, {2}, ,{3}, {4:F4}, {5:F4}, {6:F4}, {7:F4}",
						sampleData.Texts[i].Length,
						stat[i, (int)StatColumn.EncodedSize] / stat[i, (int)StatColumn.TrialCount],
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
}
