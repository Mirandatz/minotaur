namespace Minotaur.FittestSelection {
	using System;

	public interface IFittestIdentifier {

		int[] FindIndicesOfFittestIndividuals(ReadOnlySpan<Fitness> fitnesses);
	}
}
