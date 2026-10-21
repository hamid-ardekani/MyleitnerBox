using System.Runtime.InteropServices.JavaScript;
using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Users.Exceptions;

public class UserCanNotAddFlashcardToBoxException:ExceptionManager
{
    public UserCanNotAddFlashcardToBoxException() : base(Errors.UserCanNotAddFlashcardToBoxException)
    {
    }
}