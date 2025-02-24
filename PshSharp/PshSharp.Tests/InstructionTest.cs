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

namespace Psh;
/*.Tests*/

/**
 * @author robertbaruch
 */
[TestFixture]
public class InstructionTest
{
    // Sets things up before each test.
    [SetUp]
    protected void setUp()
    {
        interpreter = new Interpreter();
        var instructionList = new Program("( )");
        interpreter.randProgram.SetInstructions(interpreter, instructionList);
        istack = new IntStack();
        fstack = new FloatStack();
        bstack = new BooleanStack();
    }

    protected Interpreter interpreter;
    protected IntStack istack;
    protected FloatStack fstack;
    protected BooleanStack bstack;

    [Test]
    public void testNumberName()
    {
        var p = new Program("( 1 false 1.0 0 0.0 x true )");
        interpreter.Execute(p);
        Assert.AreEqual(2, interpreter.IntStack().Count);
        Assert.AreEqual(2, interpreter.FloatStack().Count);
        Assert.AreEqual(1, interpreter.NameStack().Count);
        Assert.AreEqual(2, interpreter.BoolStack().Count);
        Assert.AreEqual(0, interpreter.IntStack().Pop());
        Assert.AreEqual(1, interpreter.IntStack().Pop());
        Assert.AreEqual(0.0, interpreter.FloatStack().Pop()); //,  float.MinValue);
        Assert.AreEqual(1.0, interpreter.FloatStack().Pop()); //, float.MaxValue);
        Assert.AreEqual("x", interpreter.NameStack().Pop());
        Assert.AreEqual(true, interpreter.BoolStack().Pop());
        Assert.AreEqual(false, interpreter.BoolStack().Pop());
    }

    [Test]
    public void testPop()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "boolean.pop integer.pop float.pop )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(2);

        fstack.Push(4.0f);

        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testDup()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "boolean.dup integer.dup float.dup )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(2);
        istack.Push(3);
        istack.Push(3);

        fstack.Push(4.0f);
        fstack.Push(5.0f);
        fstack.Push(5.0f);

        bstack.Push(true);
        bstack.Push(false);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testSwap()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "boolean.swap integer.swap float.swap )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(3);
        istack.Push(2);

        fstack.Push(5.0f);
        fstack.Push(4.0f);

        bstack.Push(false);
        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testRot()
    {
        var p = new Program("( 1 2 3 4.0 5.0 6.0 true false true " +
                            "boolean.rot integer.rot float.rot )");
        interpreter.Execute(p);

        istack.Push(2);
        istack.Push(3);
        istack.Push(1);

        fstack.Push(5.0f);
        fstack.Push(6.0f);
        fstack.Push(4.0f);

        bstack.Push(false);
        bstack.Push(true);
        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testFlush()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "boolean.flush integer.flush float.flush )");
        interpreter.Execute(p);

        Assert.AreEqual(0, interpreter.IntStack().Count);
        Assert.AreEqual(0, interpreter.FloatStack().Count);
        Assert.AreEqual(0, interpreter.BoolStack().Count);
    }

    [Test]
    public void testStackDepth()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "boolean.stackdepth integer.stackdepth float.stackdepth )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(2);
        istack.Push(3);
        istack.Push(2);
        istack.Push(4);
        istack.Push(2);

        Assert.AreEqual(istack, interpreter.IntStack());
    }

    [Test]
    public void testAdd()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "integer.+ float.+ )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(5);

        fstack.Push(9.0f);

        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testSub()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "integer.- float.- )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(-1);

        fstack.Push(-1.0f);

        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testMul()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "integer.* float.* )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(6);

        fstack.Push(20.0f);

        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void StampRepresentation()
    {
        istack.Push(1);
        istack.Push(0);
        // Assert.AreEqual("[1 0]", istack.ToString());
        Assert.AreEqual("[0 1]", istack.ToString());
        Assert.AreEqual(0, istack.Pop());
        Assert.AreEqual(1, istack.Pop());
        // Assert.AreEqual(new [] { 1, 2, 3}, new [] { 1, 0, 4, 5});
    }


    [Test]
    public void testDiv()
    {
        var p = new Program("( 1 2 3 4.0 5.0 true false " +
                            "integer./ float./ )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(0);

        fstack.Push(4.0f / 5.0f);

        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        // Assert.AreEqual(istack[1], interpreter.IntStack()[1]);
        Assert.AreEqual(1, 3 / 2);
        Assert.AreEqual(0, 2 / 3);
        // Assert.AreEqual(3/2, interpreter.IntStack()[1]);
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testMod()
    {
        var p = new Program("( 1 5 3 7.0 5.0 true false " +
                            "integer.% float.% )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(2);

        fstack.Push(2.0f);

        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testEq()
    {
        var p = new Program("( 1 3 3 7.0 5.0 true false " +
                            "integer.= float.= true false boolean.= false false boolean.=)");
        interpreter.Execute(p);

        istack.Push(1);

        bstack.Push(true);
        bstack.Push(false);
        bstack.Push(true);
        bstack.Push(false);
        bstack.Push(false);
        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testLt()
    {
        var p = new Program("( 1 3 3 5.0 6.0 true false " +
                            "integer.< float.< )");
        interpreter.Execute(p);

        istack.Push(1);

        bstack.Push(true);
        bstack.Push(false);
        bstack.Push(false);
        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testGt()
    {
        var p = new Program("( 1 3 3 5.0 6.0 true false " +
                            "integer.> float.> )");
        interpreter.Execute(p);

        istack.Push(1);

        bstack.Push(true);
        bstack.Push(false);
        bstack.Push(false);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testBoolOps()
    {
        var p = new Program("( true false boolean.or " +
                            "true false boolean.and true false boolean.xor true boolean.not )");
        interpreter.Execute(p);

        bstack.Push(true);
        bstack.Push(false);
        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testInputIndex()
    {
        var p = new Program("( 1 input.index 1 input.index 0 input.index " +
                            "0 input.index 2 input.index 2 input.index 1000 input.index -1 input.index)");
        interpreter.InputStack().Push(true);
        interpreter.InputStack().Push(3);
        interpreter.InputStack().Push(2.0f);

        interpreter.Execute(p);

        istack.Push(3);
        istack.Push(3);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        bstack.Push(true);
        bstack.Push(true);
        bstack.Push(true);

        var inputs = new ObjectStack();
        inputs.Push(true);
        inputs.Push(3);
        inputs.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
        Assert.AreEqual(inputs, interpreter.InputStack());
    }

    [Test]
    public void testInputStackDepth()
    {
        var p = new Program("( input.stackdepth )");
        interpreter.InputStack().Push(true);
        interpreter.InputStack().Push(3);
        interpreter.InputStack().Push(2.0f);
        interpreter.InputStack().Push(1.0f);

        interpreter.Execute(p);

        istack.Push(4);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testInputInAll()
    {
        var p = new Program("( input.inall )");
        interpreter.InputStack().Push(true);
        interpreter.InputStack().Push(3);
        interpreter.InputStack().Push(2.0f);
        interpreter.InputStack().Push(1.0f);

        interpreter.Execute(p);

        istack.Push(3);

        fstack.Push(2.0f);
        fstack.Push(1.0f);

        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testInputInAllRev()
    {
        var p = new Program("( input.inallrev )");
        interpreter.InputStack().Push(true);
        interpreter.InputStack().Push(3);
        interpreter.InputStack().Push(2.0f);
        interpreter.InputStack().Push(1.0f);

        interpreter.Execute(p);

        istack.Push(3);

        fstack.Push(1.0f);
        fstack.Push(2.0f);

        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testCodeQuote()
    {
        var p = new Program("( 1 code.quote integer.Pop code.quote code.quote)");
        interpreter.Execute(p);

        istack.Push(1);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
        Assert.AreEqual("code.quote", interpreter.CodeStack().Pop());
        Assert.AreEqual("integer.Pop", interpreter.CodeStack().Pop());
        // Assert.AreEqual(interpreter.GetInstruction("code.quote"), interpreter.CodeStack().Pop());
        // Assert.AreEqual(interpreter.GetInstruction("integer.Pop"), interpreter.CodeStack().Pop());
    }

    [Test]
    public void testCodeEquals()
    {
        var p = new Program("( 1 " +
                            "code.quote integer.Pop code.quote integer.Pop code.= " +
                            "code.quote integer.Pop code.quote integer.+ code.= )");
        interpreter.Execute(p);

        istack.Push(1);

        bstack.Push(true);
        bstack.Push(false);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testExecEquals()
    {
        var p = new Program("( 1 " +
                            "exec.= code.quote integer.Pop " +
                            "exec.= integer.Pop integer.Pop )");
        interpreter.Execute(p);

        istack.Push(1);

        bstack.Push(false);
        bstack.Push(true);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testCodeIf()
    {
        var p = new Program("( 1 2 1.0 2.0 " +
                            "code.quote integer.pop code.quote float.pop true code.if " +
                            "code.quote integer.pop code.quote float.pop false code.if )");
        interpreter.Execute(p);

        istack.Push(1);

        fstack.Push(1.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testExecIf()
    {
        var p = new Program("( 1 2 1.0 2.0 " +
                            "true exec.if integer.pop float.pop " +
                            "false exec.if integer.pop float.pop )");
        interpreter.Execute(p);

        istack.Push(1);

        fstack.Push(1.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testExecDoRange()
    {
        var p = new Program("( 1 3 " +
                            "exec.do*range 2.0 )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(2);
        istack.Push(3);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testExecDoTimes()
    {
        var p = new Program("( 1 3 " +
                            "exec.do*times 2.0 )");
        interpreter.Execute(p);

        istack.Push(1);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testExecDoCount()
    {
        var p = new Program("( 1 3 " +
                            "exec.do*count 2.0 )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(0);
        istack.Push(1);
        istack.Push(2);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testCodeDoRange()
    {
        var p = new Program("( 1 3 " +
                            "code.quote 2.0 code.do*range )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(2);
        istack.Push(3);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testCodeDoTimes()
    {
        var p = new Program("( 1 3 " +
                            "code.quote 2.0 code.do*times )");
        interpreter.Execute(p);

        istack.Push(1);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void testCodeDoCount()
    {
        var p = new Program("( 1 3 " +
                            "code.quote 2.0 code.do*count )");
        interpreter.Execute(p);

        istack.Push(1);
        istack.Push(0);
        istack.Push(1);
        istack.Push(2);

        fstack.Push(2.0f);
        fstack.Push(2.0f);
        fstack.Push(2.0f);

        Assert.AreEqual(istack, interpreter.IntStack());
        Assert.AreEqual(fstack, interpreter.FloatStack());
        Assert.AreEqual(bstack, interpreter.BoolStack());
    }

    [Test]
    public void TestIntegerDefine()
    {
        interpreter.Execute(new Program("(1 x)"));
        Assert.AreEqual(1, interpreter.NameStack().Count);
        Assert.AreEqual(1, interpreter.IntStack().Count);
        interpreter.ClearStacks();
        Assert.AreEqual(0, interpreter.FloatStack().Count);
        Assert.AreEqual(0, interpreter.NameStack().Count);
        Assert.AreEqual(0, interpreter.IntStack().Count);
        interpreter.Execute(new Program("(1 x integer.define x)"), 3);
        Assert.AreEqual(1, interpreter.IntStack().Count);
        Assert.AreEqual(0, interpreter.FloatStack().Count);
        Assert.AreEqual(1, interpreter.NameStack().Count);

        // define "x"
        interpreter.Step(1);
        Assert.AreEqual(0, interpreter.FloatStack().Count);
        Assert.AreEqual(0, interpreter.IntStack().Count);
        Assert.AreEqual(0, interpreter.NameStack().Count);

        // eval "x"
        interpreter.Step(1);
        Assert.AreEqual(0, interpreter.FloatStack().Count);
        Assert.AreEqual(1, interpreter.IntStack().Count);
        Assert.AreEqual(1, interpreter.IntStack().Top());
        Assert.AreEqual(0, interpreter.NameStack().Count);
    }

    [Test]
    public void TestNameEqual()
    {
        interpreter.Execute(new Program("(blah1 blah2 name.=)"));
        Assert.AreEqual(0, interpreter.NameStack().Count);
        Assert.AreEqual(1, interpreter.BoolStack().Count);
        Assert.AreEqual(false, interpreter.BoolStack().Top());
        // Assert.AreEqual(false, true);
    }
}