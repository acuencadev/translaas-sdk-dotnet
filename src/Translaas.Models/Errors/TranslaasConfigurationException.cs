using System;

namespace Translaas.Models.Errors;

/// <summary>
/// Exception thrown when there is a configuration error with the Translaas SDK.
/// </summary>
public class TranslaasConfigurationException : TranslaasException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TranslaasConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TranslaasConfigurationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
