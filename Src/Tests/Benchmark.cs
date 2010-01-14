﻿using System;
using System.Collections.Generic;
using System.Text;
using Loyc.Runtime;
using System.Threading;

namespace Loyc.Tests
{
	class Benchmark
	{
		[ThreadStatic]
		static int _threadStatic;
		static LocalDataStoreSlot _tlSlot;
		static Dictionary<int, int> _dictById = new Dictionary<int,int>();
		static ThreadLocalVariable<int> _dict = new ThreadLocalVariable<int>();
		static int _globalVariable = 0;

		public static void ThreadLocalStorage()
		{
			Console.WriteLine("Performance of accessing a thread-local variable 10,000,000 times:");
			SimpleTimer t = new SimpleTimer();
			const int Iterations = 10000000;
			
			// Baseline comparison
			for (int i = 0; i < Iterations; i++)
				_globalVariable += i;
			Console.WriteLine("    Non-thread-local global variable: {0}ms", t.Restart());

			// ThreadStatic attribute
			t = new SimpleTimer();
			for (int i = 0; i < Iterations; i++)
			{
				// In CLR 2.0, this is the same performance-wise as two separate 
				// operations (a read and a write)
				_threadStatic += i;
			}
			int time = t.Restart();
			for (int i = 0; i < Iterations; i++)
			{
				_globalVariable += _threadStatic;
			}
			int time2 = t.Restart();
			Console.WriteLine("    ThreadStatic variable: {0}ms (read-only: {1}ms)", time, time2);

			// ThreadLocalVariable<int>
			_dict.Value = 0;
			for (int i = 0; i < Iterations; i++)
				_dict.Value += i;
			time = t.Restart();
			for (int i = 0; i < Iterations; i++)
				_globalVariable += _dict.Value;
			time2 = t.Restart();
			Console.WriteLine("    ThreadLocalVariable: {0}ms (read-only: {1}ms)", time, time2);

			// Dictionary indexed by thread ID
			_dictById[Thread.CurrentThread.ManagedThreadId] = 0;
			for (int i = 0; i < Iterations; i++)
			{
				lock (_dictById)
				{
					_dictById[Thread.CurrentThread.ManagedThreadId] += i;
				}
			}
			time = t.Restart();
			// Calling Thread.CurrentThread.ManagedThreadId
			for (int i = 0; i < Iterations; i++)
				_globalVariable += Thread.CurrentThread.ManagedThreadId;
			time2 = t.Restart();
			Console.WriteLine("    Dictionary: {0}ms ({1}ms getting the current Thread ID)", time, time2);

			// Thread Data Slot: slow, so extrapolate from 1/5 the work
			_tlSlot = Thread.AllocateDataSlot();
			Thread.SetData(_tlSlot, 0);
			t.Restart();
			for (int i = 0; i < Iterations/5; i++)
				Thread.SetData(_tlSlot, (int)Thread.GetData(_tlSlot) + i);
			time = t.Restart() * 5;
			Console.WriteLine("    Thread-local data slot: {0}ms (extrapolated)", time);
		}
		
		/// This benchmark is for the sake of JPTrie, which encodes keys in a byte
		/// array. Often, it needs to do operations that operate on 4 bytes at a
		/// time, so in this benchmark I attempt to do the same operations 1 byte at
		/// a time and 4 bytes at a time, to compare the difference.
		/// 
		/// Note that the arrays are small and therefore likely to be in L1 cache.
		/// Consequently, any inefficiency in the code produced by JIT tends to be
		/// obvious in this benchmark, as it is not hidden behind memory latency.
		/// 
		/// Unfortunately, these benchmarks suggest that it is impossible to
		/// reach the theoretical optimum speed using managed code. On my machine, 
		/// Array.Copy can copy 256 bytes 2,000,000 times in 150 ms. Array.Copy 
		/// requires 9 times as long to do the same operation 16 bytes at a time, 
		/// which suggests that Array.Copy has a very high call overhead and should 
		/// not be used to copy small arrays. About half of the 150 ms is probably 
		/// overhead, so the theoretical optimum speed must be under 100 ms.
		/// 
		/// If you don't use loop unrolling, the fastest equivalent with C# code 
		/// is to copy 32 bits at a time with a pair of pinned pointers, and this 
		/// takes 375 ms.
		/// 
		/// Strangely, you don't gain any performance by using pointers if you
		/// access the array one byte at a time. To the contrary, using the managed
		/// byte array directly is generally faster than using a pointer, unless you
		/// access the array 32 bits at a time. This is odd since using pointers
		/// eliminates array bounds checking.
		/// 
		/// Also very strangely, reading from the array (and summing up the
		/// elements) tends to be slower than writing to the array (writing the
		/// array index into each element).
		/// 
		/// Most strangely of all, the benchmark shows that copying from one array 
		/// to another is generally faster than simply totalling up the bytes in a 
		/// single array.
		/// 
		/// Another notable result is that pinning an array seems to be a cheap
		/// operation. One copying test pins the arrays for every 4 bytes copied,
		/// and this only takes around 500 ms (it varies), versus ~350 ms if the
		/// array is pinned once per 256 bytes, and 800 ms when copying one byte at
		/// a time with verifiable code (copy test #1).
		/// 
		/// Now I just tried the same benchmark on an older laptop and pinning was
		/// not quite so cheap there--2328 ms with repeated pinning vs 1015 ms
		/// without--but it's still slightly cheaper than copying the array one byte
		/// at a time, which takes 2483 ms.
		/// 
		/// There is one operation that can be done fast in managed code with
		/// pointers: a 32-bit fill in which the same value, or a counter, is
		/// written to each element. This is about the same speed as the 256-byte
		/// Array.Copy operation.
		/// 
		/// The rather odd results prompted me to check out the assembly code. I
		/// did this by running the Release build of the benchmark outside the
		/// debugger to obtain optimized code, then attaching the VS Pro debugger,
		/// tracing into the benchmark, and viewing disassembly. Note: on my main
		/// machine I have .NET 3.5 SP1 installed, and it made no difference to 
		/// performance whether the project was set to use .NET 2.0 or 3.5.
		/// 
		/// The good news is, the assembly is faster than you'd expect from a
		/// typical C compiler's debug build. The bad news is, it's distinctly worse 
		/// than you would expect from a C compiler's Release build.
		///
		/// Notably, in these tests, the JIT sometimes makes poor use of x86's 
		/// limited registers. In read test #1, for example, it stores the inner 
		/// loop counter on the stack instead of in a register. Yet ebx, ecx, and 
		/// edi are left unused in both the inner and outer loops. Also, the JIT 
		/// will sometimes unnecessarily copy values between eax and edx, 
		/// effectively wasting one of those registers. Also, it does not cache an 
		/// array's length in a register (yet this seems to have a minimal 
		/// performance impact).
		/// 
		/// I was surprised to learn that "pinning" the array did not actually cause
		/// any special code to be generated--the machine code did not, for
		/// instance, place a special flag in the array to mark it as pinned.
		/// Perhaps this was not so surprising, since "normal" managed code doesn't
		/// have to do anything special to access an array either. As I recall,
		/// there is metadata associated with the assembly code that informs the GC
		/// about which registers contain pointers and when, so that the GC knows
		/// which registers to change during a GC. The fixed statement, then,
		/// probably just produces some metadata that marks whatever object the
		/// pointer points to as unmovable. This is a good design, as it makes the
		/// 'fixed' statement almost free as long as there is no garbage collection.
		/// Therefore, if you pin an array for a very short time, your code is 
		/// unlikely to be interrupted for a GC during that time, making the 
		/// operation free. Note that I'm only talking about the "fixed" statement
		/// here, not the Pinned GCHandle, for which I have no benchmark.
		///
		/// But if the fixed statement does not introduce additional instructions,
		/// why does it make the code slower? Well, it doesn't ALWAYS make the code
		/// slower. The JIT will produce significantly different code depending on
		/// which of several equivalent implementations you use. The benchmark shows
		/// that reading the array with the traditional advancing pointer technique
		///
		///     while (left-- > 0) 
		///         total += *p++;
		///
		/// is slightly slower than the for-loop alternative:
		/// 
		///     for (int B = 0; B &lt; array1.Length; B++) 
		///         total2 += p[B];
		///
		/// and this latter version outperforms the "normal" version (read test #1)
		/// slightly. Since this is only because of how the code is generated, the
		/// first version could easily be faster on some other JIT or processor
		/// architecture.
		/// 
		/// But why is the code slower when it uses a "fixed" statement in the inner
		/// loop (copy test #4)? I see two reasons. Firstly, the "fixed" statement
		/// itself performs a range check on the array index. Secondly, the pointers
		/// p1 and p2 are stored on the stack and then, Lord only knows why, the JIT
		/// reads them back in from the stack as dw1 and dw2, instead of caching the
		/// pointers in registers. Then, for good measure, dw2 is written back to a
		/// different slot on the stack, even though it is never read back in again.
		/// Basically, the JIT is being stupid. If Microsoft makes it smarter (maybe
		/// CLR 4.0 is better?), this code will magically become faster.
		/// 
		/// Why is reading the array (totalling up the values) so much slower than
		/// writing or copying? I compared the assembly code for the two standard,
		/// managed loops, and the only obvious problem is that the JIT doesn't hold
		/// the total in a register, but adds directly to the variable's slot on the
		/// stack. But this doesn't seem like it's enough to explain the difference.
		/// After all, the JIT generates extra code and a memory access for the
		/// array bounds check in write test #1, yet this has only a slight
		/// performance impact. I'm guessing that read loop somehow stalls the CPU
		/// pipelines on both of my test processors (an Intel Core 2 Duo and an AMD
		/// Turion) twice as much as the write loop does.
		/// 
		/// I figured that the read-modify-write memory operation might cause some
		/// stalling, so I temporarily changed "total +=" to simply "total =" in 
		/// read tests #1, #2 and #2b. Sure enough, tests #1 and #2b nearly doubled
		/// in speed--but the speed of test #2 did not change at all. In read test
		/// #2, the JIT holds the loop counter on the stack; presumably, this
		/// stalls the processor in the same way the "+=" operation does.
		///
		/// I thought perhaps the processor would be better able to handle an
		/// unrolled loop, so I added read test #5 and write test #5, which do 
		/// twice as much work per iteration. On the Intel Core 2 Duo, it reduced
		/// the run-time of both loops by about 1/3 compared to read and write 
		/// test #3; and on the AMD Turion, I observed a more modest improvement.
		/// So, if you need to squeeze a little more performance from your 
		/// performance-critical C# code without resorting to a native C++ DLL, 
		/// consider loop unrolling.
		/// 
		/// Or, simply fiddle with the loop structure, sometimes that helps too! 
		/// I mean, write test #3b takes 30% less time than #3 by using an 
		/// advancing pointer... even though read test #2b, which uses an 
		/// advancing pointer, is slower than test #2, which does not. Or, just
		/// wait for Microsoft to improve the JIT, and ... um ... force all your 
		/// users to upgrade.
		public unsafe static void ByteArrayAccess()
		{
			byte[] array1 = new byte[256];
			byte[] array2 = new byte[256];
			int total1 = 0, total2 = 0, total3 = 0;
			const int Iterations = 2000000;

			SimpleTimer t = new SimpleTimer();
			// Write test #1
			for (int i = 0; i < Iterations; i++)
				for (int B = 0; B < 256; B++)
					array1[B] = (byte)B;
			int time1 = t.Restart();

			// Write test #1b
			for (int i = 0; i < Iterations; i++)
				for (int B = 0; B < array2.Length; B++)
					array2[B] = (byte)B;
			int time1b = t.Restart();

			// Write test #2
			for (int i = 0; i < Iterations; i++) {
				fixed (byte* p = array1) {
					byte* p2 = p; // compiler won't let p change

					// Fill the byte array with an advancing pointer
					int left = array1.Length;
					for (int B = 0; left-- > 0; B++)
						*p2++ = (byte)B;
				}
			}
			int time2 = t.Restart();

			// Write test #2b
			for (int i = 0; i < Iterations; i++) {
				fixed (byte* p = array1) {
					int length = array1.Length;
					for (int B = 0; B < length; B++)
						p[B] = (byte)B;
				}
			}
			int time2b = t.Restart();

			// Write test #3
			for (int i = 0; i < Iterations; i++)
			{
				fixed (byte* p = array1)
				{
					// Do effectively the same thing as the first two loops (on a 
					// little-endian machine), but 32 bits at once
					uint* p2 = (uint*)p;
					int length2 = array1.Length >> 2;
					for (int dw = 0; dw < length2; dw++)
					{
						uint B = (uint)dw << 2;
						p2[dw] = B | ((B + 1) << 8) | ((B + 2) << 16) | ((B + 3) << 24);
					}
				}
			}
			int time3 = t.Restart();

			// Write test #3b
			for (int i = 0; i < Iterations; i++)
			{
				fixed (byte* p = array1)
				{
					// same as the last test, but with an advancing pointer
					uint* p2 = (uint*)p;
					for (uint B = 0; B < array1.Length; B += 4)
						*p2++ = B | ((B + 1) << 8) | ((B + 2) << 16) | ((B + 3) << 24);
				}
			}
			int time3b = t.Restart();

			// Write test #4
			for (int i = 0; i < Iterations; i++) {
				int left2 = array1.Length >> 2;
				fixed (byte* p = array1) {
					// Fast fill: fill with zeros
					uint* p2 = (uint*)p;
					while (left2-- > 0)
						*p2++ = 0;
				}
			}
			int time4 = t.Restart();

			// Write test #5
			for (int i = 0; i < Iterations; i++) {
				fixed (byte* p = array1) {
					// same as test #3, but unrolled
					uint* p2 = (uint*)p;
					for (uint B = 0; B < array1.Length; B += 4) {
						*p2++ = B | ((B + 1) << 8) | ((B + 2) << 16) | ((B + 3) << 24);
						B += 4;
						*p2++ = B | ((B + 1) << 8) | ((B + 2) << 16) | ((B + 3) << 24);
					}
				}
			}
			int time5 = t.Restart();

			Console.WriteLine("Performance of writing a byte array (256B * 2M):");
			Console.WriteLine("    Standard for loop: {0}ms or {1}ms", time1, time1b);
			Console.WriteLine("    Pinned pointer, one byte at a time: {0}ms", time2);
			Console.WriteLine("    Pinned pointer, 32 bits at a time: {0}ms or {1}ms", time3, time3b);
			Console.WriteLine("    Pinned pointer, 32-bit fast fill: {0}ms", time4);
			Console.WriteLine("    Pinned pointer, 32 bits, unrolled: {0}ms", time5);
			t.Restart();

			// Read test #1
			for (int i = 0; i < Iterations; i++)
				for (int B = 0; B < array1.Length; B++)
					total1 += array1[B];
			time1 = t.Restart();

			// Read test #2
			for (int i = 0; i < Iterations; i++) {
				fixed (byte* p = array1) {
					for (int B = 0; B < array1.Length; B++)
						total2 += p[B];
				}
			}
			time2 = t.Restart();

			// Read test #2b
			int total2b = 0;
			for (int i = 0; i < Iterations; i++)
			{
				int left = array1.Length;
				fixed (byte* p = array1)
				{
					byte* p2 = p;
					while (left-- > 0)
						total2b += *p2++;
				}
			}
			time2b = t.Restart();

			// Read test #3
			for (int i = 0; i < Iterations; i++)
			{
				int left2 = array1.Length >> 2;
				fixed (byte* p = array2)
				{
					uint* p2 = (uint*)p;
					while (left2-- > 0)
					{
						uint v = *p2++;
						total3 += (int)((v >> 24) + (byte)(v >> 16) + (byte)(v >> 8) + (byte)v);
					}
				}
			}
			time3 = t.Restart();

			// Read test #4
			int dummy = 0; // to prevent overoptimization
			for (int i = 0; i < Iterations; i++)
			{
				int left2 = array1.Length >> 2;
				fixed (byte* p = array2)
				{
					uint* p2 = (uint*)p;
					while (left2-- > 0)
						dummy += (int)(*p2++);
				}
			}
			time4 = t.Restart() | (dummy & 1);

			// Read test #5
			int total5 = 0;
			for (int i = 0; i < Iterations; i++) {
				int lengthDW = array1.Length >> 2;
				fixed (byte* p = array2) {
					// same as test #3, but unrolled
					uint* p2 = (uint*)p;
					for (int dw = 0; dw < lengthDW;) {
						uint v = p2[dw++];
						total5 += (int)((v >> 24) + (byte)(v >> 16) + (byte)(v >> 8) + (byte)v);
						v = p2[dw++];
						total5 += (int)((v >> 24) + (byte)(v >> 16) + (byte)(v >> 8) + (byte)v);
					}
				}
			}
			time5 = t.Restart();

			Console.WriteLine("Performance of reading a byte array:");
			Console.WriteLine("    Standard for loop: {0}ms", time1);
			Console.WriteLine("    Pinned pointer, one byte at a time: {0}ms or {1}ms", time2, time2b);
			Console.WriteLine("    Pinned pointer, 32 bits at a time, equivalent: {0}ms", time3);
			Console.WriteLine("    Pinned pointer, 32 bits at a time, sans math: {0}ms", time4);
			Console.WriteLine("    Pinned pointer, 32 bits, unrolled: {0}ms", time5);
			t.Restart();

			if (total1 != total2 || total2 != total3 || total2 != total2b || total3 != total5)
				throw new Exception("bug");

			// Copy test #1
			for (int i = 0; i < Iterations; i++)
			{
				for (int B = 0; B < array1.Length; B++)
					array2[B] = array1[B];
			}
			time1 = t.Restart();

			// Copy test #2
			for (int i = 0; i < Iterations; i++)
			{
				int left = array1.Length;
				fixed (byte* p1_ = array1)
				fixed (byte* p2_ = array2)
				{
					byte* p1 = p1_, p2 = p2_;
					while (left-- > 0)
						*p1++ = *p2++;
				}
			}
			time2 = t.Restart();

			// Copy test #3
			for (int i = 0; i < Iterations; i++)
			{
				int left2 = array1.Length >> 2;
				fixed (byte* p1 = array1)
				fixed (byte* p2 = array2)
				{
					uint* dw1 = (uint*)p1;
					uint* dw2 = (uint*)p2;
					while (left2-- > 0)
						*dw1++ = *dw2++;
				}
			}
			time3 = t.Restart();

			// Copy test #4
			for (int i = 0; i < Iterations; i++)
			{
				for (int dw = 0; dw < (array1.Length >> 2); dw++)
				{
					fixed (byte* p1 = &array1[dw << 2])
					fixed (byte* p2 = &array2[dw << 2])
					{
						uint* dw1 = (uint*)p1;
						uint* dw2 = (uint*)p2;
						*dw1 = *dw2;
					}
				}
			}
			time4 = t.Restart();

			// Copy test #5
			for (int i = 0; i < Iterations; i++)
				Array.Copy(array1, array2, array1.Length);
			time5 = t.Restart();

			// Copy test #5b
			for (int i = 0; i < Iterations; i++)
				Array.Copy(array1, 0, array2, 0, array1.Length);
			int time5b = t.Restart();

			// Copy test #6
			for (int i = 0; i < Iterations; i++)
				for (int B = 0; B < array1.Length; B += 16)
					Array.Copy(array1, B, array2, B, 16);
			int time6 = t.Restart();

			Console.WriteLine("Performance of copying a byte array:");
			Console.WriteLine("    Standard for loop: {0}ms", time1);
			Console.WriteLine("    Pinned pointer, one byte at a time: {0}ms", time2);
			Console.WriteLine("    Pinned pointer, 32 bits at a time: {0}ms", time3);
			Console.WriteLine("    Repeated pinning, 32 bits at a time: {0}ms", time4);
			Console.WriteLine("    Array.Copy, 256 bytes at a time: {0}ms or {1}ms", time5, time5b);
			Console.WriteLine("    Array.Copy, 16 bytes at a time: {0}ms", time6);
		}
	}
}