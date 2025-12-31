using System;
using System.Collections.Generic;
using System.Net;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class User : IIdentifiable
{
	public User()
	{
		this.CertificationPayments = new HashSet<CertificationPayment>();
		this.PolicyManagerPortabilityRequests = new HashSet<PolicyManagerPortabilityRequest>();
		this.PolicyManagerUserTerms = new HashSet<PolicyManagerUserTerm>();
		this.RefereeAnswers = new HashSet<RefereeAnswer>();
		this.RefereeCertifications = new HashSet<RefereeCertification>();
		this.RefereeLocations = new HashSet<RefereeLocation>();
		this.RefereeTeams = new HashSet<RefereeTeam>();
		this.Roles = new HashSet<Role>();
		this.TestAttempts = new HashSet<TestAttempt>();
		this.TestResults = new HashSet<TestResult>();
		this.Attributes = new HashSet<UserAttribute>();
		this.TournamentManagers = new HashSet<TournamentManager>();
		this.TeamManagers = new HashSet<TeamManager>();
		this.TeamManagersAdded = new HashSet<TeamManager>();
	}

	public long Id { get; set; }
	public string? UniqueId { get; set; } // new field for guid based id
	public string Email { get; set; } = null!;
	public string EncryptedPassword { get; set; } = null!;
	public string? ResetPasswordToken { get; set; }
	public DateTime? ResetPasswordSentAt { get; set; }
	public DateTime? RememberCreatedAt { get; set; }
	public int SignInCount { get; set; }
	public DateTime? CurrentSignInAt { get; set; } // deprecate
	public DateTime? LastSignInAt { get; set; }
	public IPAddress? CurrentSignInIp { get; set; } // deprecate
	public IPAddress? LastSignInIp { get; set; } // deprecate
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? Pronouns { get; set; }
	public bool? ShowPronouns { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public DateTime? SubmittedPaymentAt { get; set; }
	public bool? Admin { get; set; }
	public string? ConfirmationToken { get; set; }
	public DateTime? ConfirmedAt { get; set; }
	public DateTime? ConfirmationSentAt { get; set; }
	public int FailedAttempts { get; set; }
	public string? UnlockToken { get; set; }
	public DateTime? LockedAt { get; set; }
	public string? InvitationToken { get; set; }
	public DateTime? InvitationCreatedAt { get; set; }
	public DateTime? InvitationSentAt { get; set; }
	public DateTime? InvitationAcceptedAt { get; set; }
	public int? InvitationLimit { get; set; }
	public string? InvitedByType { get; set; }
	public long? InvitedById { get; set; }
	public int? InvitationsCount { get; set; }
	public bool? ExportName { get; set; }
	public string? StripeCustomerId { get; set; }
	public long? LanguageId { get; set; }

	public virtual Language? Language { get; set; }
	public virtual NationalGoverningBodyAdmin NationalGoverningBodyAdmin { get; set; } = null!;
	public virtual ICollection<CertificationPayment> CertificationPayments { get; set; }
	public virtual ICollection<PolicyManagerPortabilityRequest> PolicyManagerPortabilityRequests { get; set; }
	public virtual ICollection<PolicyManagerUserTerm> PolicyManagerUserTerms { get; set; }
	public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
	public virtual ICollection<RefereeCertification> RefereeCertifications { get; set; }
	public virtual ICollection<RefereeLocation> RefereeLocations { get; set; }
	public virtual ICollection<RefereeTeam> RefereeTeams { get; set; }
	public virtual ICollection<Role> Roles { get; set; }
	public virtual ICollection<TestAttempt> TestAttempts { get; set; }
	public virtual ICollection<TestResult> TestResults { get; set; }
	public virtual ICollection<UserAttribute> Attributes { get; set; }
	public virtual ICollection<TournamentManager> TournamentManagers { get; set; }
	public virtual ICollection<TeamManager> TeamManagers { get; set; }
	public virtual ICollection<TeamManager> TeamManagersAdded { get; set; }
}
