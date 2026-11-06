namespace Belsoft.Api.Models;

// Thrown by entities when a business rule is broken. Messages are written for humans
// and are safe to return to clients; never put internal details or personal data in them.
public sealed class DomainException(string message) : Exception(message);