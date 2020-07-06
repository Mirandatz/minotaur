namespace Minotaur.Datasets {
	using System;

	public sealed class InstancesLabelsManager {

		public readonly int InstanceCount;
		public readonly int ClassCount;
		private readonly InstanceLabels[] _instanceLabels;

		public InstancesLabelsManager() {
			_instanceLabels = null!;
			throw new NotImplementedException();
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
