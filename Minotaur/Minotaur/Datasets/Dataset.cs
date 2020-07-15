namespace Minotaur.Datasets {
	using System;

	public sealed class Dataset {

		public readonly InstancesFeaturesManager InstancesFeaturesManager;
		public readonly InstancesLabelsManager InstancesLabelsManager;

		public int InstanceCount => InstancesFeaturesManager.InstanceCount;
		public int FeatureCount => InstancesFeaturesManager.FeatureCount;
		public int ClassCount => InstancesLabelsManager.ClassCount;

		// Constructors and alike
		public Dataset(InstancesFeaturesManager featuresManager, InstancesLabelsManager labelsManager) {
			if (featuresManager.InstanceCount != labelsManager.InstanceCount)
				throw new ArgumentException();

			InstancesFeaturesManager = featuresManager;
			InstancesLabelsManager = labelsManager;
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
