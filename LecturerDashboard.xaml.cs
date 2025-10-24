using LecturerClaimsSystem2.Models;
using LecturerClaimsSystem2.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ClaimModel = LecturerClaimsSystem2.Models.Claim;


namespace LecturerClaimsSystem2
{
    public partial class LecturerDashboard : UserControl
    {
        public event EventHandler LogoutRequested;

        private string uploadedFilePath = string.Empty;
        private string currentLecturer = "Lecturer";
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; 

        public LecturerDashboard()
        {
            InitializeComponent();

            ClaimService.Instance.ClaimAdded += OnClaimAdded;
            ClaimService.Instance.ClaimUpdated += OnClaimUpdated;
        }

        public void Initialize(string lecturerName)
        {
            currentLecturer = string.IsNullOrWhiteSpace(lecturerName)
                ? "Lecturer"
                : lecturerName;

            WelcomeText.Text = $"Welcome, {currentLecturer} - Submit and track your claims";
            RefreshMyClaims();
            UpdateLatestClaimStatus();
            UpdateStatusProgress("Ready");
        }

        private void OnClaimAdded(Claim claim)
        {
            if (claim.Lecturer == currentLecturer)
            {
                Dispatcher.Invoke(() =>
                {
                    RefreshMyClaims();
                    UpdateLatestClaimStatus();
                    UpdateStatusProgress(claim.Status);
                });
            }
        }

        private void OnClaimUpdated(Claim claim)
        {
            if (claim.Lecturer == currentLecturer)
            {
                Dispatcher.Invoke(() =>
                {
                    RefreshMyClaims();
                    UpdateLatestClaimStatus();
                    UpdateStatusProgress(claim.Status);
                });
            }
        }

        private void UpdateStatusProgress(string status)
        {
            switch (status)
            {
                case "Pending":
                    StatusProgressBar.Value = 33;
                    StatusTextBlock.Text = "Pending Review";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.LightYellow;
                    break;
                case "Approved":
                    StatusProgressBar.Value = 100;
                    StatusTextBlock.Text = "Approved ✓";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.LightGreen;
                    break;
                case "Rejected":
                    StatusProgressBar.Value = 0;
                    StatusTextBlock.Text = "Rejected ✗";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.LightCoral;
                    break;
                default:
                    StatusProgressBar.Value = 0;
                    StatusTextBlock.Text = "Ready to submit";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.LightGray;
                    break;
            }
        }

        private void UpdateLatestClaimStatus()
        {
            try
            {
                var claims = ClaimService.Instance.GetClaimsForLecturer(currentLecturer);
                if (claims.Count > 0)
                {
                    var latestClaim = claims[0];
                    LatestClaimStatus.Text = $"Latest: {latestClaim.Date:dd/MM/yyyy} - {latestClaim.Hours} hours - Status: {latestClaim.Status}";

                    if (latestClaim.Status == "Approved")
                        LatestClaimStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
                    else if (latestClaim.Status == "Rejected")
                        LatestClaimStatus.Foreground = System.Windows.Media.Brushes.LightCoral;
                    else
                        LatestClaimStatus.Foreground = System.Windows.Media.Brushes.LightYellow;
                }
                else
                {
                    LatestClaimStatus.Text = "No claims submitted yet";
                    LatestClaimStatus.Foreground = System.Windows.Media.Brushes.LightGray;
                }
            }
            catch (Exception ex)
            {
                LatestClaimStatus.Text = "Error loading latest claim status";
                LatestClaimStatus.Foreground = System.Windows.Media.Brushes.LightCoral;
            }
        }
        

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Documents (*.pdf;*.docx;*.xlsx)|*.pdf;*.docx;*.xlsx",
                    Title = "Select Supporting Document",
                    Multiselect = false
                };

                if (dialog.ShowDialog() == true)
                {
                    FileInfo fileInfo = new FileInfo(dialog.FileName);
                    
                    if (fileInfo.Length > MAX_FILE_SIZE)
                    {
                        MessageBox.Show($"File size exceeds the 10MB limit. Please choose a smaller file.",
                            "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    uploadedFilePath = dialog.FileName;
                    UploadedFileName.Text = System.IO.Path.GetFileName(uploadedFilePath);
                    UploadedFileName.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading file: {ex.Message}",
                    "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(HoursTextBox.Text, out double hours) || hours <= 0)
                {
                    MessageBox.Show("Please enter a valid number greater than 0 for Hours.",
                        "Invalid Hours", MessageBoxButton.OK, MessageBoxImage.Warning);
                    HoursTextBox.Focus();
                    return;
                }

                if (!double.TryParse(RateTextBox.Text, out double rate) || rate <= 0)
                {
                    MessageBox.Show("Please enter a valid number greater than 0 for Rate.",
                        "Invalid Rate", MessageBoxButton.OK, MessageBoxImage.Warning);
                    RateTextBox.Focus();
                    return;
                }

                var claim = new LecturerClaimsSystem2.Models.Claim
                {
                    Lecturer = currentLecturer,
                    Date = DateTime.Now,
                    Hours = hours,
                    Rate = rate,
                    Notes = NotesTextBox.Text,
                    DocumentPath = uploadedFilePath,
                    Status = "Pending",
                    ApprovedBy = string.Empty
                };



                ClaimService.Instance.SubmitClaim(claim);

                ClaimStatusText.Text = $"Claim submitted successfully! Status: {claim.Status}";
                UpdateStatusProgress(claim.Status);

                HoursTextBox.Text = "0";
                RateTextBox.Text = "0";
                NotesTextBox.Clear();
                UploadedFileName.Text = "No file selected";
                UploadedFileName.Foreground = System.Windows.Media.Brushes.LightGray;
                uploadedFilePath = string.Empty;

                RefreshMyClaims();
                UpdateLatestClaimStatus();

                MessageBox.Show("Claim submitted successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting claim: {ex.Message}\n\nPlease try again.",
                    "Submission Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshMyClaims()
        {

            try
            {
                var totalClaims = ClaimService.Instance.GetTotalClaimCount();
                var debugInfo = ClaimService.Instance.DebugClaimInfo();
                System.Diagnostics.Debug.WriteLine($"Debug: {debugInfo}");

                var myClaims = ClaimService.Instance.GetClaimsForLecturer(currentLecturer);
                System.Diagnostics.Debug.WriteLine($"Debug: Found {myClaims.Count} claims for {currentLecturer}");

                MyClaimsListView.ItemsSource = null;
                MyClaimsListView.ItemsSource = myClaims;

                MyClaimsListView.UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading claims: {ex.Message}",
                    "Loading Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
    }
}