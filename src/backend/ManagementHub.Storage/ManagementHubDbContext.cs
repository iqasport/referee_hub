using System;
using System.Collections.Generic;
using ManagementHub.Models.Data;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ManagementHub.Storage;

public partial class ManagementHubDbContext : DbContext, IDataProtectionKeyContext
{
	public ManagementHubDbContext()
	{
	}

	public ManagementHubDbContext(DbContextOptions<ManagementHubDbContext> options)
		: base(options)
	{
	}

	public virtual DbSet<ActiveStorageAttachment> ActiveStorageAttachments { get; set; } = null!;
	public virtual DbSet<ActiveStorageBlob> ActiveStorageBlobs { get; set; } = null!;
	public virtual DbSet<Answer> Answers { get; set; } = null!;
	public virtual DbSet<ArInternalMetadatum> ArInternalMetadata { get; set; } = null!;
	public virtual DbSet<Certification> Certifications { get; set; } = null!;
	public virtual DbSet<CertificationPayment> CertificationPayments { get; set; } = null!;
	public virtual DbSet<DataMigration> DataMigrations { get; set; } = null!;
	public virtual DbSet<ExportedCsv> ExportedCsvs { get; set; } = null!;
	public virtual DbSet<FlipperFeature> FlipperFeatures { get; set; } = null!;
	public virtual DbSet<FlipperGate> FlipperGates { get; set; } = null!;
	public virtual DbSet<Language> Languages { get; set; } = null!;
	public virtual DbSet<NationalGoverningBody> NationalGoverningBodies { get; set; } = null!;
	public virtual DbSet<NationalGoverningBodyAdmin> NationalGoverningBodyAdmins { get; set; } = null!;
	public virtual DbSet<NationalGoverningBodyStat> NationalGoverningBodyStats { get; set; } = null!;
	public virtual DbSet<PolicyManagerPortabilityRequest> PolicyManagerPortabilityRequests { get; set; } = null!;
	public virtual DbSet<PolicyManagerTerm> PolicyManagerTerms { get; set; } = null!;
	public virtual DbSet<PolicyManagerUserTerm> PolicyManagerUserTerms { get; set; } = null!;
	public virtual DbSet<Question> Questions { get; set; } = null!;
	public virtual DbSet<RefereeAnswer> RefereeAnswers { get; set; } = null!;
	public virtual DbSet<RefereeCertification> RefereeCertifications { get; set; } = null!;
	public virtual DbSet<RefereeLocation> RefereeLocations { get; set; } = null!;
	public virtual DbSet<RefereeTeam> RefereeTeams { get; set; } = null!;
	public virtual DbSet<Role> Roles { get; set; } = null!;
	public virtual DbSet<SchemaMigration> SchemaMigrations { get; set; } = null!;
	public virtual DbSet<SocialAccount> SocialAccounts { get; set; } = null!;
	public virtual DbSet<Team> Teams { get; set; } = null!;
	public virtual DbSet<TeamManager> TeamManagers { get; set; } = null!;
	public virtual DbSet<TeamStatusChangeset> TeamStatusChangesets { get; set; } = null!;
	public virtual DbSet<Test> Tests { get; set; } = null!;
	public virtual DbSet<Tournament> Tournaments { get; set; } = null!;
	public virtual DbSet<TournamentInvite> TournamentInvites { get; set; } = null!;
	public virtual DbSet<TournamentManager> TournamentManagers { get; set; } = null!;
	public virtual DbSet<TournamentTeamParticipant> TournamentTeamParticipants { get; set; } = null!;
	public virtual DbSet<TournamentTeamRosterEntry> TournamentTeamRosterEntries { get; set; } = null!;
	public virtual DbSet<TestAttempt> TestAttempts { get; set; } = null!;
	public virtual DbSet<TestResult> TestResults { get; set; } = null!;
	public virtual DbSet<User> Users { get; set; } = null!;
	public virtual DbSet<UserAttribute> UserAttributes { get; set; } = null!;
	public virtual DbSet<UserDelicateInfo> UserDelicateInfos { get; set; } = null!;

	public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseNpgsql();
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ActiveStorageAttachment>(entity =>
		{
			entity.ToTable("active_storage_attachments");

			entity.HasIndex(e => e.BlobId, "index_active_storage_attachments_on_blob_id");

			entity.HasIndex(e => new { e.RecordType, e.RecordId, e.Name, e.BlobId }, "index_active_storage_attachments_uniqueness")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.BlobId).HasColumnName("blob_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Name)
				.HasColumnType("character varying")
				.HasColumnName("name");

			entity.Property(e => e.RecordId).HasColumnName("record_id");

			entity.Property(e => e.RecordType)
				.HasColumnType("character varying")
				.HasColumnName("record_type");

			entity.HasOne(d => d.Blob)
				.WithMany(p => p.ActiveStorageAttachments)
				.HasForeignKey(d => d.BlobId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_rails_c3b3935057");
		});

		modelBuilder.Entity<ActiveStorageBlob>(entity =>
		{
			entity.ToTable("active_storage_blobs");

			entity.HasIndex(e => e.Key, "index_active_storage_blobs_on_key")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.ByteSize).HasColumnName("byte_size");

			entity.Property(e => e.Checksum)
				.HasColumnType("character varying")
				.HasColumnName("checksum");

			entity.Property(e => e.ContentType)
				.HasColumnType("character varying")
				.HasColumnName("content_type");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Filename)
				.HasColumnType("character varying")
				.HasColumnName("filename");

			entity.Property(e => e.Key)
				.HasColumnType("character varying")
				.HasColumnName("key");

			entity.Property(e => e.Metadata).HasColumnName("metadata");
		});

		modelBuilder.Entity<Answer>(entity =>
		{
			entity.ToTable("answers");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.Correct).HasColumnName("correct");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Description).HasColumnName("description");

			entity.Property(e => e.QuestionId).HasColumnName("question_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Question)
				.WithMany(p => p.Answers)
				.HasForeignKey(d => d.QuestionId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("answers__question_fkey");
		});

		modelBuilder.Entity<ArInternalMetadatum>(entity =>
		{
			entity.HasKey(e => e.Key)
				.HasName("ar_internal_metadata_pkey");

			entity.ToTable("ar_internal_metadata");

			entity.Property(e => e.Key)
				.HasColumnType("character varying")
				.HasColumnName("key");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.Value)
				.HasColumnType("character varying")
				.HasColumnName("value");
		});

		modelBuilder.Entity<Certification>(entity =>
		{
			entity.ToTable("certifications");

			entity.HasIndex(e => new { e.Level, e.Version }, "index_certifications_on_level_and_version")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.DisplayName)
				.HasColumnType("character varying")
				.HasColumnName("display_name")
				.HasDefaultValueSql("''");

			entity.Property(e => e.Level).HasColumnName("level");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.Version)
				.HasColumnName("version")
				.HasDefaultValueSql("0");
		});

		modelBuilder.Entity<CertificationPayment>(entity =>
		{
			entity.ToTable("certification_payments");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CertificationId).HasColumnName("certification_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.StripeSessionId)
				.HasColumnType("character varying")
				.HasColumnName("stripe_session_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.HasOne(d => d.Certification)
				.WithMany(p => p.CertificationPayments)
				.HasForeignKey(d => d.CertificationId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("certification_payments__certification_fkey");

			entity.HasOne(d => d.User)
				.WithMany(p => p.CertificationPayments)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("certification_payments__user_fkey");
		});

		modelBuilder.Entity<DataMigration>(entity =>
		{
			entity.HasKey(e => e.Version)
				.HasName("data_migrations_pkey");

			entity.ToTable("data_migrations");

			entity.Property(e => e.Version)
				.HasColumnType("character varying")
				.HasColumnName("version");
		});

		modelBuilder.Entity<ExportedCsv>(entity =>
		{
			entity.ToTable("exported_csvs");

			entity.HasIndex(e => e.UserId, "index_exported_csvs_on_user_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.ExportOptions)
				.HasColumnType("json")
				.HasColumnName("export_options")
				.HasDefaultValueSql("'{}'");

			entity.Property(e => e.ProcessedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("processed_at");

			entity.Property(e => e.SentAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("sent_at");

			entity.Property(e => e.Type)
				.HasColumnType("character varying")
				.HasColumnName("type");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.Url)
				.HasColumnType("character varying")
				.HasColumnName("url");

			entity.Property(e => e.UserId).HasColumnName("user_id");
		});

		modelBuilder.Entity<FlipperFeature>(entity =>
		{
			entity.ToTable("flipper_features");

			entity.HasIndex(e => e.Key, "index_flipper_features_on_key")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Key)
				.HasColumnType("character varying")
				.HasColumnName("key");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");
		});

		modelBuilder.Entity<FlipperGate>(entity =>
		{
			entity.ToTable("flipper_gates");

			entity.HasIndex(e => new { e.FeatureKey, e.Key, e.Value }, "index_flipper_gates_on_feature_key_and_key_and_value")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.FeatureKey)
				.HasColumnType("character varying")
				.HasColumnName("feature_key");

			entity.Property(e => e.Key)
				.HasColumnType("character varying")
				.HasColumnName("key");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.Value)
				.HasColumnType("character varying")
				.HasColumnName("value");
		});

		modelBuilder.Entity<Language>(entity =>
		{
			entity.ToTable("languages");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.LongName)
				.HasColumnType("character varying")
				.HasColumnName("long_name")
				.HasDefaultValueSql("'english'");

			entity.Property(e => e.LongRegion)
				.HasColumnType("character varying")
				.HasColumnName("long_region");

			entity.Property(e => e.ShortName)
				.HasColumnType("character varying")
				.HasColumnName("short_name")
				.HasDefaultValueSql("'en'");

			entity.Property(e => e.ShortRegion)
				.HasColumnType("character varying")
				.HasColumnName("short_region");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");
		});

		modelBuilder.Entity<NationalGoverningBody>(entity =>
		{
			entity.ToTable("national_governing_bodies");

			entity.HasIndex(e => e.Region, "index_national_governing_bodies_on_region");
			entity.HasIndex(e => e.CountryCode, "index_national_governing_bodies_on_country_code")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CountryCode)
				.HasColumnType("varchar(3)")
				.HasColumnName("countrycode");

			entity.Property(e => e.Acronym)
				.HasColumnType("character varying")
				.HasColumnName("acronym");

			entity.Property(e => e.Country)
				.HasColumnType("character varying")
				.HasColumnName("country");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.ImageUrl)
				.HasColumnType("character varying")
				.HasColumnName("image_url");

			entity.Property(e => e.MembershipStatus).HasColumnName("membership_status");

			entity.Property(e => e.Name)
				.HasColumnType("character varying")
				.HasColumnName("name");

			entity.Property(e => e.PlayerCount).HasColumnName("player_count");

			entity.Property(e => e.Region).HasColumnName("region");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.Website)
				.HasColumnType("character varying")
				.HasColumnName("website");
		});

		modelBuilder.Entity<NationalGoverningBodyAdmin>(entity =>
		{
			entity.ToTable("national_governing_body_admins");

			entity.HasIndex(e => e.NationalGoverningBodyId, "index_national_governing_body_admins_on_ngb_id");

			entity.HasIndex(e => e.UserId, "index_national_governing_body_admins_on_user_id")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.NationalGoverningBodyId).HasColumnName("national_governing_body_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.HasOne(d => d.NationalGoverningBody)
				.WithMany(p => p.NationalGoverningBodyAdmins)
				.HasForeignKey(d => d.NationalGoverningBodyId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_rails_e74edd8114");

			entity.HasOne(d => d.User)
				.WithOne(p => p.NationalGoverningBodyAdmin)
				.HasForeignKey<NationalGoverningBodyAdmin>(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_rails_c6796ff8f7");
		});

		modelBuilder.Entity<NationalGoverningBodyStat>(entity =>
		{
			entity.ToTable("national_governing_body_stats");

			entity.HasIndex(e => e.NationalGoverningBodyId, "ngb_stats_on_ngb_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AssistantRefereesCount)
				.HasColumnName("assistant_referees_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.CommunityTeamsCount)
				.HasColumnName("community_teams_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.CompetitiveTeamsCount)
				.HasColumnName("competitive_teams_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.DevelopingTeamsCount)
				.HasColumnName("developing_teams_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.EndTime)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("end_time");

			entity.Property(e => e.HeadRefereesCount)
				.HasColumnName("head_referees_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.InactiveTeamsCount)
				.HasColumnName("inactive_teams_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.NationalGoverningBodyId).HasColumnName("national_governing_body_id");

			entity.Property(e => e.ScorekeeperRefereesCount)
				.HasColumnName("scorekeeper_referees_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.SnitchRefereesCount)
				.HasColumnName("snitch_referees_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.Start)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("start");

			entity.Property(e => e.TeamStatusChangeCount)
				.HasColumnName("team_status_change_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.TotalRefereesCount)
				.HasColumnName("total_referees_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.TotalTeamsCount)
				.HasColumnName("total_teams_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.UncertifiedCount)
				.HasColumnName("uncertified_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.UniversityTeamsCount)
				.HasColumnName("university_teams_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.YouthTeamsCount)
				.HasColumnName("youth_teams_count")
				.HasDefaultValueSql("0");

			entity.HasOne(d => d.NationalGoverningBody)
				.WithMany(p => p.NationalGoverningBodyStats)
				.HasForeignKey(d => d.NationalGoverningBodyId)
				.HasConstraintName("national_governing_body_stats__national_governing_body_fkey");
		});

		modelBuilder.Entity<PolicyManagerPortabilityRequest>(entity =>
		{
			entity.ToTable("policy_manager_portability_requests");

			entity.HasIndex(e => e.UserId, "index_policy_manager_portability_requests_on_user_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.ExpireAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("expire_at");

			entity.Property(e => e.State)
				.HasColumnType("character varying")
				.HasColumnName("state");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.HasOne(d => d.User)
				.WithMany(p => p.PolicyManagerPortabilityRequests)
				.HasForeignKey(d => d.UserId)
				.HasConstraintName("policy_manager_portability_requests__user_fkey");
		});

		modelBuilder.Entity<PolicyManagerTerm>(entity =>
		{
			entity.ToTable("policy_manager_terms");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AcceptedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("accepted_at");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Description).HasColumnName("description");

			entity.Property(e => e.RejectedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("rejected_at");

			entity.Property(e => e.Rule)
				.HasColumnType("character varying")
				.HasColumnName("rule");

			entity.Property(e => e.State)
				.HasColumnType("character varying")
				.HasColumnName("state");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");
		});

		modelBuilder.Entity<PolicyManagerUserTerm>(entity =>
		{
			entity.ToTable("policy_manager_user_terms");

			entity.HasIndex(e => e.State, "index_policy_manager_user_terms_on_state");

			entity.HasIndex(e => e.TermId, "index_policy_manager_user_terms_on_term_id");

			entity.HasIndex(e => e.UserId, "index_policy_manager_user_terms_on_user_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.State)
				.HasColumnType("character varying")
				.HasColumnName("state");

			entity.Property(e => e.TermId).HasColumnName("term_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.HasOne(d => d.Term)
				.WithMany(p => p.PolicyManagerUserTerms)
				.HasForeignKey(d => d.TermId)
				.HasConstraintName("policy_manager_user_terms__term_fkey");

			entity.HasOne(d => d.User)
				.WithMany(p => p.PolicyManagerUserTerms)
				.HasForeignKey(d => d.UserId)
				.HasConstraintName("policy_manager_user_terms__user_fkey");
		});

		modelBuilder.Entity<Question>(entity =>
		{
			entity.ToTable("questions");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Description).HasColumnName("description");

			entity.Property(e => e.Feedback).HasColumnName("feedback");

			entity.Property(e => e.PointsAvailable)
				.HasColumnName("points_available")
				.HasDefaultValueSql("1");

			entity.Property(e => e.TestId).HasColumnName("test_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.SequenceId).HasColumnName("seq_id");

			entity.HasOne(d => d.Test)
				.WithMany(p => p.Questions)
				.HasForeignKey(d => d.TestId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("questions__test_fkey");
		});

		modelBuilder.Entity<RefereeAnswer>(entity =>
		{
			entity.ToTable("referee_answers");

			entity.HasIndex(e => e.AnswerId, "index_referee_answers_on_answer_id");

			entity.HasIndex(e => e.QuestionId, "index_referee_answers_on_question_id");

			entity.HasIndex(e => e.RefereeId, "index_referee_answers_on_referee_id");

			entity.HasIndex(e => e.TestAttemptId, "index_referee_answers_on_test_attempt_id");

			entity.HasIndex(e => e.TestId, "index_referee_answers_on_test_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AnswerId).HasColumnName("answer_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.QuestionId).HasColumnName("question_id");

			entity.Property(e => e.RefereeId).HasColumnName("referee_id");

			entity.Property(e => e.TestAttemptId).HasColumnName("test_attempt_id");

			entity.Property(e => e.TestId).HasColumnName("test_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Answer)
				.WithMany(p => p.RefereeAnswers)
				.HasForeignKey(d => d.AnswerId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_answers__answer_fkey");

			entity.HasOne(d => d.Question)
				.WithMany(p => p.RefereeAnswers)
				.HasForeignKey(d => d.QuestionId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_answers__question_fkey");

			entity.HasOne(d => d.Referee)
				.WithMany(p => p.RefereeAnswers)
				.HasForeignKey(d => d.RefereeId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_answers__user_fkey");

			entity.HasOne(d => d.TestAttempt)
				.WithMany(p => p.RefereeAnswers)
				.HasForeignKey(d => d.TestAttemptId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_answers__test_attempt_fkey");

			entity.HasOne(d => d.Test)
				.WithMany(p => p.RefereeAnswers)
				.HasForeignKey(d => d.TestId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_answers__test_fkey");
		});

		modelBuilder.Entity<RefereeCertification>(entity =>
		{
			entity.ToTable("referee_certifications");

			entity.HasIndex(e => new { e.RefereeId, e.CertificationId }, "index_referee_certs_on_ref_id_and_cert_id")
				.IsUnique()
				.HasFilter("(revoked_at IS NULL)");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CertificationId).HasColumnName("certification_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.NeedsRenewalAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("needs_renewal_at");

			entity.Property(e => e.ReceivedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("received_at");

			entity.Property(e => e.RefereeId).HasColumnName("referee_id");

			entity.Property(e => e.RenewedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("renewed_at");

			entity.Property(e => e.RevokedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("revoked_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Certification)
				.WithMany(p => p.RefereeCertifications)
				.HasForeignKey(d => d.CertificationId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_certifications__certification_fkey");

			entity.HasOne(d => d.Referee)
				.WithMany(p => p.RefereeCertifications)
				.HasForeignKey(d => d.RefereeId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_certifications__user_fkey");
		});

		modelBuilder.Entity<RefereeLocation>(entity =>
		{
			entity.ToTable("referee_locations");

			entity.HasIndex(e => new { e.RefereeId, e.NationalGoverningBodyId }, "index_referee_locations_on_referee_id_and_ngb_id")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AssociationType)
				.HasColumnName("association_type")
				.HasDefaultValueSql("0");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.NationalGoverningBodyId).HasColumnName("national_governing_body_id");

			entity.Property(e => e.RefereeId).HasColumnName("referee_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.NationalGoverningBody)
				.WithMany(p => p.RefereeLocations)
				.HasForeignKey(d => d.NationalGoverningBodyId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_locations__national_governing_body_fkey");

			entity.HasOne(d => d.Referee)
				.WithMany(p => p.RefereeLocations)
				.HasForeignKey(d => d.RefereeId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("referee_locations__user_fkey");
		});

		modelBuilder.Entity<RefereeTeam>(entity =>
		{
			entity.ToTable("referee_teams");

			entity.HasIndex(e => e.RefereeId, "index_referee_teams_on_referee_id");

			entity.HasIndex(e => new { e.RefereeId, e.AssociationType }, "index_referee_teams_on_referee_id_and_association_type")
				.IsUnique();

			entity.HasIndex(e => e.TeamId, "index_referee_teams_on_team_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AssociationType)
				.HasColumnName("association_type")
				.HasDefaultValueSql("0");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.RefereeId).HasColumnName("referee_id");

			entity.Property(e => e.TeamId).HasColumnName("team_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Referee)
				.WithMany(p => p.RefereeTeams)
				.HasForeignKey(d => d.RefereeId)
				.HasConstraintName("referee_teams__user_fkey");

			entity.HasOne(d => d.Team)
				.WithMany(p => p.RefereeTeams)
				.HasForeignKey(d => d.TeamId)
				.HasConstraintName("referee_teams__team_fkey");
		});

		modelBuilder.Entity<Role>(entity =>
		{
			entity.ToTable("roles");

			entity.HasIndex(e => e.UserId, "index_roles_on_user_id");

			entity.HasIndex(e => new { e.UserId, e.AccessType }, "index_roles_on_user_id_and_access_type")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AccessType).HasColumnName("access_type");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.HasOne(d => d.User)
				.WithMany(p => p.Roles)
				.HasForeignKey(d => d.UserId)
				.HasConstraintName("fk_rails_ab35d699f0");
		});

		modelBuilder.Entity<SchemaMigration>(entity =>
		{
			entity.HasKey(e => e.Version)
				.HasName("schema_migrations_pkey");

			entity.ToTable("schema_migrations");

			entity.Property(e => e.Version)
				.HasColumnType("character varying")
				.HasColumnName("version");
		});

		modelBuilder.Entity<SocialAccount>(entity =>
		{
			entity.ToTable("social_accounts");

			entity.HasIndex(e => new { e.OwnableType, e.OwnableId }, "index_social_accounts_on_ownable_type_and_ownable_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.AccountType).HasColumnName("account_type");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.OwnableId).HasColumnName("ownable_id");

			entity.Property(e => e.OwnableType)
				.HasColumnType("character varying")
				.HasColumnName("ownable_type");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.Url)
				.HasColumnType("character varying")
				.HasColumnName("url");
		});

		modelBuilder.Entity<Team>(entity =>
		{
			entity.ToTable("teams");

			entity.HasIndex(e => e.NationalGoverningBodyId, "index_teams_on_national_governing_body_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.City)
				.HasColumnType("character varying")
				.HasColumnName("city");

			entity.Property(e => e.Country)
				.HasColumnType("character varying")
				.HasColumnName("country");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.GroupAffiliation)
				.HasColumnName("group_affiliation")
				.HasDefaultValueSql("0");

			entity.Property(e => e.JoinedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("joined_at")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");

			entity.Property(e => e.Name)
				.HasColumnType("character varying")
				.HasColumnName("name");

			entity.Property(e => e.NationalGoverningBodyId).HasColumnName("national_governing_body_id");

			entity.Property(e => e.State)
				.HasColumnType("character varying")
				.HasColumnName("state");

			entity.Property(e => e.Status)
				.HasColumnName("status")
				.HasDefaultValueSql("0");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.LogoUrl)
				.HasColumnType("character varying")
				.HasColumnName("logo_url");

			entity.Property(e => e.Description)
				.HasColumnType("text")
				.HasColumnName("description");

			entity.Property(e => e.ContactEmail)
				.HasColumnType("character varying")
				.HasColumnName("contact_email");

			entity.HasOne(d => d.NationalGoverningBody)
				.WithMany(p => p.Teams)
				.HasForeignKey(d => d.NationalGoverningBodyId)
				.HasConstraintName("fk_rails_d1c3a2117a");
		});

		modelBuilder.Entity<TeamStatusChangeset>(entity =>
		{
			entity.ToTable("team_status_changesets");

			entity.HasIndex(e => e.TeamId, "index_team_status_changesets_on_team_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.NewStatus)
				.HasColumnType("character varying")
				.HasColumnName("new_status");

			entity.Property(e => e.PreviousStatus)
				.HasColumnType("character varying")
				.HasColumnName("previous_status");

			entity.Property(e => e.TeamId).HasColumnName("team_id");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Team)
				.WithMany(p => p.TeamStatusChangesets)
				.HasForeignKey(d => d.TeamId)
				.HasConstraintName("team_status_changesets__team_fkey");
		});

		modelBuilder.Entity<Test>(entity =>
		{
			entity.ToTable("tests");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.UniqueId)
				.HasColumnType("varchar(40)")
				.HasColumnName("unique_id");

			entity.Property(e => e.Active).HasColumnName("active");

			entity.Property(e => e.CertificationId).HasColumnName("certification_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Description).HasColumnName("description");

			entity.Property(e => e.Language)
				.HasColumnType("character varying")
				.HasColumnName("language");

			entity.Property(e => e.Level)
				.HasColumnName("level")
				.HasDefaultValueSql("0");

			entity.Property(e => e.MinimumPassPercentage)
				.HasColumnName("minimum_pass_percentage")
				.HasDefaultValueSql("80");

			entity.Property(e => e.Name)
				.HasColumnType("character varying")
				.HasColumnName("name");

			entity.Property(e => e.NegativeFeedback).HasColumnName("negative_feedback");

			entity.Property(e => e.NewLanguageId).HasColumnName("new_language_id");

			entity.Property(e => e.PositiveFeedback).HasColumnName("positive_feedback");

			entity.Property(e => e.Recertification)
				.HasColumnName("recertification")
				.HasDefaultValueSql("false");

			entity.Property(e => e.TestableQuestionCount).HasColumnName("testable_question_count");

			entity.Property(e => e.TimeLimit)
				.HasColumnName("time_limit")
				.HasDefaultValueSql("18");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Certification)
				.WithMany(p => p.Tests)
				.HasForeignKey(d => d.CertificationId)
				.HasConstraintName("tests__certification_fkey");

			entity.HasOne(d => d.NewLanguage)
				.WithMany(p => p.Tests)
				.HasForeignKey(d => d.NewLanguageId)
				.HasConstraintName("tests__language_fkey");
		});

		modelBuilder.Entity<TestAttempt>(entity =>
		{
			entity.ToTable("test_attempts");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.UniqueId)
				.HasColumnType("varchar(40)")
				.HasColumnName("unique_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.NextAttemptAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("next_attempt_at");

			entity.Property(e => e.RefereeId).HasColumnName("referee_id");

			entity.Property(e => e.TestId).HasColumnName("test_id");

			entity.Property(e => e.TestLevel).HasColumnName("test_level");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Referee)
				.WithMany(p => p.TestAttempts)
				.HasForeignKey(d => d.RefereeId)
				.HasConstraintName("test_attempts__user_fkey");

			entity.HasOne(d => d.Test)
				.WithMany(p => p.TestAttempts)
				.HasForeignKey(d => d.TestId)
				.HasConstraintName("test_attempts__test_fkey");
		});

		modelBuilder.Entity<TestResult>(entity =>
		{
			entity.ToTable("test_results");

			entity.HasIndex(e => e.RefereeId, "index_test_results_on_referee_id");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.UniqueId)
				.HasColumnType("varchar(40)")
				.HasColumnName("unique_id");

			entity.Property(e => e.CertificateUrl)
				.HasColumnType("character varying")
				.HasColumnName("certificate_url");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.Duration)
				.HasColumnType("character varying")
				.HasColumnName("duration");

			entity.Property(e => e.MinimumPassPercentage).HasColumnName("minimum_pass_percentage");

			entity.Property(e => e.Passed).HasColumnName("passed");

			entity.Property(e => e.Percentage).HasColumnName("percentage");

			entity.Property(e => e.PointsAvailable).HasColumnName("points_available");

			entity.Property(e => e.PointsScored).HasColumnName("points_scored");

			entity.Property(e => e.RefereeId).HasColumnName("referee_id");

			entity.Property(e => e.TestId).HasColumnName("test_id");

			entity.Property(e => e.TestLevel)
				.HasColumnName("test_level")
				.HasDefaultValueSql("0");

			entity.Property(e => e.TimeFinished).HasColumnName("time_finished");

			entity.Property(e => e.TimeStarted).HasColumnName("time_started");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Referee)
				.WithMany(p => p.TestResults)
				.HasForeignKey(d => d.RefereeId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("test_results__user_fkey");

			entity.HasOne(d => d.Test)
				.WithMany(p => p.TestResults)
				.HasForeignKey(d => d.TestId)
				.HasConstraintName("test_results__test_fkey");
		});

		modelBuilder.Entity<User>(entity =>
		{
			entity.ToTable("users");

			entity.HasIndex(e => e.ConfirmationToken, "index_users_on_confirmation_token")
				.IsUnique();

			entity.HasIndex(e => e.Email, "index_users_on_email")
				.IsUnique();

			entity.HasIndex(e => e.InvitationToken, "index_users_on_invitation_token")
				.IsUnique();

			entity.HasIndex(e => e.InvitationsCount, "index_users_on_invitations_count");

			entity.HasIndex(e => e.InvitedById, "index_users_on_invited_by_id");

			entity.HasIndex(e => new { e.InvitedByType, e.InvitedById }, "index_users_on_invited_by_type_and_invited_by_id");

			entity.HasIndex(e => e.ResetPasswordToken, "index_users_on_reset_password_token")
				.IsUnique();

			entity.HasIndex(e => e.UnlockToken, "index_users_on_unlock_token")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.UniqueId)
				.HasColumnType("varchar(40)")
				.HasColumnName("unique_id");

			entity.Property(e => e.Admin)
				.HasColumnName("admin")
				.HasDefaultValueSql("false");

			entity.Property(e => e.Bio).HasColumnName("bio");

			entity.Property(e => e.ConfirmationSentAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("confirmation_sent_at");

			entity.Property(e => e.ConfirmationToken)
				.HasColumnType("character varying")
				.HasColumnName("confirmation_token");

			entity.Property(e => e.ConfirmedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("confirmed_at");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.CurrentSignInAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("current_sign_in_at");

			entity.Property(e => e.CurrentSignInIp).HasColumnName("current_sign_in_ip");

			entity.Property(e => e.Email)
				.HasColumnType("character varying")
				.HasColumnName("email")
				.HasDefaultValueSql("''");

			entity.Property(e => e.EncryptedPassword)
				.HasColumnType("character varying")
				.HasColumnName("encrypted_password")
				.HasDefaultValueSql("''");

			entity.Property(e => e.ExportName)
				.HasColumnName("export_name")
				.HasDefaultValueSql("true");

			entity.Property(e => e.FailedAttempts).HasColumnName("failed_attempts");

			entity.Property(e => e.FirstName)
				.HasColumnType("character varying")
				.HasColumnName("first_name");

			entity.Property(e => e.InvitationAcceptedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("invitation_accepted_at");

			entity.Property(e => e.InvitationCreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("invitation_created_at");

			entity.Property(e => e.InvitationLimit).HasColumnName("invitation_limit");

			entity.Property(e => e.InvitationSentAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("invitation_sent_at");

			entity.Property(e => e.InvitationToken)
				.HasColumnType("character varying")
				.HasColumnName("invitation_token");

			entity.Property(e => e.InvitationsCount)
				.HasColumnName("invitations_count")
				.HasDefaultValueSql("0");

			entity.Property(e => e.InvitedById).HasColumnName("invited_by_id");

			entity.Property(e => e.InvitedByType)
				.HasColumnType("character varying")
				.HasColumnName("invited_by_type");

			entity.Property(e => e.LanguageId).HasColumnName("language_id");

			entity.Property(e => e.LastName)
				.HasColumnType("character varying")
				.HasColumnName("last_name");

			entity.Property(e => e.LastSignInAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("last_sign_in_at");

			entity.Property(e => e.LastSignInIp).HasColumnName("last_sign_in_ip");

			entity.Property(e => e.LockedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("locked_at");

			entity.Property(e => e.Pronouns)
				.HasColumnType("character varying")
				.HasColumnName("pronouns");

			entity.Property(e => e.RememberCreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("remember_created_at");

			entity.Property(e => e.ResetPasswordSentAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("reset_password_sent_at");

			entity.Property(e => e.ResetPasswordToken)
				.HasColumnType("character varying")
				.HasColumnName("reset_password_token");

			entity.Property(e => e.ShowPronouns)
				.HasColumnName("show_pronouns")
				.HasDefaultValueSql("false");

			entity.Property(e => e.SignInCount).HasColumnName("sign_in_count");

			entity.Property(e => e.StripeCustomerId)
				.HasColumnType("character varying")
				.HasColumnName("stripe_customer_id");

			entity.Property(e => e.SubmittedPaymentAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("submitted_payment_at");

			entity.Property(e => e.UnlockToken)
				.HasColumnType("character varying")
				.HasColumnName("unlock_token");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Language)
				.WithMany(p => p.Users)
				.HasForeignKey(d => d.LanguageId)
				.HasConstraintName("users__language_fkey");
		});

		modelBuilder.Entity<UserAttribute>(entity =>
		{
			entity.ToTable("user_attributes");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.Property(e => e.Prefix).HasColumnName("prefix");

			entity.Property(e => e.Key).HasColumnName("key");

			entity.Property(e => e.Attribute).HasColumnName("attribute_value");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.User)
				.WithMany(p => p.Attributes)
				.HasForeignKey(d => d.UserId)
				.HasConstraintName("fk_user_attributes_user_user_id");
		});

		modelBuilder.Entity<Tournament>(entity =>
		{
			entity.ToTable("tournaments");

			entity.HasIndex(e => e.UniqueId, "index_tournaments_on_unique_id")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.UniqueId)
				.HasColumnType("character varying")
				.HasColumnName("unique_id");

			entity.Property(e => e.Name)
				.HasColumnType("character varying")
				.HasColumnName("name");

			entity.Property(e => e.Description)
				.HasColumnType("text")
				.HasColumnName("description");

			entity.Property(e => e.StartDate)
				.HasColumnType("date")
				.HasColumnName("start_date");

			entity.Property(e => e.EndDate)
				.HasColumnType("date")
				.HasColumnName("end_date");

			entity.Property(e => e.RegistrationEndsDate)
				.HasColumnType("date")
				.HasColumnName("registration_ends_date");

			entity.Property(e => e.Type)
				.HasColumnName("type");

			entity.Property(e => e.Country)
				.HasColumnType("character varying")
				.HasColumnName("country");

			entity.Property(e => e.City)
				.HasColumnType("character varying")
				.HasColumnName("city");

			entity.Property(e => e.Place)
				.HasColumnType("character varying")
				.HasColumnName("place");

			entity.Property(e => e.Organizer)
				.HasColumnType("character varying")
				.HasColumnName("organizer");

			entity.Property(e => e.IsPrivate)
				.HasColumnName("is_private");

			entity.Property(e => e.IsRegistrationOpen)
				.HasColumnName("is_registration_open");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");
		});

		modelBuilder.Entity<TournamentManager>(entity =>
		{
			entity.ToTable("tournament_managers");

			entity.HasIndex(e => new { e.TournamentId, e.UserId }, "index_tournament_managers_on_tournament_id_and_user_id")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.TournamentId).HasColumnName("tournament_id");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.Property(e => e.AddedByUserId).HasColumnName("added_by_user_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Tournament)
				.WithMany(p => p.TournamentManagers)
				.HasForeignKey(d => d.TournamentId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_tournament_managers_tournament");

			entity.HasOne(d => d.User)
				.WithMany(p => p.TournamentManagers)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_tournament_managers_user");

			entity.HasOne(d => d.AddedBy)
				.WithMany()
				.HasForeignKey(d => d.AddedByUserId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_tournament_managers_added_by_user");
		});

		modelBuilder.Entity<TournamentInvite>(entity =>
		{
			entity.ToTable("tournament_invites");

			entity.HasIndex(e => new { e.TournamentId, e.ParticipantType, e.ParticipantId }, "index_tournament_invites_on_tournament_and_participant")
				.IsUnique()
				.HasFilter("tournament_manager_approval != 2 AND participant_approval != 2");

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.TournamentId).HasColumnName("tournament_id");

			entity.Property(e => e.ParticipantType)
				.HasColumnType("character varying")
				.HasColumnName("participant_type");

			entity.Property(e => e.ParticipantId)
				.HasColumnType("character varying")
				.HasColumnName("participant_id");

			entity.Property(e => e.InitiatorUserId).HasColumnName("initiator_user_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.TournamentManagerApproval)
				.HasColumnName("tournament_manager_approval");

			entity.Property(e => e.TournamentManagerApprovalDate)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("tournament_manager_approval_date");

			entity.Property(e => e.ParticipantApproval)
				.HasColumnName("participant_approval");

			entity.Property(e => e.ParticipantApprovalDate)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("participant_approval_date");

			entity.HasOne(d => d.Tournament)
				.WithMany(p => p.TournamentInvites)
				.HasForeignKey(d => d.TournamentId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_tournament_invites_tournament");

			entity.HasOne(d => d.Initiator)
				.WithMany()
				.HasForeignKey(d => d.InitiatorUserId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_tournament_invites_initiator");
		});

		modelBuilder.Entity<TournamentTeamParticipant>(entity =>
		{
			entity.ToTable("tournament_team_participants");

			entity.HasIndex(e => new { e.TournamentId, e.TeamId }, "index_tournament_team_participants_on_tournament_and_team")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.TournamentId).HasColumnName("tournament_id");

			entity.Property(e => e.TeamId).HasColumnName("team_id");

			entity.Property(e => e.TeamName)
				.HasColumnType("character varying")
				.HasColumnName("team_name");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Tournament)
				.WithMany(p => p.TournamentTeamParticipants)
				.HasForeignKey(d => d.TournamentId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_tournament_team_participants_tournament");

			entity.HasOne(d => d.Team)
				.WithMany(p => p.TournamentTeamParticipants)
				.HasForeignKey(d => d.TeamId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_tournament_team_participants_team");
		});

		modelBuilder.Entity<TournamentTeamRosterEntry>(entity =>
		{
			entity.ToTable("tournament_team_roster_entries");

			entity.HasIndex(e => new { e.TournamentTeamParticipantId, e.UserId }, "index_roster_entries_on_participant_and_user")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.TournamentTeamParticipantId).HasColumnName("tournament_team_participant_id");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.Property(e => e.Role)
				.HasConversion<int>()
				.HasColumnName("role");

			entity.Property(e => e.JerseyNumber)
				.HasMaxLength(5)
				.HasColumnType("character varying")
				.HasColumnName("jersey_number");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Participant)
				.WithMany(p => p.RosterEntries)
				.HasForeignKey(d => d.TournamentTeamParticipantId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_tournament_team_roster_entries_participant");

			entity.HasOne(d => d.User)
				.WithMany(p => p.TournamentTeamRosterEntries)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_tournament_team_roster_entries_user")
				.IsRequired(false);

			// Check constraint: JerseyNumber required when Role = Player (0)
			entity.ToTable(t => t.HasCheckConstraint(
				"chk_roster_entry_jersey_number",
				"role != 0 OR (role = 0 AND jersey_number IS NOT NULL)"));
		});

		modelBuilder.Entity<UserDelicateInfo>(entity =>
		{
			entity.ToTable("user_delicate_info");

			entity.HasIndex(e => e.UserId, "index_user_delicate_info_on_user_id")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.Property(e => e.Gender)
				.HasColumnType("character varying")
				.HasColumnName("gender");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.HasOne(d => d.User)
				.WithOne()
				.HasForeignKey<UserDelicateInfo>(d => d.UserId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_user_delicate_info_user")
				.IsRequired();
		});

		modelBuilder.Entity<TeamManager>(entity =>
		{
			entity.ToTable("team_managers");

			entity.HasIndex(e => new { e.TeamId, e.UserId }, "index_team_managers_on_team_id_and_user_id")
				.IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");

			entity.Property(e => e.TeamId).HasColumnName("team_id");

			entity.Property(e => e.UserId).HasColumnName("user_id");

			entity.Property(e => e.AddedByUserId).HasColumnName("added_by_user_id");

			entity.Property(e => e.CreatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("created_at");

			entity.Property(e => e.UpdatedAt)
				.HasColumnType("timestamp with time zone")
				.HasColumnName("updated_at");

			entity.HasOne(d => d.Team)
				.WithMany(p => p.TeamManagers)
				.HasForeignKey(d => d.TeamId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_team_managers_team");

			entity.HasOne(d => d.User)
				.WithMany(p => p.TeamManagers)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_team_managers_user");

			entity.HasOne(d => d.AddedBy)
				.WithMany(u => u.TeamManagersAdded)
				.HasForeignKey(d => d.AddedByUserId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasConstraintName("fk_team_managers_added_by_user");
		});

		this.OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
