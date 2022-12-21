namespace Psh.Coevolution;

public class FloatRegFitPrediction : PredictionGA
{
    protected internal override void InitIndividual(GAIndividual inIndividual)
    {
        var i = (FloatRegFitPredictionIndividual)inIndividual;
        var samples = new int[FloatRegFitPredictionIndividual._sampleSize];
        for (var j = 0; j < samples.Length; j++) samples[j] = Rng.Next(_solutionGA._testCases.Count);
        i.SetSampleIndicesAndSolutionGA(_solutionGA, samples);
    }

    protected internal override void EvaluateIndividual(GAIndividual inIndividual)
    {
        var predictor = (FloatRegFitPredictionIndividual)inIndividual;
        var errors = new List<float>();
        for (var i = 0; i < _trainerPopulationSize; i++)
        {
            var predictedError = predictor.PredictSolutionFitness(_trainerPopulation[i]);
            // Error is difference between predictedError and the actual fitness
            // of the trainer.
            var error = Math.Abs(predictedError) - Math.Abs(_trainerPopulation[i].GetFitness());
            errors.Add(error);
        }

        predictor.SetFitness(AbsoluteAverageOfErrors(errors));
        predictor.SetErrors(errors);
    }

    /// <summary>
    ///     Determines the predictor's fitness on a trainer, where the trainer is the
    ///     inInput, and the trainer's actual fitness is inOutput.
    /// </summary>
    /// <remarks>
    ///     Determines the predictor's fitness on a trainer, where the trainer is the
    ///     inInput, and the trainer's actual fitness is inOutput. The fitness of
    ///     the predictor is the absolute error between the prediction and the
    ///     trainer's actual fitness.
    /// </remarks>
    /// <returns>Predictor's fitness (i.e. error) for the given trainer.</returns>
    /// <exception cref="System.Exception"></exception>
    public override float EvaluateTestCase(GAIndividual inIndividual, object inInput, object inOutput)
    {
        var trainer = (PushGPIndividual)inInput;
        var trainerFitness = (float)inOutput;
        var predictedTrainerFitness = ((PredictionGAIndividual)inIndividual).PredictSolutionFitness(trainer);
        return Math.Abs(predictedTrainerFitness - trainerFitness);
    }

    protected internal override void EvaluateTrainerFitnesses()
    {
        foreach (var trainer in _trainerPopulation)
            if (!trainer.FitnessIsSet())
                EvaluateSolutionIndividual(trainer);
    }

    protected internal override void Reproduce()
    {
        var nextPopulation = _currentPopulation == 0 ? 1 : 0;
        for (var n = 0; n < _populations[_currentPopulation].Length; n++)
        {
            float method = Rng.Next(100);
            GAIndividual next;
            if (method < _mutationPercent)
            {
                next = ReproduceByMutation(n);
            }
            else
            {
                if (method < _crossoverPercent + _mutationPercent)
                    next = ReproduceByCrossover(n);
                else
                    next = ReproduceByClone(n);
            }

            // Make sure next isn't already in the population, so that all
            // predictors are unique.
            for (var k = 0; k < n; k++)
                if (((FloatRegFitPredictionIndividual)next).EqualPredictors(_populations[nextPopulation][k]))
                {
                    var index = Rng.Next(FloatRegFitPredictionIndividual._sampleSize);
                    ((FloatRegFitPredictionIndividual)next).SetSampleIndex(index,
                        Rng.Next(_solutionGA._testCases.Count));
                }

            _populations[nextPopulation][n] = next;
        }
    }

    /// <summary>
    ///     Mutates an individual by choosing an index at random and randomizing
    ///     its training point among possible individuals.
    /// </summary>
    protected internal override GAIndividual ReproduceByMutation(int inIndex)
    {
        var i = (FloatRegFitPredictionIndividual)ReproduceByClone(inIndex);
        var index = Rng.Next(FloatRegFitPredictionIndividual._sampleSize);
        i.SetSampleIndex(index, Rng.Next(_solutionGA._testCases.Count));
        return i;
    }

    protected internal override GAIndividual ReproduceByCrossover(int inIndex)
    {
        var a = (FloatRegFitPredictionIndividual)ReproduceByClone(inIndex);
        var b = (FloatRegFitPredictionIndividual)TournamentSelect(_tournamentSize, inIndex);
        // crossoverPoint is the first index of a that will be changed to the
        // gene from b.
        var crossoverPoint = Rng.Next(FloatRegFitPredictionIndividual._sampleSize - 1) + 1;
        for (var i = crossoverPoint; i < FloatRegFitPredictionIndividual._sampleSize; i++)
            a.SetSampleIndex(i, b.GetSampleIndex(i));
        return a;
    }


    protected internal override void EndGeneration()
    {
    }
}