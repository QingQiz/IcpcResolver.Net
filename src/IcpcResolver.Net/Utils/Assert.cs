using System;
using System.Windows;

namespace IcpcResolver.Net.Utils
{
    public static class Assertion
    {
        public static void Assert(bool condition, string message="")
        {
            if (condition) return;

            MessageBox.Show(message, "AssertionError", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new AssertionErrorException();
        }
    }

    public class AssertionErrorException : Exception
    {
    }
}