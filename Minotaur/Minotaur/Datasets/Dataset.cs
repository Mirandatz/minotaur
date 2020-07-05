namespace Minotaur.Datasets {
	using System;

	public sealed class Dataset {

		public readonly InstancesFeaturesManager InstancesFeaturesManager;
		public readonly InstancesLabelsManager InstancesLabelsManager;

		public Dataset() {
			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
