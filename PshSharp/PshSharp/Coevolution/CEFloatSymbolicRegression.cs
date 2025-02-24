/*
* Copyright 2009-2010 Jon Klein
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Psh.TestCase;

namespace Psh.Coevolution;

/// <summary>
///     This problem class implements symbolic regression for floating point numbers
///     using co-evolved prediction.
/// </summary>
/// <remarks>
///     This problem class implements symbolic regression for floating point numbers
///     using co-evolved prediction. The class must keep track of the amount of
///     effort it takes compared to the effort of the co-evolving predictor
///     population, and use about 95% of the effort. Effort based on the number of
///     evaluation executions thus far, which is tracked by the interpreter.
/// </remarks>
public class CEFloatSymbolicRegression : PushGP
{
    private readonly float _noResultPenalty = 1000f;
    protected internal float _currentInput;

    protected internal long _effort;

    protected internal float _predictorEffortPercent;

    protected internal PredictionGA _predictorGA;

    private bool _success;

    /// <exception cref="System.Exception" />
    protected internal override void InitFromParameters()
    {
        base.InitFromParameters();
        _effort = 0;
        var cases = GetParam("test-cases", true);
        var casesClass = GetParam("test-case-class", true);
        if (cases == null && casesClass == null) throw new Exception("No acceptable test-case parameter.");
        if (casesClass != null)
        {
            // Get test cases from the TestCasesClass.
            var iclass = Type.GetType(casesClass);
            var iObject = Activator.CreateInstance(iclass);
            if (!(iObject is TestCaseGenerator))
                throw new Exception("test-case-class must inherit from class TestCaseGenerator");
            var testCaseGenerator = (TestCaseGenerator)iObject;
            var numTestCases = testCaseGenerator.TestCaseCount();
            for (var i = 0; i < numTestCases; i++)
            {
                var testCase = testCaseGenerator.TestCase(i);
                var @in = testCase._first;
                var @out = testCase._second;
                Print(";; Fitness case #" + i + " input: " + @in + " output: " + @out + "\n");
                _testCases.Add(new GATestCase(@in, @out));
            }
        }
        else
        {
            // Get test cases from test-cases.
            var caselist = new Program(cases);
            for (var i = 0; i < caselist.Size(); i++)
            {
                var p = (Program)caselist.DeepPeek(i);
                if (p.Size() < 2) throw new Exception("Not enough elements for fitness case \"" + p + "\"");
                var @in = Convert.ToSingle(p.DeepPeek(0).ToString());
                var @out = Convert.ToSingle(p.DeepPeek(1).ToString());
                Print(";; Fitness case #" + i + " input: " + @in + " output: " + @out + "\n");
                _testCases.Add(new GATestCase(@in, @out));
            }
        }

        // Create and initialize predictors
        _predictorEffortPercent = GetFloatParam("PREDICTOR-effort-percent", true);
        _predictorGA = PredictionGA.PredictionGAWithParameters(this, GetPredictorParameters(_parameters));
    }

    protected internal override void InitInterpreter(Interpreter inInterpreter)
    {
    }

    /// <exception cref="System.Exception" />
    protected internal override void BeginGeneration()
    {
        //trh Temporary solution, needs to actually use effort info
        if (_generationCount % 2 == 1) _predictorGA.Run(1);
    }

    /// <summary>Evaluates a solution individual using the best predictor so far.</summary>
    protected internal virtual void PredictIndividual(GAIndividual inIndividual, bool duringSimplify)
    {
        var predictor = (FloatRegFitPredictionIndividual)_predictorGA.GetBestPredictor();
        var fitness = predictor.PredictSolutionFitness((PushGPIndividual)inIndividual);
        inIndividual.SetFitness(fitness);
        inIndividual.SetErrors(new List<float>());
    }

    public override float EvaluateTestCase(GAIndividual inIndividual, object inInput, object inOutput)
    {
        _effort++;
        _interpreter.ClearStacks();
        _currentInput = (float)inInput;
        var fstack = _interpreter.FloatStack();
        fstack.Push(_currentInput);
        // Must be included in order to use the input stack.
        _interpreter.InputStack().Push(_currentInput);
        _interpreter.Execute(((PushGPIndividual)inIndividual)._program, _executionLimit);
        var result = fstack.Top();
        // System.out.println( _interpreter + " " + result );
        //trh
        /*
        * System.out.println("\nevaluations according to interpreter " +
        * Interpreter.GetEvaluationExecutions());
        * System.out.println("evaluations according to effort " + _effort);
        */
        // Penalize individual if there is no result on the stack.
        if (fstack.Size() == 0) return _noResultPenalty;
        return result - (float)inOutput;
    }

    protected internal override bool Success()
    {
        if (_success) return true;
        var best = _populations[_currentPopulation][_bestIndividual];
        var predictedFitness = best.GetFitness();
        _predictorGA.EvaluateSolutionIndividual((PushGPIndividual)best);
        _bestMeanFitness = best.GetFitness();
        if (_bestMeanFitness <= 0.1)
        {
            _success = true;
            return true;
        }

        best.SetFitness(predictedFitness);
        return false;
    }

    protected internal override string Report()
    {
        Success();
        // Finds the real fitness of the best individual
        return base.Report();
    }

    /// <exception cref="System.Exception" />
    private Dictionary<string, string> GetPredictorParameters(Dictionary<string, string> parameters)
    {
        var predictorParameters = new Dictionary<string, string>();
        predictorParameters["max-generations"] = int.MaxValue.ToString();
        predictorParameters["problem-class"] = GetParam("PREDICTOR-problem-class");
        predictorParameters["individual-class"] = GetParam("PREDICTOR-individual-class");
        predictorParameters["population-size"] = GetParam("PREDICTOR-population-size");
        predictorParameters["mutation-percent"] = GetParam("PREDICTOR-mutation-percent");
        predictorParameters["crossover-percent"] = GetParam("PREDICTOR-crossover-percent");
        predictorParameters["tournament-size"] = GetParam("PREDICTOR-tournament-size");
        predictorParameters["trivial-geography-radius"] = GetParam("PREDICTOR-trivial-geography-radius");
        predictorParameters["generations-between-trainers"] = GetParam("PREDICTOR-generations-between-trainers");
        predictorParameters["trainer-population-size"] = GetParam("PREDICTOR-trainer-population-size");
        return predictorParameters;
    }

    /// <summary>
    ///     NOTE: This is entirely copied from PushGP, except EvaluateIndividual
    ///     was changed to PredictIndividual, as noted below.
    /// </summary>
    protected internal override void Evaluate()
    {
        float totalFitness = 0;
        _bestMeanFitness = float.MaxValue;
        for (var n = 0; n < _populations[_currentPopulation].Length; n++)
        {
            var i = _populations[_currentPopulation][n];
            PredictIndividual(i, false);
            totalFitness += i.GetFitness();
            if (i.GetFitness() < _bestMeanFitness)
            {
                _bestMeanFitness = i.GetFitness();
                _bestIndividual = n;
                _bestSize = ((PushGPIndividual)i)._program.ProgramSize();
                _bestErrors = i.GetErrors();
            }
        }

        _populationMeanFitness = totalFitness / _populations[_currentPopulation].Length;
    }

    /// <summary>
    ///     NOTE: This is entirely copied from PushGP, except EvaluateIndividual
    ///     was changed to PredictIndividual, as noted below (twice).
    /// </summary>
    protected internal override PushGPIndividual Autosimplify(PushGPIndividual inIndividual, int steps)
    {
        var simplest = (PushGPIndividual)inIndividual.Clone();
        var trial = (PushGPIndividual)inIndividual.Clone();
        PredictIndividual(simplest, true);
        // Changed from EvaluateIndividual
        var bestError = simplest.GetFitness();
        var madeSimpler = false;
        for (var i = 0; i < steps; i++)
        {
            madeSimpler = false;
            float method = Rng.Next(100);
            if (trial._program.ProgramSize() <= 0) break;
            if (method < _simplifyFlattenPercent)
            {
                // Flatten random thing
                var pointIndex = Rng.Next(trial._program.ProgramSize());
                var point = trial._program.Subtree(pointIndex);
                if (point is Program)
                {
                    trial._program.Flatten(pointIndex);
                    madeSimpler = true;
                }
            }
            else
            {
                // Remove small number of random things
                var numberToRemove = Rng.Next(3) + 1;
                for (var j = 0; j < numberToRemove; j++)
                {
                    var trialSize = trial._program.ProgramSize();
                    if (trialSize > 0)
                    {
                        var pointIndex = Rng.Next(trialSize);
                        trial._program.ReplaceSubtree(pointIndex, new Program());
                        trial._program.Flatten(pointIndex);
                        madeSimpler = true;
                    }
                }
            }

            if (madeSimpler)
            {
                PredictIndividual(trial, true);
                // Changed from EvaluateIndividual
                if (trial.GetFitness() <= bestError)
                {
                    simplest = (PushGPIndividual)trial.Clone();
                    bestError = trial.GetFitness();
                }
            }

            trial = (PushGPIndividual)simplest.Clone();
        }

        return simplest;
    }
}