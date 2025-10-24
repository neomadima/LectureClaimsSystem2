using LecturerClaimsSystem2.Models;
using LecturerClaimsSystem2.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LecturerClaimsSystem2
{
    public partial class AdminDashboard : UserControl
    {
        public event EventHandler LogoutRequested;

        private readonly ObservableCollection<Claim> pendingClaims = new();
        private readonly ObservableCollection<Claim> approvedClaims = new();
        private string currentAdmin = "Admin";

        public AdminDashboard()
        {
            InitializeComponent();

            ClaimsListView.ItemsSource = pendingClaims;
            ApprovedClaimsListView.ItemsSource = approvedClaims;

            ClaimService.Instance.ClaimAdded += OnClaimAdded;
            ClaimService.Instance.ClaimUpdated += OnClaimUpdated;
        }

        public void Initialize(string role)
        {
            currentAdmin = string.IsNullOrWhiteSpace(role) ? "Admin" : role;
            DashboardTitle.Text = $"{currentAdmin} Dashboard";
            LoadClaimsFromService();
        }

        private void LoadClaimsFromService()
        {
            pendingClaims.Clear();
            approvedClaims.Clear();

            foreach (var c in ClaimService.Instance.GetPendingClaims())
                pendingClaims.Add(c);

            foreach (var c in ClaimService.Instance.GetApprovedClaims())
                approvedClaims.Add(c);
        }

        private void OnClaimAdded(Claim claim)
        {
            if (claim.Status == "Pending")
            {
                Dispatcher.Invoke(() =>
                {
                    if (!pendingClaims.Contains(claim))
                        pendingClaims.Add(claim);
                });
            }
        }

        private void OnClaimUpdated(Claim claim)
        {
            Dispatcher.Invoke(() =>
            {
                if (claim.Status == "Approved")
                {
                    if (!approvedClaims.Contains(claim))
                        approvedClaims.Add(claim);
                    pendingClaims.Remove(claim);
                }
                else if (claim.Status == "Rejected")
                {
                    pendingClaims.Remove(claim);
                }
            });
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Claim claim)
            {
                ClaimService.Instance.ApproveClaim(claim, currentAdmin);
                MessageBox.Show("Claim approved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Claim claim)
            {
                ClaimService.Instance.RejectClaim(claim);
                MessageBox.Show("Claim rejected.", "Rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
            => LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
