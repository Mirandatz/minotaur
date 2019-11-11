namespace Minotaur.Theseus.Evolution {
	using System;

	public sealed class MaximumNumberOfGenerations: IEvolutionStopper {

		public readonly int Threshold;

		public MaximumNumberOfGenerations(int threshold) {
			if (threshold < 1)
				throw new ArgumentOutOfRangeException(nameof(threshold));

			Threshold = threshold;
		}

		public bool ShouldStopEvolution(GenerationResult generationResult) => generationResult.GenerationNumber >= Threshold;
	}
}
