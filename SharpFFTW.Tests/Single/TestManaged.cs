﻿
namespace SharpFFTW.Tests.Single
{
    using SharpFFTW;
    using SharpFFTW.Single;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Test managed FFTW interface (1D).
    /// </summary>
    public class TestManaged
    {
        /// <summary>
        /// Run examples.
        /// </summary>
        /// <param name="n">Logical size of the transform.</param>
        public static void Run(int length)
        {
            Console.WriteLine("Starting managed test with FFT size = " + length + " (Type: single)");
            Console.WriteLine();

            try
            {
                Example1(length);
                Example2(length);
                Example3(length);
                Example4(2000, true);
            }
            catch (BadImageFormatException)
            {
                Util.Write("Couldn't load native FFTW image (Type: single)", false);
            }
            catch (Exception e)
            {
                Util.Write(e.Message, false);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Complex to complex transform.
        /// </summary>
        static void Example1(int length)
        {
            Console.Write("Test 1: complex transform ... ");

            // Size is 2 * n because we are dealing with complex numbers.
            int size = 2 * length;

            // Create two managed arrays, possibly misaligned.
            var data = Util.GenerateSignal(size);

            // Copy to native memory.
            using var input = new ComplexArray(data);
            using var output = new ComplexArray(size);

            // Create a managed plan as well.
            using var plan1 = Plan.Create1(length, input, output, Direction.Forward, Options.Estimate);

            plan1.Execute();

            using var plan2 = Plan.Create1(length, output, input, Direction.Backward, Options.Estimate);

            plan2.Execute();

            Array.Clear(data, 0, data.Length);

            // Copy unmanaged output of back-transform to managed array.
            input.CopyTo(data);

            // Check and see how we did.
            Util.CheckResults(length, length, data);
        }

        /// <summary>
        /// Real to complex transform.
        /// </summary>
        static void Example2(int length)
        {
            Console.Write("Test 2: real to complex transform ... ");

            int n = length;

            // Create two managed arrays, possibly misaligned.
            var data = Util.GenerateSignal(n);

            // Copy to native memory.
            using var input = new RealArray(data);
            using var output = new ComplexArray(n / 2 + 1);

            // Create a managed plan.
            using var plan1 = Plan.Create1(n, input, output, Options.Estimate);

            plan1.Execute();

            using var plan2 = Plan.Create1(n, output, input, Options.Estimate | Options.PreserveInput);

            plan2.Execute();

            Array.Clear(data, 0, n);

            // Copy unmanaged output of back-transform to managed array.
            input.CopyTo(data);

            // Check and see how we did.
            Util.CheckResults(n, n, data);
        }

        /// <summary>
        /// Real to half-complex transform.
        /// </summary>
        static void Example3(int length)
        {
            Console.Write("Test 3: real to half-complex transform ... ");

            int n = length;

            // Create two managed arrays, possibly misaligned.
            var data = Util.GenerateSignal(n);

            // Copy to native memory.
            using var input = new RealArray(data);
            using var output = new RealArray(n);

            // Create a managed plan.
            using var plan1 = Plan.Create1(n, input, output, Transform.R2HC, Options.Estimate);

            plan1.Execute();

            using var plan2 = Plan.Create1(n, output, input, Transform.HC2R, Options.Estimate);

            plan2.Execute();

            Array.Clear(data, 0, n);

            // Copy unmanaged output of back-transform to managed array.
            input.CopyTo(data);

            // Check and see how we did.
            Util.CheckResults(n, n, data);
        }

        /// <summary>
        /// Parallel execution.
        /// </summary>
        static void Example4(int tasks, bool print)
        {
            Console.WriteLine("Test 4: parallel real to complex transform ... ");

            Parallel.For(0, tasks, (i, state) =>
            {
                if (print)
                {
                    Console.WriteLine($"{i,5}: current thread = {Thread.CurrentThread.ManagedThreadId}");
                }

                Example4Core(i % 2 == 0 ? 1024 : 2048);
            });
        }

        /// <summary>
        /// Complex to complex transform.
        /// </summary>
        static void Example4Core(int length)
        {
            int n = length;

            // Create two managed arrays, possibly misaligned.
            var data = Util.GenerateSignal(n);

            // Copy to native memory.
            using var input = new RealArray(data);
            using var output = new ComplexArray(n / 2 + 1);

            // Create a managed plan.
            using var plan = Plan.Create1(n, input, output, Options.Estimate);

            plan.Execute();
        }
    }
}