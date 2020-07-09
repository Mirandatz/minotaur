namespace Minotaur.IO {
	using Minotaur.Datasets;

	public static class DatasetReader {

		public static Dataset Read(string instancesFeaturesPath, string instancesLabelsPath) {
			var ifm = InstancesFeaturesManagerReader.Read(path: instancesFeaturesPath);
			var ilm = InstancesLabelsManagerReader.Read(path: instancesLabelsPath);

			return new Dataset(
				featuresManager: ifm,
				labelsManager: ilm);
		}
	}
}
