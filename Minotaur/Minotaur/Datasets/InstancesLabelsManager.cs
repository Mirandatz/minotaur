namespace Minotaur.Datasets {
	using System;

	public sealed class InstancesLabelsManager {

		public readonly int InstanceCount;
		public readonly int ClassCount;
		private readonly InstanceLabels[] _instanceLabels;

		// Constructors and alike
		private InstancesLabelsManager(int instanceCount, int classCount, InstanceLabels[] instanceLabels) {
			InstanceCount = instanceCount;
			ClassCount = classCount;
			_instanceLabels = instanceLabels;
		}

		public static InstancesLabelsManager Create(ReadOnlySpan<InstanceLabels> instancesLabels) {
			if (instancesLabels.Length == 0)
				throw new ArgumentException(nameof(instancesLabels) + " can't be empty.");

			var expectedClassCount = instancesLabels[0].Count;
			var storage = new InstanceLabels[instancesLabels.Length];

			for (int i = 0; i < instancesLabels.Length; i++) {
				var current = instancesLabels[i];

				if (current is null)
					throw new ArgumentException(nameof(instancesLabels) + " can't contain nulls.");

				if (current.Count != expectedClassCount)
					throw new ArgumentException(nameof(instancesLabels) + $" " +
						$"can't contain instances with different {current.Count}.");

				storage[i] = current;
			}

			return new InstancesLabelsManager(
				instanceCount: storage.Length,
				classCount: expectedClassCount,
				instanceLabels: storage);
		}

		// Views
		public ReadOnlySpan<InstanceLabels> AsSpan() => _instanceLabels;

		// Actual methods
		public InstanceLabels GetInstanceLabels(int instanceIndex) {
			if (instanceIndex < 0 || instanceIndex >= InstanceCount)
				throw new ArgumentOutOfRangeException(nameof(instanceIndex));

			return _instanceLabels[instanceIndex];
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
