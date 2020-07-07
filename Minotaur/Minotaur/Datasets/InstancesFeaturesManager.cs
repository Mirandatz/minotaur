namespace Minotaur.Datasets {
	using System;

	public sealed class InstancesFeaturesManager {

		public readonly int InstanceCount;
		public readonly int FeatureCount;
		private readonly InstanceFeatures[] _instanceFeatures;

		// Constructors and alike
		public InstancesFeaturesManager() {
			_instanceFeatures = null!;
			throw new NotImplementedException();
		}

		public static InstancesFeaturesManager Create(ReadOnlySpan<InstanceFeatures> instancesFeatures) => throw new NotImplementedException();

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
