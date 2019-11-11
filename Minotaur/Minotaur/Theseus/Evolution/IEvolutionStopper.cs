namespace Minotaur.Theseus.Evolution {

	public interface IEvolutionStopper {
		bool ShouldStopEvolution(GenerationResult generationResult);
	}
}
