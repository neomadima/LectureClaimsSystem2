using System;
using System.Collections.Generic;
using System.Linq;
using LecturerClaimsSystem2.Models;

namespace LecturerClaimsSystem2.Services
{
     public class ClaimService
    {
        private static readonly Lazy<ClaimService> _instance =
            new Lazy<ClaimService>(() => new ClaimService());
        public static ClaimService Instance => _instance.Value;

        public event Action<Claim> ClaimAdded;
        public event Action<Claim> ClaimUpdated;

        private readonly List<Claim> _claims = new();

        private ClaimService() { }

        public void SubmitClaim(Claim claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            _claims.Add(claim);
            ClaimAdded?.Invoke(claim);
        }

        public void AddNewClaim(Claim claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            _claims.Add(claim);

            if (ClaimAdded != null)
            {
                ClaimAdded(claim);
            }
        }

        public void ApproveClaim(Claim claim, string approvedBy)
        {
            if (claim == null)
                return;

            claim.Status = "Approved";
            claim.ApprovedBy = approvedBy;
            ClaimUpdated?.Invoke(claim);
        }

        public void RejectClaim(Claim claim)
        {
            if (claim == null)
                return;

            claim.Status = "Rejected";
            ClaimUpdated?.Invoke(claim);
        }

        public List<Claim> GetClaimsForLecturer(string lecturer)
        {
            return _claims.Where(c => c.Lecturer == lecturer)
                         .OrderByDescending(c => c.Date)
                         .ToList();
        }

        public List<Claim> GetPendingClaims()
        {
            return _claims.Where(c => c.Status == "Pending")
                          .OrderByDescending(c => c.Date)
                          .ToList();
        }

        public List<Claim> GetApprovedClaims()
        {
            return _claims.Where(c => c.Status == "Approved")
                        .OrderByDescending(c => c.Date)
                        .ToList();
        }

        public List<Claim> GetAllClaims()
        {
            return _claims.OrderByDescending(c => c.Date).ToList();
        }

        public Claim GetClaimById(string lecturer, DateTime date)
        {
            return _claims.FirstOrDefault(c => c.Lecturer == lecturer && c.Date == date);
        }

        public int GetTotalClaimCount()
        {
            return _claims.Count;
        }

        public string DebugClaimInfo()
        {
            return $"Total claims: {_claims.Count}, Lecturers: {string.Join(", ", _claims.Select(c => c.Lecturer))}";
        }
    }
}