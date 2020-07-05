namespace Minotaur.Datasets {
	using System;

	public sealed class InstancesLabelsManager {

		public readonly int InstanceCount;
		public readonly int ClassCount;
		private readonly InstanceLabels[] _instanceFeatures;

		public InstancesLabelsManager() {
			throw new NotImplementedException();
		}

		public InstanceLabels GetInstanceLabels(int instanceIndex) {
			if (instanceIndex < 0 || instanceIndex >= InstanceCount)
				throw new ArgumentOutOfRangeException(nameof(instanceIndex));

			return _instanceFeatures[instanceIndex];
		}
	}
}
