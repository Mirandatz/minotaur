namespace Minotaur.IO {
	using System.Collections.Generic;
	using Minotaur.Datasets;

	public static class InstancesFeaturesManagerReader {

		public static InstancesFeaturesManager Read(string path) {
			using var streamReader = IOHelper.CreateStreamReader(path: path);
			using var csvReader = IOHelper.CreateCsvReader(streamReader: streamReader);

			var records = new List<InstanceFeatures>();

			while (csvReader.Read()) {
				var rawFieldsValues = csvReader.Context.Record;
				var rec = ParseRecord(rawFieldsValues: rawFieldsValues);
				records.Add(rec);
			}

			return InstancesFeaturesManager.Create(instancesFeatures: records.ToArray());
		}

		private static InstanceFeatures ParseRecord(string[] rawFieldsValues) {
			var featureValues = new float[rawFieldsValues.Length];

			for (int i = 0; i < featureValues.Length; i++) {
				var rawValue = rawFieldsValues[i];
				var parsedValue = float.Parse(rawValue);
				featureValues[i] = parsedValue;
			}

			return new InstanceFeatures(values: featureValues);
		}
	}
}
