namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Minotaur.Theseus.TestCreation;

	public sealed class InstanceCoveringRuleAntecedentCreator {
		public Dataset Dataset { get; }
		private readonly TestCreator _testCreator;

		public InstanceCoveringRuleAntecedentCreator(TestCreator testCreator) {
			_testCreator = testCreator;
			Dataset = _testCreator.Dataset;
		}

		public IFeatureTest[] CreateAntecedent(Array<float> seed, ReadOnlySpan<int> nearestInstancesIndices) {
			var box = MutableHyperRectangle.FromDatasetInstance(
				seed: seed,
				featureTypes: Dataset.FeatureTypes);

			throw new NotImplementedException();
		}
	}
}