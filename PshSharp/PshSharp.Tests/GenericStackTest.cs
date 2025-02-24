/*
 * Copyright 2009-2010 Jon Klein and Robert Baruch
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
public class GenericStackTest
{
    [Test]
    public void testPushPop()
    {
        var stringStack = new GenericStack<string>();
        var stringListStack = new GenericStack<List<string>>();

        var vect = new List<string>();
        vect.Add("a string in a vector 1");
        vect.Add("another string 2");

        stringStack.Push("value 1");
        stringListStack.Push(vect);

        stringStack.Push("value 2");
        stringListStack.Push(null);

        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual(2, stringListStack.Count);

        Assert.AreEqual("value 2", stringStack.Pop());
        Assert.AreEqual(1, stringStack.Count);
        Assert.AreEqual("value 1", stringStack.Pop());
        Assert.AreEqual(0, stringStack.Count);

        Assert.Null(stringListStack.Pop());
        Assert.AreEqual(vect, stringListStack.Pop());

        Assert.Null(stringStack.Pop());
        Assert.AreEqual(0, stringStack.Count);
    }

    [Test]
    public void testPushAllReverse()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Push("value 1");
        stringStack.Push("value 2");

        var stringStack2 = new GenericStack<string>();

        stringStack.PushAllReverse(stringStack2);

        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual(2, stringStack2.Count);
        Assert.AreEqual("value 1", stringStack2.Pop());
        Assert.AreEqual("value 2", stringStack2.Pop());
    }

    [Test]
    public void testHashCode()
    {
        var stringStack = new List<string>();
        var stringStack2 = new List<string>();

        // System.out.println("stringStack type is " + stringStack.getClass());
        // XXX What!?
        // Assert.True(stringStack == (stringListStack)); // see note in equals
        // Assert.True(stringStack == (stringStack2));

        // In general, collections with the same elements and not equal via hashcode.
        Assert.AreNotEqual(stringStack.GetHashCode(), stringStack2.GetHashCode());
    }

    [Test]
    public void testEquals()
    {
        var stringStack = new GenericStack<string>();
        var stringStack2 = new GenericStack<string>();
        var stringListStack = new GenericStack<List<string>>();

        // System.out.println("stringStack type is " + stringStack.getClass());
        // XXX What!?
        // Assert.True(stringStack == (stringListStack)); // see note in equals
        // Assert.True(stringStack == (stringStack2));

        Assert.AreEqual(stringStack.GetHashCode(), stringStack2.GetHashCode());
        // Assert.AreEqual(stringStack.GetHashCode(), stringListStack.GetHashCode()); // see note in equals
        // XXX C# doesn't do type erasure so this doesn't work anymore. Thankfully.
        Assert.AreNotEqual(stringStack.GetHashCode(), stringListStack.GetHashCode());

        stringStack.Push("value 1");
        Assert.False(stringStack == stringStack2);
        // Assert.False(stringStack == (stringListStack));
        Assert.False(stringStack.GetHashCode() == stringStack2.GetHashCode());

        stringStack2.Push("value 1");
        Assert.True(stringStack.Equals(stringStack2));

        Assert.AreEqual(stringStack.GetHashCode(), stringStack2.GetHashCode());
    }

    [Test]
    public void testDeepPeek()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Push("value 1");
        stringStack.Push("value 2");

        Assert.AreEqual("value 1", stringStack.DeepPeek(0)); // deepest stack
        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual("value 2", stringStack.Top());
        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual("value 2", stringStack.DeepPeek(1));
    }


    [Test]
    public void testPeek()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Push("value 1");
        stringStack.Push("value 2");

        Assert.AreEqual("value 2", stringStack.Peek(0)); // deepest stack
        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual("value 2", stringStack.Top());
        Assert.AreEqual("value 1", stringStack.Peek(1));
    }

    [Test]
    public void testDup()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Dup();
        Assert.AreEqual(0, stringStack.Count);

        stringStack.Push("value 1");
        stringStack.Push("value 2");
        stringStack.Dup();

        Assert.AreEqual(3, stringStack.Count);
        Assert.AreEqual("value 2", stringStack.Peek(0));
        Assert.AreEqual("value 2", stringStack.Peek(1));
        Assert.AreEqual("value 1", stringStack.Peek(2));
    }

    [Test]
    public void testSwap()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Push("value 1");
        stringStack.Swap();
        Assert.AreEqual(1, stringStack.Count);
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));

        stringStack.Push("value 2");
        stringStack.Swap();

        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual("value 1", stringStack.DeepPeek(1));
        Assert.AreEqual("value 2", stringStack.DeepPeek(0));
    }

    [Test]
    public void testRot()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Push("value 1");
        stringStack.Push("value 2");
        stringStack.Rot();

        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual("value 2", stringStack.DeepPeek(1));
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));

        stringStack.Push("value 3");
        stringStack.Push("value 4");
        stringStack.Rot();
        Assert.AreEqual(4, stringStack.Count);
        Assert.AreEqual("value 2", stringStack.DeepPeek(3));
        Assert.AreEqual("value 4", stringStack.DeepPeek(2));
        Assert.AreEqual("value 3", stringStack.DeepPeek(1));
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));
    }

    [Test]
    public void TestReverseIndex()
    {
        var stringStack = new GenericStack<string>();
        stringStack.Push("1");
        stringStack.Push("2");
        stringStack.Push("3");

        Assert.AreEqual(2, stringStack.ReverseIndex(0));
        Assert.AreEqual(1, stringStack.ReverseIndex(1));
        Assert.AreEqual(0, stringStack.ReverseIndex(2));
        Assert.AreEqual(0, stringStack.ReverseIndex(5));
        Assert.AreEqual(2, stringStack.ReverseIndex(-20));
    }

    [Test]
    public void testShove()
    {
        var stringStack = new GenericStack<string>();

        stringStack.Shove("value 1", 0);
        Assert.AreEqual(1, stringStack.Count);
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));
        Assert.AreEqual("value 1", stringStack.Peek(0));
        stringStack.Shove("value 4", 0);
        Assert.AreEqual(2, stringStack.Count);
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));
        Assert.AreEqual("value 4", stringStack.Peek(0));
        stringStack.Pop();

        stringStack.Shove("value 2", 1);
        Assert.AreEqual(2, stringStack.Count);
        // Assert.AreEqual("value 2", stringStack.DeepPeek(0));
        // Assert.AreEqual("value 1", stringStack.DeepPeek(1));
        // Assert.AreEqual("[value 1 value 2]", stringStack.ToString());
        Assert.AreEqual("value 2", stringStack.DeepPeek(1));
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));

        Assert.AreEqual(2, stringStack.Count);
        stringStack.Shove("value 3", 1);
        Assert.AreEqual(3, stringStack.Count);
        // Assert.AreEqual("[value 1 value 3 value 2]", stringStack.ToString());
        Assert.AreEqual("value 2", stringStack.DeepPeek(2));
        Assert.AreEqual("value 3", stringStack.DeepPeek(1));
        Assert.AreEqual("value 1", stringStack.DeepPeek(0));
        Assert.AreEqual("value 2", stringStack.Peek(0));
        Assert.AreEqual("value 3", stringStack.Peek(1));
        Assert.AreEqual("value 1", stringStack.Peek(2));
    }

    [Test]
    public void TestTop()
    {
        var stringStack = new GenericStack<string>();
        stringStack.Push("Hi");
        Assert.AreEqual("Hi", stringStack.Top());
        stringStack.Pop();
        Assert.AreEqual(null, stringStack.Top());
    }
}