using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kontur.GameStats.Tests
{
    internal static class MyAssert
    {
        public static void Throws<T>(Action func) where T : Exception
        {
            var exceptionThrown = false;
            try
            {
                func.Invoke();
            }
            catch (T)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                throw new AssertFailedException(
                    $"An exception of type {typeof(T)} was expected, but not thrown"
                );
            }
        }
    }
}
