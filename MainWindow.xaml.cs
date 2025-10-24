using System;
using System.Windows;
using System.Windows.Controls;

namespace LecturerClaimsSystem2
{
    public partial class MainWindow : Window
    {
        private LecturerDashboard lecturerDashboard;
        private AdminDashboard adminDashboard;

        public MainWindow()
        {
            InitializeComponent();

            lecturerDashboard = new LecturerDashboard();
            adminDashboard = new AdminDashboard();

            lecturerDashboard.LogoutRequested += Dashboard_LogoutRequested;
            adminDashboard.LogoutRequested += Dashboard_LogoutRequested;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ErrorText.Text = "Please fill in all fields";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;

            if (role == "Lecturer")
            {
                lecturerDashboard.Initialize(username);

                this.Content = lecturerDashboard;
                this.Title = "Lecturer Claims System - Lecturer Dashboard";
                this.Width = 900;
                this.Height = 600;
            }
            else
            {
                adminDashboard.Initialize(role);
                this.Content = adminDashboard;
                this.Title = $"Lecturer Claims System - {role} Dashboard";
                this.Width = 900;
                this.Height = 600;
            }
        }

        private void Dashboard_LogoutRequested(object sender, EventArgs e)
        {
            this.Content = MainGrid;
            this.Title = "Lecturer Claims System";
            this.Width = 400;
            this.Height = 500;

            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
            RoleComboBox.SelectedIndex = -1;
        }
    }
}
