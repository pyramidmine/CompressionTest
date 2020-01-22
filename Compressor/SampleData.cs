using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Compressor
{
	public class SampleData
	{
		public List<string> Texts { get; private set; } = new List<string>();

		public SampleData()
		{
			string currDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string[] sampleFiles = Directory.GetFiles(currDirectory, "sample.*.txt");
			foreach (var file in sampleFiles)
			{
				this.Texts.Add(File.ReadAllText(file));
			}
		}
	}
}
