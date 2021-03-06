﻿/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

namespace Arcade
{
    public static class ArrayExtensions
    {
        public static void RotateLeft<T>(this T[] arr, int count)
        {
            if (arr is null || arr.Length < 1)
                return;

            for (int i = 0; i < count; ++i)
            {
                T first = arr[0];
                for (int j = 1; j < arr.Length; ++j)
                    arr[j - 1] = arr[j];
                arr[arr.Length - 1] = first;
            }
        }

        public static void RotateRight<T>(this T[] arr, int count)
        {
            if (arr is null || arr.Length < 1)
                return;

            for (int i = 0; i < count; ++i)
            {
                T last = arr[arr.Length - 1];
                for (int j = arr.Length - 2; j >= 0; --j)
                    arr[j + 1] = arr[j];
                arr[0] = last;
            }
        }
    }
}
