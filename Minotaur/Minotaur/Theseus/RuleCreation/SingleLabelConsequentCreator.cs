namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections.Dataset;

	public sealed class SingleLabelConsequentCreator: IConsequentCreator {

		public Dataset Dataset { get; }

		public SingleLabelConsequentCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public ILabel CreateConsequent(ReadOnlySpan<int> indicesOfDatasetInstances) {
			// @Performance

			var labels = new int[indicesOfDatasetInstances.Length];

			for (int i = 0; i < indicesOfDatasetInstances.Length; i++) {
				var instanceIndex = indicesOfDatasetInstances[i];
				var label = (SingleLabel) Dataset.GetInstanceLabel(instanceIndex: instanceIndex);
				labels[i] = label.Value;
			}

			Array.Sort(labels);
			var commonestValue = labels[labels.Length / 2];
			return new SingleLabel(commonestValue);
		}
	}
}

