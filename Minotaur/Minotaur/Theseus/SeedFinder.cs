namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class SeedFinder {
		public readonly Dataset Dataset;
		private readonly RuleAntecedentHyperRectangleConverter _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;

		public SeedFinder(RuleAntecedentHyperRectangleConverter ruleAntecedentHyperRectangleConverter, HyperRectangleCoverageComputer hyperRectangleCoverageComputer) {
			_boxCreator = ruleAntecedentHyperRectangleConverter;
			_coverageComputer = hyperRectangleCoverageComputer;
			Dataset = _boxCreator.Dataset;
		}

		public Array<int> FindSeedsIndices(Array<Rule> existingRules) {
			if (existingRules.ContainsNulls())
				throw new ArgumentException(nameof(existingRules) + " can't contain nulls.");

			if (existingRules.IsEmpty) {
				var instanceCount = Dataset.InstanceCount;
				var indices = new int[instanceCount];
				for (int i = 0; i < indices.Length; i++)
					indices[i] = i;

				return indices;
			}

			var boxes = _boxCreator.FromRules(existingRules);
			var coverages = _coverageComputer.ComputeCoverages(boxes);
			var totalCoverage = DatasetCoverage.CombineCoveragesBinaryOr(coverages);
			return totalCoverage.IndicesOfUncoveredInstances;
		}
	}
}
