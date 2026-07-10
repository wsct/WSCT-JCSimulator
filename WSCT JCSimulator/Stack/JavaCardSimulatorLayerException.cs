using System.Diagnostics.CodeAnalysis;
using WSCT.JCSimulator.Core;
using WSCT.Stack;

namespace WSCT.JCSimulator.Stack;

/// <summary>
/// Exception thrown when a Java Card Simulator error occurs.
/// </summary>
public class JavaCardSimulatorLayerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaCardSimulatorLayerException"/> class.
    /// </summary>
    public JavaCardSimulatorLayerException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaCardSimulatorLayerException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JavaCardSimulatorLayerException(string message) : base(message)
    {
    }

    /// <summary>
    /// Throws a <see cref="JavaCardSimulatorLayerException"/> if the specified argument is null.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <param name="message">The message to throw with the exception.</param>
    /// <exception cref="JavaCardSimulatorLayerException"></exception>
    public static void ThrowIfNull([NotNull] object? argument, string message)
    {
        if (argument is null)
        {
            throw new JavaCardSimulatorLayerException(message);
        }
    }

    /// <summary>
    /// Throws a <see cref="JavaCardSimulatorLayerException"/> if the <see cref="JcsCardChannelCore"/> instance is null.
    /// </summary>
    /// <param name="argument">The <see cref="JcsCardChannelCore"/> instance to check.</param>
    /// <exception cref="JavaCardSimulatorLayerException"></exception>
    public static void ThrowIfNull([NotNull] JcsCardChannelCore? argument)
    {
        if (argument is null)
        {
            throw new JavaCardSimulatorLayerException("JcsCardChannelCore not initialized: Call Attach(...) first");
        }
    }

    /// <summary>
    /// Throws a <see cref="JavaCardSimulatorLayerException"/> if the <see cref="ICardChannelStack"/> instance is null.
    /// </summary>
    /// <param name="argument">The <see cref="ICardChannelStack"/> instance to check.</param>
    /// <exception cref="JavaCardSimulatorLayerException"></exception>
    public static void ThrowIfNull([NotNull] ICardChannelStack? argument)
    {
        if (argument is null)
        {
            throw new JavaCardSimulatorLayerException("Card channel stack not initialized: Call SetStack(...) first");
        }
    }

    /// <summary>
    /// Throws a <see cref="JavaCardSimulatorLayerException"/> if the <see cref="ICardContextStack"/> instance is null.
    /// </summary>
    /// <param name="argument">The <see cref="ICardContextStack"/> instance to check.</param>
    /// <exception cref="JavaCardSimulatorLayerException"></exception>
    public static void ThrowIfNull([NotNull] ICardContextStack? argument)
    {
        if (argument is null)
        {
            throw new JavaCardSimulatorLayerException("Card context stack not initialized: Call SetStack(...) first");
        }
    }
}
