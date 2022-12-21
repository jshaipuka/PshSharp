namespace Psh.TestCase;

public class IntRegTestCases1 : TestCaseGenerator
{
    private static readonly int _testCaseCount = 200;

    private static readonly int _firstSample = -99;

    private static readonly int _stepSize = 1;

    private int[] _testCasesX;

    private int[] _testCasesY;

    public override int TestCaseCount()
    {
        return _testCaseCount;
    }

    public override ObjectPair TestCase(int inIndex)
    {
        if (_testCasesX == null)
        {
            _testCasesX = new int[_testCaseCount];
            _testCasesY = new int[_testCaseCount];
            for (var i = 0; i < _testCaseCount; i++)
            {
                _testCasesX[i] = XValue(i);
                _testCasesY[i] = TestCaseFunction(_testCasesX[i]);
            }
        }

        return new ObjectPair(_testCasesX[inIndex], _testCasesY[inIndex]);
    }

    private int XValue(int i)
    {
        return _firstSample + _stepSize * i;
    }

    private int TestCaseFunction(int x)
    {
        // y = 9x^3 + 24x^2 - 3x + 10
        return 9 * x * x * x + 24 * x * x - 3 * x + 10;
    }
}