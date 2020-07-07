namespace Minotaur.Datasets {
	using System;
	using System.Collections.Generic;

	public sealed class InstancesFeaturesManager {

		public readonly int InstanceCount;
		public readonly int FeatureCount;
		private readonly InstanceFeatures[] _instanceFeatures;

		// Constructors and alike
		private InstancesFeaturesManager(int instanceCount, int featureCount, InstanceFeatures[] instanceFeatures) {
			InstanceCount = instanceCount;
			FeatureCount = featureCount;
			_instanceFeatures = instanceFeatures;
		}

		public static InstancesFeaturesManager Create(ReadOnlySpan<InstanceFeatures> instancesFeatures) {
			if (instancesFeatures.Length == 0)
				throw new ArgumentException(nameof(instancesFeatures) + " can't be empty.");

			var storage = new InstanceFeatures[instancesFeatures.Length];
			var uniques = new HashSet<InstanceFeatures>(instancesFeatures.Length);
			var expectedFeatureCount = instancesFeatures[0].Count;

			// Checking if all instances have the same number of features
			// and if there are any duplicated instances
			for (int i = 0; i < instancesFeatures.Length; i++) {
				var current = instancesFeatures[i];

				if (current.Count != expectedFeatureCount) {
					throw new ArgumentException(nameof(instancesFeatures) + " " +
						"contains instances with " +
						"different numbers of features.");
				}

				var isUnique = uniques.Add(current);
				if (!isUnique) {
					throw new ArgumentException(nameof(instancesFeatures) + " " +
						"contains duplicated instances.");
				}

				storage[i] = current;
			}

			return new InstancesFeaturesManager(
				instanceCount: storage.Length,
				featureCount: expectedFeatureCount,
				instanceFeatures: storage);
		}

		// Actual methods
		public InstanceFeatures GetFeatures(int instanceIndex) {
			if (instanceIndex < 0 || instanceIndex >= InstanceCount)
				throw new ArgumentOutOfRangeException(nameof(instanceIndex));

			return _instanceFeatures[instanceIndex];
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
