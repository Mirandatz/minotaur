namespace Minotaur.Datasets {
	using System;

	public sealed class InstancesFeaturesManager {

		public readonly int InstanceCount;
		public readonly int FeatureCount;
		private readonly InstanceFeatures[] _instanceFeatures;

		public InstancesFeaturesManager() {
			throw new NotImplementedException();
		}

		public InstanceFeatures GetInstanceFeatureValues(int instanceIndex) {
			if (instanceIndex < 0 || instanceIndex >= InstanceCount)
				throw new ArgumentOutOfRangeException(nameof(instanceIndex));

			return _instanceFeatures[instanceIndex];
		}
	}
}
