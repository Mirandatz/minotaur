namespace Minotaur.Datasets {
	using System;
	using Minotaur.Classification;

	public sealed class DatasetCoverageComputer {

		private readonly Dataset _dataset;

		public DatasetCoverageComputer(Dataset dataset) {
			_dataset = dataset;
		}

		public DatasetCoverage ComputeCoverage(ConsistentModel model) {
			var ifm = _dataset.InstancesFeaturesManager;
			var instanceIsCovered = new bool[ifm.InstanceCount];

			for (int i = 0; i < instanceIsCovered.Length; i++) {
				var instanceFeatures = ifm.GetFeatures(i);
				var covered = model.Covers(instanceFeatures);
				instanceIsCovered[i] = covered;
			}

			return new DatasetCoverage(instaceIsCoveredMap: instanceIsCovered);
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
