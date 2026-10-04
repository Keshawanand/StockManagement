namespace StockManagementApp.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.Text = "Stock Management — Login";
        this.Size = new System.Drawing.Size(380, 260);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.AcceptButton = btnLogin;

        lblTitle = new Label
        {
            Text = "Stock Management System",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };

        lblUsername = new Label { Text = "Username:", Location = new Point(20, 70), AutoSize = true };
        txtUsername = new TextBox { Location = new Point(110, 67), Width = 220 };

        lblPassword = new Label { Text = "Password:", Location = new Point(20, 105), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(110, 102), Width = 220, PasswordChar = '●' };

        btnLogin = new Button { Text = "Login", Location = new Point(110, 150), Width = 100, Height = 32 };
        btnCancel = new Button { Text = "Cancel", Location = new Point(225, 150), Width = 100, Height = 32 };

        btnLogin.Click += new EventHandler(btnLogin_Click);
        btnCancel.Click += new EventHandler(btnCancel_Click);

        Controls.AddRange(new Control[] { lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnCancel });
    }
}
