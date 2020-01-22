using Compressor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Validator
{
	class Item
	{
		public string Name { get; private set; }
		public ICompressor Compressor { get; private set; }
		public bool Enabled { get; private set; }

		public Item(string name, ICompressor compressor, bool enabled)
		{
			this.Name = name;
			this.Compressor = compressor;
			this.Enabled = enabled;
		}
	}

	class Program
	{
		static List<Item> items = new List<Item>();
		static void Main(string[] args)
		{
			// 샘플 텍스트
			string sampleText = "To succeed in Life, you need two things: Ignorance and Confidence - Mark Twain.";

			// 압축기 및 검증 여부 리스트 작성
			items.Add(new Item("deflater", new DeflateCompressor(), true));
			items.Add(new Item("gzip", new GZipCompressor(), true));

			// 압축기를 순회하면서 검증
			foreach (var item in items)
			{
				if (!item.Enabled)
				{
					continue;
				}

				// 압축기 정보
				Console.WriteLine($"---------- Validate ----------");
				Console.WriteLine($"Compressor: {item.Compressor.GetType().Name}");

				// UTF-8 인코딩 & 압축
				string decodedText = null;
				byte[] encodedData = null;
				byte[] compressedData = null;
				byte[] decompressedData = null;
				try
				{
					encodedData = Encoding.UTF8.GetBytes(sampleText);
					compressedData = item.Compressor.Compress(encodedData);
					decompressedData = item.Compressor.Decompress(compressedData);
					decodedText = Encoding.UTF8.GetString(decompressedData);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
					return;
				}

				// 원본 텍스트 비교
				bool textEquality = string.Compare(sampleText, decodedText) == 0;
				Console.WriteLine($"Compare Text: {textEquality}");

				// 인코딩 데이터 비교
				bool dataEquality = encodedData.SequenceEqual(decompressedData);
				Console.WriteLine($"Compare Data: {dataEquality}");

				//
				// 다른 언어와의 호환성을 체크하기 위해 압축된 데이터를 Base64 인코딩해서 파일에 저장
				// 다른 언어에서는 이 파일을 읽어서 Base64 디코딩 후 압축을 풀어서 잘 풀리는지 확인
				//

				// Base64 인코딩
				String base64EncodedText = Convert.ToBase64String(compressedData);

				// Base64 인코딩 데이터를 텍스트 파일에 저장
				string path = item.Name + ".base64.cs.txt";
				Console.WriteLine($"File Path: {path}");
				Console.WriteLine($"Encoded Base64 Text: {base64EncodedText}");
				File.WriteAllText(path, base64EncodedText);

				// Base64 텍스트 파일 읽기
				string base64DecodedText = File.ReadAllText(path);

				// Base64 텍스트를 바이너리로 변환 (=압축 데이터)
				byte[] base64DecodedData = Convert.FromBase64String(base64DecodedText);

				// Base64 텍스트가 제대로 저장됐는지 확인
				Console.WriteLine($"Decoded Base64 Text: {base64EncodedText}");
				Console.WriteLine($"Compare Base64 Text: {base64EncodedText.Equals(base64DecodedText)}");
				Console.WriteLine($"Compare Base64 Data: {compressedData.SequenceEqual(base64DecodedData)}");

				// 다른 언어 파일 검색
				string currDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				string otherFilePattern = item.Name + ".base64.*.txt";
				List<string> otherCompressedDataFiles = Directory.GetFiles(currDirectory, otherFilePattern).Where(file => !file.Contains(".cs.")).ToList();
				foreach (var file in otherCompressedDataFiles)
				{
					string otherBase64EncodedText = File.ReadAllText(file);
					byte[] otherBase64DecodedData = Convert.FromBase64String(otherBase64EncodedText);
					byte[] otherDecompressedData = item.Compressor.Decompress(otherBase64DecodedData);
					String otherDecodedText = Encoding.UTF8.GetString(otherDecompressedData);
					Console.WriteLine($"Other Language, File: {Path.GetFileName(file)}, Compare Text: {sampleText.Equals(otherDecodedText)}");
				}
			}
		}
	}
}
