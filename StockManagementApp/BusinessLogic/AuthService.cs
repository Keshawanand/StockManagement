using System.Security.Principal;

namespace StockManagementApp.BusinessLogic;

public class AuthService
{
    public bool ValidateLogin()
    {
        var identity = WindowsIdentity.GetCurrent();
        return identity?.Name is { Length: > 0 };
    }

    public string GetCurrentWindowsUser()
    {
        return WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;
    }
}
