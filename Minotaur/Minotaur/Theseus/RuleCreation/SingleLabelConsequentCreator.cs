namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.Collections.Dataset;

	public sealed class SingleLabelConsequentCreator: IConsequentCreator {

		private readonly Dataset _dataset;

		public SingleLabelConsequentCreator(Dataset dataset) {
			_dataset = dataset;
		}

		public Consequent CreateConsequent(ReadOnlySpan<int> indicesOfDatasetInstances) {
			// @Performance

			throw new NotImplementedException();

			//var labels = new int[indicesOfDatasetInstances.Length];

			//for (int i = 0; i < indicesOfDatasetInstances.Length; i++) {
			//	var instanceIndex = indicesOfDatasetInstances[i];
			//	var label = (SingleLabel) Dataset.GetInstanceLabel(instanceIndex: instanceIndex);
			//	labels[i] = label.Value;
			//}

			//Array.Sort(labels);
			//var commonestValue = labels[labels.Length / 2];
			//return new SingleLabel(commonestValue);
		}
	}
}

