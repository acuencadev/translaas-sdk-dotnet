using System;

namespace Translaas.Models.Errors;

/// <summary>
/// Base exception for all Translaas API errors.
/// </summary>
public class TranslaasException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TranslaasException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TranslaasException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
