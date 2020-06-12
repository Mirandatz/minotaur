namespace Minotaur.Theseus {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public sealed class CFSBESeedFinder {
		private readonly Dataset _dataset;
		private readonly RuleAntecedentHyperRectangleConverter _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;

		public CFSBESeedFinder(RuleAntecedentHyperRectangleConverter ruleConverter, HyperRectangleCoverageComputer coverageComputer, Dataset dataset) {
			_boxCreator = ruleConverter;
			_coverageComputer = coverageComputer;
			_dataset = dataset;
		}

		public Array<int> FindSeedsIndices(ReadOnlySpan<Rule> existingRules) {
			if (existingRules.IsEmpty) {
				var instanceCount = _dataset.InstanceCount;
				var indices = new int[instanceCount];
				for (int i = 0; i < indices.Length; i++)
					indices[i] = i;

				return indices;
			}

			var boxes = _boxCreator.FromRules(existingRules.AsSpan());
			var coverages = _coverageComputer.ComputeCoverages(boxes);
			var totalCoverage = DatasetCoverage.CombineCoveragesBinaryOr(coverages);
			return totalCoverage.IndicesOfUncoveredInstances;
		}
	}
}
