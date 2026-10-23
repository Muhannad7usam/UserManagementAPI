using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Validation;

public static class UserValidator
{
    public static Dictionary<string, string[]> Validate(
        string? name, string? email, string? department)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Name is required."];
        else if (name.Trim().Length > 100)
            errors["name"] = ["Name must be at most 100 characters."];

        if (string.IsNullOrWhiteSpace(email))
            errors["email"] = ["Email is required."];
        else if (email.Trim().Length > 254)
            errors["email"] = ["Email must be at most 254 characters."];
        else if (!new EmailAddressAttribute().IsValid(email.Trim()))
            errors["email"] = ["Email must have a valid email address format."];

        if (string.IsNullOrWhiteSpace(department))
            errors["department"] = ["Department is required."];
        else if (department.Trim().Length > 80)
            errors["department"] = ["Department must be at most 80 characters."];

        return errors;
    }
}
