using System.Security.Authentication;

namespace EventFoto.API.Exceptions;

public class InvalidUserIdException() : AuthenticationException("Invalid User ID. User ID must be a valid GUID.");
