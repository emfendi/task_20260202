namespace EmployeeContactApi.Domain.Exceptions;

public class DuplicateEmailException : Exception
{
    public IReadOnlyList<string> DuplicateEmails { get; }

    public DuplicateEmailException(IEnumerable<string> duplicateEmails)
        : base($"Duplicate email(s) found: {string.Join(", ", duplicateEmails)}")
    {
        DuplicateEmails = duplicateEmails.ToList();
    }

    public DuplicateEmailException(string email)
        : base($"Duplicate email found: {email}")
    {
        DuplicateEmails = new List<string> { email };
    }
}
