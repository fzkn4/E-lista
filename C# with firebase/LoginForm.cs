using Firebase.Auth; // Still need this for FirebaseAuthException and UserCredential types
using Firebase.Auth.Providers; // Still need this for AuthErrorReason enum
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

// Make sure this namespace matches your project's namespace
namespace C__with_firebase
{
    public partial class LoginForm : Form
    {
        // Remove FirebaseApiKey and _firebaseAuthClient from here

        public LoginForm()
        {
            InitializeComponent();

            // The Firebase client is initialized in the service class now

            registerPanel.Visible = false; // Keep your UI initialization
            // loginPanel.Visible = true; // Make sure your login panel is visible initially
        }

        // Inside your LoginForm class

        private async void login_Click(object sender, EventArgs e)
        {
            string email = loginEmail.Text.Trim(); // Using loginEmail
            string password = loginPass.Text.Trim(); // Using loginPass

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Call the SignInAsync method on the service instance
                var userCredential = await FirebaseAuthenticationService.Instance.SignInAsync(email, password);

                // If successful, userCredential.User contains the authenticated user information
                // Accessing Uid is correct here
                MessageBox.Show($"Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide(); // Hide the login form

            }
            catch (FirebaseAuthException ex)
            {
                // Handle specific Firebase Authentication errors - Keep this UI logic in the form
                string errorMessage = "An error occurred during login.";

                switch (ex.Reason)
                {
                    case AuthErrorReason.InvalidEmailAddress:
                    case AuthErrorReason.UnknownEmailAddress:
                    case AuthErrorReason.UserNotFound:
                        errorMessage = "Invalid email address.";
                        break;
                    case AuthErrorReason.WrongPassword:
                        errorMessage = "Incorrect password.";
                        break;
                    default:
                        errorMessage = $"Login failed: {ex.Message}";
                        break;
                }

                MessageBox.Show(errorMessage, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                MessageBox.Show($"Login Failed: Invalid Email or Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void navLogin_Click(object sender, EventArgs e)
        {
            registerPanel.Visible = false;
            loginPanel.Visible = true;
        }


        private async void register_ClickAsync(object sender, EventArgs e)
        {

            if (registerPass.Text.Trim() != registerConfirmPass.Text.Trim())
            {
                MessageBox.Show("Passwords do not match.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string email = registerEmail.Text.Trim();
            string password = registerPass.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password for registration.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Password Weakness", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Call the RegisterAsync method on the service instance
                var userCredential = await FirebaseAuthenticationService.Instance.RegisterAsync(email, password);

                MessageBox.Show($"Registration successful! User created: {userCredential.User.Uid}. You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                registerEmail.Clear(); // Clear fields
                registerPass.Clear();
                registerConfirmPass.Clear();

                // Optional: After successful registration, you might want to
                // switch back to the login view and prompt the user to log in.
                navLogin_Click(null, null); // Simulate clicking the navLogin button


            }
            catch (FirebaseAuthException ex)
            {
                // Handle specific Firebase Authentication errors - Keep this UI logic in the form
                string errorMessage = "An error occurred during registration.";

                switch (ex.Reason)
                {
                    case AuthErrorReason.EmailExists:
                        errorMessage = "This email address is already in use.";
                        break;
                    case AuthErrorReason.InvalidEmailAddress:
                        errorMessage = "Invalid email address format.";
                        break;
                    case AuthErrorReason.WeakPassword:
                        errorMessage = "Password is too weak. Please choose a stronger password.";
                        break;
                    default:
                        errorMessage = $"Registration failed: {ex.Message}";
                        break;
                }

                MessageBox.Show(errorMessage, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void navRegister_Click(object sender, EventArgs e)
        {
            registerPanel.Visible = true;
            loginPanel.Visible = false;
            // Optional: Clear fields when switching to register view
            registerEmail.Clear();
            registerPass.Clear();
            registerConfirmPass.Clear();
        }

        private void cancelRegister_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mouseHoverRegister(object sender, EventArgs e)
        {
            navRegister.Cursor = Cursors.Hand;

        }

        private void mouseHoverLogin(object sender, EventArgs e)
        {
            navLogin.Cursor = Cursors.Hand;
        }
    }
}