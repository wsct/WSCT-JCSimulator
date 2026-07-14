using System.Diagnostics.CodeAnalysis;

namespace WSCT.JCSimulator.Wrapper;

/// <summary>
/// Exception thrown when a Java Card Simulator error occurs.
/// </summary>
public class JavaCardSimulatorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaCardSimulatorException"/> class.
    /// </summary>
    public JavaCardSimulatorException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaCardSimulatorException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JavaCardSimulatorException(string message) : base(message)
    {
    }

    /// <summary>
    /// Throws a <see cref="JavaCardSimulatorException"/> if the specified argument is null.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <param name="message">The message to throw with the exception.</param>
    /// <exception cref="JavaCardSimulatorException"></exception>
    public static void ThrowIfNull([NotNull] object? argument, string message)
    {
        if (argument is null)
        {
            throw new JavaCardSimulatorException(message);
        }
    }

    /// <summary>
    /// Throws a <see cref="JavaCardSimulatorException"/> if the <see cref="Stream"/> instance is null.
    /// </summary>
    /// <param name="argument">The <see cref="Stream"/> instance to check.</param>
    /// <exception cref="JavaCardSimulatorException"></exception>
    public static void ThrowIfNull([NotNull] Stream? argument)
    {
        if (argument is null)
        {
            throw new JavaCardSimulatorException("A successful JcsClient.Connect() must be called first");
        }
    }
}
