namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class InstanceCoveringRuleAntecedentCreator {

		public Dataset Dataset { get; }
		private readonly RuleAntecedentHyperRectangleConverter _converter;

		public InstanceCoveringRuleAntecedentCreator(RuleAntecedentHyperRectangleConverter ruleAntecedentHyperRectangleConverter) {
			_converter = ruleAntecedentHyperRectangleConverter;
			Dataset = _converter.Dataset;
		}

		public IFeatureTest[]? CreateAntecedent(int seedIndex, ReadOnlySpan<int> nearestInstancesIndices) {
			var builder = HyperRectangleBuilder.InitializeWithSeed(
				dataset: Dataset,
				seedIndex: seedIndex);

			for (int i = 0; i < nearestInstancesIndices.Length; i++) {
				var index = nearestInstancesIndices[i];
				EnlargeToCoverInstance(
					builder: builder,
					instanceIndex: index);
			}

			var box = builder.TryBuild();
			if (box is null)
				return null;

			return _converter.FromHyperRectangle(box);
		}

		private void EnlargeToCoverInstance(HyperRectangleBuilder builder, int instanceIndex) {
			var dimensionCount = Dataset.FeatureCount;
			var instance = Dataset.GetInstanceData(instanceIndex);

			for (int i = 0; i < dimensionCount; i++) {
				switch (Dataset.GetFeatureType(i)) {

				case FeatureType.Continuous:
				EnlargeContinuousInterval(
					builder: builder,
					instance: instance,
					dimensionIndex: i);
				break;

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}
		}

		private void EnlargeContinuousInterval(HyperRectangleBuilder builder, Array<float> instance, int dimensionIndex) {
			var (Start, End) = builder.GetContinuousDimensionPreview(dimensionIndex);
			var target = instance[dimensionIndex];

			if (target < Start) {
				builder.UpdateContinuousDimensionIntervalStart(
					dimensionIndex: dimensionIndex,
					value: target);
				return;
			}

			if (target >= End) {
				var possibleValues = Dataset.GetSortedUniqueFeatureValues(featureIndex: dimensionIndex);
				var indexOfStart = possibleValues.BinarySearch(target);
				if (indexOfStart == possibleValues.Length - 1) {
					builder.UpdateContinuousDimensionIntervalEnd(
						dimensionIndex: dimensionIndex,
						value: float.PositiveInfinity);
				} else {
					var nextLarger = possibleValues[indexOfStart + 1];
					builder.UpdateContinuousDimensionIntervalEnd(
						dimensionIndex: dimensionIndex,
						value: nextLarger);
				}
			}
		}
	}
}
