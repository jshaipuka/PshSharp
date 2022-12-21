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

namespace Psh.Tests;

[TestFixture]
public class ProgramTest
{
    [Test]
    public void testEquals()
    {
        // Equality testing of nested programs 

        Program p = new(), q = new(), r = new();

        p.Parse("( 1.0 ( TEST 2 ( 3 ) ) )");
        q.Parse("( 1.0 ( TEST 2 ( 3 ) ) )");
        r.Parse("( 2.0 ( TEST 2 ( 3 ) ) )");

        Assert.AreNotEqual(p, r);
        Assert.AreEqual(p, q);
    }

    [Test]
    public void testParse()
    {
        // Parse a program, and then re-parse its string representation.
        // They should be equal.

        Program p = new(), q = new();
        var program = "(1(2) (3) TEST TEST (2 TEST))";

        p.Parse(program);
        q.Parse(p.ToString());

        Assert.AreEqual(p, q);
    }

    [Test]
    public void testSubtreeFetch()
    {
        var p = new Program();
        p.Parse("( 2.0 ( TEST 2 ( 3 ) ) )");
    }

    [Test]
    public void testProgramToStringAndBack()
    {
        var p = new Program("(0 1 0.0 1.0)");
        Assert.AreEqual("(0 1 0.0 1.0)", p.ToString());
        var q = new Program(p.ToString());
        Assert.AreEqual("(0 1 0.0 1.0)", q.ToString());
    }

    [Test]
    public void testSubtreeReplace()
    {
        var p = new Program();
        var q = new Program();

        p.Parse("( 2.0 ( TEST 2 ( 3 ) ) )");

        p.ReplaceSubtree(0, 3);
        p.ReplaceSubtree(2, "TEST2");
        p.ReplaceSubtree(3, new Program("( X )"));

        // Console.Out.WriteLine( p );

        q.Parse("( 3 ( TEST2 ( X ) ( 3 ) ) )");

        Assert.AreEqual(p, q);
    }
}