﻿namespace BlockchainSharp.Tests.Vm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BlockchainSharp.Vm;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MachineTests
    {
        [TestMethod]
        public void PushByte()
        {
            PushPop(new byte[] { 0x01 });
        }

        [TestMethod]
        public void PushBytes()
        {
            for (int k = 1; k <= 32; k++)
                PushPop(k);
        }

        [TestMethod]
        public void PushAndDup()
        {
            PushDupPop(1);
        }

        [TestMethod]
        public void PushTwiceAndDup()
        {
            PushDupPop(2);
        }

        [TestMethod]
        public void PushThreeValuesAndDup()
        {
            PushDupPop(3);
        }

        [TestMethod]
        public void PushFourValuesAndDup()
        {
            PushDupPop(4);
        }

        [TestMethod]
        public void PushAllValuesAndDup()
        {
            for (uint k = 1; k <= 16; k++)
                PushDupPop(k);
        }

        [TestMethod]
        public void PushByteTwice()
        {
            Machine machine = new Machine();

            machine.Execute(new byte[] { (byte)Bytecodes.Push1, 0x01, (byte)Bytecodes.Push1, 0x02 });

            Assert.AreEqual(machine.Stack.Pop(), DataWord.Two);
            Assert.AreEqual(machine.Stack.Pop(), DataWord.One);
            Assert.AreEqual(0, machine.Stack.Size);
        }

        [TestMethod]
        public void PushTwoBytesTwice()
        {
            Machine machine = new Machine();

            machine.Execute(new byte[] { (byte)Bytecodes.Push2, 0x01, 0x02, (byte)Bytecodes.Push2, 0x03, 0x04 });

            Assert.AreEqual(machine.Stack.Pop(), new DataWord((256 * 3) + 4));
            Assert.AreEqual(machine.Stack.Pop(), new DataWord(256 + 2));
        }

        [TestMethod]
        public void PushThreeBytesTwice()
        {
            Machine machine = new Machine();

            machine.Execute(new byte[] { (byte)Bytecodes.Push3, 0x01, 0x02, 0x03, (byte)Bytecodes.Push3, 0x04, 0x05, 0x06 });

            Assert.AreEqual(machine.Stack.Pop(), new DataWord((256 * 256 * 4) + (256 * 5) + 6));
            Assert.AreEqual(machine.Stack.Pop(), new DataWord((256 * 256 * 1) + (256 * 2) + 3));
            Assert.AreEqual(0, machine.Stack.Size);
        }

        [TestMethod]
        public void PushesAndSwap()
        {
            for (int k = 1; k <= 16; k++)
            {
                Machine machine = PushSwap(k);
                Assert.AreEqual(new DataWord(16), machine.Stack.ElementAt(k));
                Assert.AreEqual(new DataWord(16 - k), machine.Stack.Top());
            }
        }

        [TestMethod]
        public void IsZero()
        {
            BytecodeCompiler compiler = new BytecodeCompiler();

            compiler.Compile(Bytecodes.Push1, 2);
            compiler.Compile(Bytecodes.IsZero);
            compiler.Compile(Bytecodes.Push1, 0);
            compiler.Compile(Bytecodes.IsZero);

            Machine machine = new Machine();

            machine.Execute(compiler.ToBytes());

            var stack = machine.Stack;

            Assert.IsNotNull(stack);
            Assert.AreEqual(2, stack.Size);
            Assert.AreEqual(DataWord.Zero, stack.ElementAt(1));
            Assert.AreEqual(DataWord.One, stack.ElementAt(0));
        }

        private static void PushPop(int times)
        {
            byte[] bytes = new byte[times];

            for (int k = 0; k < times; k++)
                bytes[k] = (byte)(k + 1);

            PushPop(bytes);
        }

        private static void PushPop(byte[] bytes)
        {
            BytecodeCompiler compiler = new BytecodeCompiler();

            compiler.CompileAdjust(Bytecodes.Push1, bytes.Length - 1, bytes);

            Machine machine = new Machine();

            machine.Execute(compiler.ToBytes());

            Assert.AreEqual(1, machine.Stack.Size);
            Assert.AreEqual(new DataWord(bytes), machine.Stack.Pop());
        }

        private static Machine PushSwap(int nswap)
        {
            BytecodeCompiler compiler = new BytecodeCompiler();

            for (byte k = 0; k < 17; k++)
                compiler.Compile(Bytecodes.Push1, k);

            compiler.CompileAdjust(Bytecodes.Swap1, nswap - 1);

            Machine machine = new Machine();

            machine.Execute(compiler.ToBytes());

            return machine;
        }

        private static void PushDupPop(uint times)
        {
            IList<byte> bytes = new List<byte>();

            for (int k = 0; k < times; k++)
            {
                bytes.Add((byte)Bytecodes.Push1);
                bytes.Add((byte)k);
            }

            bytes.Add((byte)(Bytecodes.Dup1 + (int)times - 1));

            Machine machine = new Machine();

            machine.Execute(bytes.ToArray());

            DataWord value = new DataWord(times);

            Assert.AreEqual(DataWord.Zero, machine.Stack.Pop());

            for (int k = 0; k < times; k++)
            {
                value = value.Subtract(DataWord.One);
                Assert.AreEqual(value, machine.Stack.Pop());
            }

            Assert.AreEqual(0, machine.Stack.Size);
        }
    }
}