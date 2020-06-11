namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Classification;
	using Minotaur.Classification.Rules;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	public sealed class AntecedentCreator {

		private readonly Dataset _dataset;
		private readonly RuleAntecedentHyperRectangleConverter _converter;

		public AntecedentCreator(RuleAntecedentHyperRectangleConverter ruleAntecedentHyperRectangleConverter, Dataset dataset) {
			_converter = ruleAntecedentHyperRectangleConverter;
			_dataset = dataset;
		}

		public Antecedent? CreateAntecedent(int seedIndex, ReadOnlySpan<int> nearestInstancesIndices) {
			var builder = HyperRectangleBuilder.InitializeWithSeed(
				dataset: _dataset,
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

			var featureTests = _converter.FromHyperRectangle(box);
			return new Antecedent(featureTests);
		}

		private void EnlargeToCoverInstance(HyperRectangleBuilder builder, int instanceIndex) {
			var dimensionCount = _dataset.FeatureCount;
			var instance = _dataset.GetInstanceData(instanceIndex);

			for (int i = 0; i < dimensionCount; i++) {
				switch (_dataset.GetFeatureType(i)) {

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
				var possibleValues = _dataset.GetSortedUniqueFeatureValues(featureIndex: dimensionIndex);
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
