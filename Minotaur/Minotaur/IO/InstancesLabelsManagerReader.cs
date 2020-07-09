namespace Minotaur.IO {
	using System;
	using System.Collections.Generic;
	using Minotaur.Datasets;

	public static class InstancesLabelsManagerReader {

		public static InstancesLabelsManager Read(string path) {
			using var streamReader = IOHelper.CreateStreamReader(path: path);
			using var csvReader = IOHelper.CreateCsvReader(streamReader: streamReader);

			var records = new List<InstanceLabels>();

			while (csvReader.Read()) {
				var rawFieldsValues = csvReader.Context.Record;
				var rec = ParseRecord(rawFieldsValues: rawFieldsValues);
				records.Add(rec);
			}

			return InstancesLabelsManager.Create(instancesLabels: records.ToArray());
		}

		private static InstanceLabels ParseRecord(string[] rawFieldsValues) {
			var labels = new bool[rawFieldsValues.Length];

			for (int i = 0; i < rawFieldsValues.Length; i++) {
				var rawValue = rawFieldsValues[i];
				var parsedValue = int.Parse(rawValue);

				labels[i] = parsedValue switch
				{
					0 => false,
					1 => true,
					_ => throw new InvalidOperationException($"Parsing error. Unable to parse {rawValue} as bool.")
				};
			}

			return new InstanceLabels(values: labels);
		}
	}
}
