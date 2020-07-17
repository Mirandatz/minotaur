namespace Minotaur.FittestSelection {
	using System;

	public interface IFittestSelector {

		int[] FindIndicesOfFittestIndividuals(ReadOnlySpan<Fitness> fitnesses);
	}
}
