using StockManagementApp.Forms;

namespace StockManagementApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }
}