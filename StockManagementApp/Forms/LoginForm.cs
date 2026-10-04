using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class LoginForm : Form
{
    private readonly AuthenticationService _authService = new();

    public LoginForm()
    {
        InitializeComponent();
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Please enter your username and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_authService.ValidateUser(username, password, out var role))
        {
            var dashboard = new MainForm();
            dashboard.Tag = new UserSession { Username = username, Role = role };
            dashboard.Show();
            Hide();
        }
        else
        {
            MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }
}

public class UserSession
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
