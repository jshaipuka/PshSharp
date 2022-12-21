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

namespace Psh.ProbClass;

/// <summary>This problem class implements symbolic regression for integers.</summary>
/// <remarks>
///     This problem class implements symbolic regression for integers. See also
///     IntSymbolicRegression for integer symbolic regression.
/// </remarks>
public class IntSymbolicRegression : PushGP
{
    protected internal float _noResultPenalty = 1000;

    /// <exception cref="System.Exception" />
    protected internal override void InitFromParameters()
    {
        base.InitFromParameters();
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
                var @in = (int)testCase._first;
                var @out = (int)testCase._second;
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
                var @in = Convert.ToInt32(p.DeepPeek(0).ToString());
                var @out = Convert.ToInt32(p.DeepPeek(1).ToString());
                Print(";; Fitness case #" + i + " input: " + @in + " output: " + @out + "\n");
                _testCases.Add(new GATestCase(@in, @out));
            }
        }
    }

    protected internal override void InitInterpreter(Interpreter inInterpreter)
    {
    }

    public override float EvaluateTestCase(GAIndividual inIndividual, object inInput, object inOutput)
    {
        _interpreter.ClearStacks();
        var currentInput = (int)inInput;
        var stack = _interpreter.IntStack();
        stack.Push(currentInput);
        // Must be included in order to use the input stack.
        _interpreter.InputStack().Push(currentInput);
        _interpreter.Execute(((PushGPIndividual)inIndividual)._program, _executionLimit);
        var result = stack.Top();
        // System.out.println( _interpreter + " " + result );
        // Penalize individual if there is no result on the stack.
        if (stack.Size() == 0) return _noResultPenalty;
        return result - (int)inOutput;
    }
}