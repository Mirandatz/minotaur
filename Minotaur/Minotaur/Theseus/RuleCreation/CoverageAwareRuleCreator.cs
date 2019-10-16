namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Random = Random.ThreadStaticRandom;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		public Dataset Dataset { get; }
		private readonly SeedFinder _seedSelector;
		private readonly RuleAntecedentHyperRectangleConverter _boxConverter;
		private readonly NonIntersectingRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;
		private readonly InstanceCoveringRuleAntecedentCreator _antecedentCreator;
		private readonly MultiLabelConsequentCreator _consequentCreator;
		private readonly HyperRectangleIntersector _rectangleIntersector;
		private readonly int _targetNumberOfInstancesToCover;

		public CoverageAwareRuleCreator(SeedFinder seedSelector, RuleAntecedentHyperRectangleConverter boxConverter, NonIntersectingRectangleCreator boxCreator, HyperRectangleCoverageComputer coverageComputer, InstanceCoveringRuleAntecedentCreator antecedentCreator, MultiLabelConsequentCreator consequentCreator, HyperRectangleIntersector hyperRectangleIntersector, int targetNumberOfInstancesToCover) {
			_seedSelector = seedSelector;
			_boxConverter = boxConverter;
			_boxCreator = boxCreator;
			_coverageComputer = coverageComputer;
			_antecedentCreator = antecedentCreator;
			_consequentCreator = consequentCreator;
			_targetNumberOfInstancesToCover = targetNumberOfInstancesToCover;
			_rectangleIntersector = hyperRectangleIntersector;
			Dataset = _seedSelector.Dataset;
		}

		public Rule? TryCreateRule(Array<Rule> existingRules) {
			var seedsIndices = _seedSelector.FindSeedsIndices(existingRules);

			if (seedsIndices.IsEmpty)
				return null;

			// This may fail just because we chose a bad seed or even because
			// we chose a bad dimension expansion order
			var seedIndex = Random.Choice(seedsIndices);
			var seed = Dataset.GetInstanceData(seedIndex);

			var boxes = _boxConverter.FromRules(existingRules);

			// @Sanity check
			for (int i = 0; i < boxes.Length; i++) {
				var box = boxes[i];
				if (box.Contains(seed))
					throw new InvalidOperationException();
			}

			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var secureRectangle = _boxCreator.TryCreateLargestNonIntersectingRectangle(
				seedIndex: seedIndex,
				existingHyperRectangles: boxes,
				dimensionExpansionOrder: dimensionExpansionOrder);

			if (secureRectangle is null)
				return null;

			// @Sanity check
			for (int i = 0; i < boxes.Length; i++) {
				var box = boxes[i];
				if (_rectangleIntersector.IntersectsInAllDimension(secureRectangle, box))
					throw new InvalidOperationException();
			}

			var secureRectangleCoverage = _coverageComputer.ComputeCoverage(secureRectangle);
			var coveredInstancesIndices = secureRectangleCoverage.IndicesOfCoveredInstances.ToArray();

			if (coveredInstancesIndices.Length == 0)
				return null;

			var coveredInstancesDistancesToSeed = Dataset.ComputeDistances(
				targetInstanceIndex: seedIndex,
				otherInstancesIndices: coveredInstancesIndices);

			Array.Sort(
				keys: coveredInstancesDistancesToSeed,
				items: coveredInstancesIndices);

			var instancesToCover = Math.Min(_targetNumberOfInstancesToCover, coveredInstancesIndices.Length);
			var relevantInstances = coveredInstancesIndices
				.AsSpan()
				.Slice(start: 0, length: instancesToCover);

			var ruleAntecedent = _antecedentCreator.CreateAntecedent(
				seedIndex: seedIndex,
				nearestInstancesIndices: relevantInstances);

			if (ruleAntecedent is null)
				return null;

			var ruleConsequent = _consequentCreator.CreateConsequent(relevantInstances);

			return new Rule(
				antecedent: ruleAntecedent,
				consequent: ruleConsequent);
		}
	}
}
