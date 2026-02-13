using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "active_storage_blobs",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    key = table.Column<string>(type: "character varying", nullable: false),
                    filename = table.Column<string>(type: "character varying", nullable: false),
                    content_type = table.Column<string>(type: "character varying", nullable: true),
                    metadata = table.Column<string>(type: "TEXT", nullable: true),
                    byte_size = table.Column<long>(type: "INTEGER", nullable: false),
                    checksum = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_active_storage_blobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ar_internal_metadata",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying", nullable: false),
                    value = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ar_internal_metadata_pkey", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    level = table.Column<int>(type: "INTEGER", nullable: false),
                    display_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "''"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_migrations",
                columns: table => new
                {
                    version = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("data_migrations_pkey", x => x.version);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: true),
                    Xml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exported_csvs",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type = table.Column<string>(type: "character varying", nullable: true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    url = table.Column<string>(type: "character varying", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    export_options = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exported_csvs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flipper_features",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    key = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flipper_features", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flipper_gates",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    feature_key = table.Column<string>(type: "character varying", nullable: false),
                    key = table.Column<string>(type: "character varying", nullable: false),
                    value = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flipper_gates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    long_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "'english'"),
                    short_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "'en'"),
                    long_region = table.Column<string>(type: "character varying", nullable: true),
                    short_region = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "national_governing_bodies",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    countrycode = table.Column<string>(type: "varchar(3)", nullable: false),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    website = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    player_count = table.Column<int>(type: "INTEGER", nullable: false),
                    image_url = table.Column<string>(type: "character varying", nullable: true),
                    country = table.Column<string>(type: "character varying", nullable: true),
                    acronym = table.Column<string>(type: "character varying", nullable: true),
                    region = table.Column<int>(type: "INTEGER", nullable: true),
                    membership_status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_national_governing_bodies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy_manager_terms",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    rule = table.Column<string>(type: "character varying", nullable: true),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_manager_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "schema_migrations",
                columns: table => new
                {
                    version = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("schema_migrations_pkey", x => x.version);
                });

            migrationBuilder.CreateTable(
                name: "social_accounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ownable_type = table.Column<string>(type: "character varying", nullable: true),
                    ownable_id = table.Column<long>(type: "INTEGER", nullable: true),
                    url = table.Column<string>(type: "character varying", nullable: false),
                    account_type = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tournaments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    unique_id = table.Column<string>(type: "character varying", nullable: false),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    registration_ends_date = table.Column<DateOnly>(type: "date", nullable: true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    country = table.Column<string>(type: "character varying", nullable: false),
                    city = table.Column<string>(type: "character varying", nullable: false),
                    place = table.Column<string>(type: "character varying", nullable: true),
                    organizer = table.Column<string>(type: "character varying", nullable: false),
                    is_private = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_registration_open = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournaments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "active_storage_attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    record_type = table.Column<string>(type: "character varying", nullable: false),
                    record_id = table.Column<long>(type: "INTEGER", nullable: false),
                    blob_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_active_storage_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_rails_c3b3935057",
                        column: x => x.blob_id,
                        principalTable: "active_storage_blobs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tests",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    unique_id = table.Column<string>(type: "varchar(40)", nullable: true),
                    level = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    name = table.Column<string>(type: "character varying", nullable: true),
                    certification_id = table.Column<long>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    time_limit = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "18"),
                    minimum_pass_percentage = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "80"),
                    positive_feedback = table.Column<string>(type: "TEXT", nullable: true),
                    negative_feedback = table.Column<string>(type: "TEXT", nullable: true),
                    language = table.Column<string>(type: "character varying", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    testable_question_count = table.Column<int>(type: "INTEGER", nullable: false),
                    recertification = table.Column<bool>(type: "INTEGER", nullable: true, defaultValueSql: "false"),
                    new_language_id = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tests", x => x.id);
                    table.ForeignKey(
                        name: "tests__certification_fkey",
                        column: x => x.certification_id,
                        principalTable: "certifications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tests__language_fkey",
                        column: x => x.new_language_id,
                        principalTable: "languages",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    unique_id = table.Column<string>(type: "varchar(40)", nullable: true),
                    email = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "''"),
                    encrypted_password = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "''"),
                    reset_password_token = table.Column<string>(type: "character varying", nullable: true),
                    reset_password_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remember_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sign_in_count = table.Column<int>(type: "INTEGER", nullable: false),
                    current_sign_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sign_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_sign_in_ip = table.Column<string>(type: "TEXT", nullable: true),
                    last_sign_in_ip = table.Column<string>(type: "TEXT", nullable: true),
                    first_name = table.Column<string>(type: "character varying", nullable: true),
                    last_name = table.Column<string>(type: "character varying", nullable: true),
                    bio = table.Column<string>(type: "TEXT", nullable: true),
                    pronouns = table.Column<string>(type: "character varying", nullable: true),
                    show_pronouns = table.Column<bool>(type: "INTEGER", nullable: true, defaultValueSql: "false"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_payment_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    admin = table.Column<bool>(type: "INTEGER", nullable: true, defaultValueSql: "false"),
                    confirmation_token = table.Column<string>(type: "character varying", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmation_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    unlock_token = table.Column<string>(type: "character varying", nullable: true),
                    locked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invitation_token = table.Column<string>(type: "character varying", nullable: true),
                    invitation_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invitation_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invitation_accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invitation_limit = table.Column<int>(type: "INTEGER", nullable: true),
                    invited_by_type = table.Column<string>(type: "character varying", nullable: true),
                    invited_by_id = table.Column<long>(type: "INTEGER", nullable: true),
                    invitations_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    export_name = table.Column<bool>(type: "INTEGER", nullable: true, defaultValueSql: "true"),
                    stripe_customer_id = table.Column<string>(type: "character varying", nullable: true),
                    language_id = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "users__language_fkey",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "national_governing_body_stats",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    national_governing_body_id = table.Column<long>(type: "INTEGER", nullable: true),
                    total_referees_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    head_referees_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    assistant_referees_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    snitch_referees_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    competitive_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    developing_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    inactive_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    youth_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    university_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    community_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    team_status_change_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_teams_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    uncertified_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scorekeeper_referees_count = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_national_governing_body_stats", x => x.id);
                    table.ForeignKey(
                        name: "national_governing_body_stats__national_governing_body_fkey",
                        column: x => x.national_governing_body_id,
                        principalTable: "national_governing_bodies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    city = table.Column<string>(type: "character varying", nullable: false),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    country = table.Column<string>(type: "character varying", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    group_affiliation = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    national_governing_body_id = table.Column<long>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_rails_d1c3a2117a",
                        column: x => x.national_governing_body_id,
                        principalTable: "national_governing_bodies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    test_id = table.Column<long>(type: "INTEGER", nullable: false),
                    seq_id = table.Column<long>(type: "INTEGER", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    points_available = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "1"),
                    feedback = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "questions__test_fkey",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "certification_payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    certification_id = table.Column<long>(type: "INTEGER", nullable: false),
                    stripe_session_id = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certification_payments", x => x.id);
                    table.ForeignKey(
                        name: "certification_payments__certification_fkey",
                        column: x => x.certification_id,
                        principalTable: "certifications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "certification_payments__user_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "national_governing_body_admins",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    national_governing_body_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_national_governing_body_admins", x => x.id);
                    table.ForeignKey(
                        name: "fk_rails_c6796ff8f7",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_rails_e74edd8114",
                        column: x => x.national_governing_body_id,
                        principalTable: "national_governing_bodies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "policy_manager_portability_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: true),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    expire_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_manager_portability_requests", x => x.id);
                    table.ForeignKey(
                        name: "policy_manager_portability_requests__user_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "policy_manager_user_terms",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: true),
                    term_id = table.Column<long>(type: "INTEGER", nullable: true),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_manager_user_terms", x => x.id);
                    table.ForeignKey(
                        name: "policy_manager_user_terms__term_fkey",
                        column: x => x.term_id,
                        principalTable: "policy_manager_terms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "policy_manager_user_terms__user_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "referee_certifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    referee_id = table.Column<long>(type: "INTEGER", nullable: false),
                    certification_id = table.Column<long>(type: "INTEGER", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    renewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    needs_renewal_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referee_certifications", x => x.id);
                    table.ForeignKey(
                        name: "referee_certifications__certification_fkey",
                        column: x => x.certification_id,
                        principalTable: "certifications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_certifications__user_fkey",
                        column: x => x.referee_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "referee_locations",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    referee_id = table.Column<long>(type: "INTEGER", nullable: false),
                    national_governing_body_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    association_type = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referee_locations", x => x.id);
                    table.ForeignKey(
                        name: "referee_locations__national_governing_body_fkey",
                        column: x => x.national_governing_body_id,
                        principalTable: "national_governing_bodies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_locations__user_fkey",
                        column: x => x.referee_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: true),
                    access_type = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_rails_ab35d699f0",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "test_attempts",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    unique_id = table.Column<string>(type: "varchar(40)", nullable: true),
                    test_id = table.Column<long>(type: "INTEGER", nullable: true),
                    referee_id = table.Column<long>(type: "INTEGER", nullable: true),
                    test_level = table.Column<int>(type: "INTEGER", nullable: true),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_attempts", x => x.id);
                    table.ForeignKey(
                        name: "test_attempts__test_fkey",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "test_attempts__user_fkey",
                        column: x => x.referee_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "test_results",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    unique_id = table.Column<string>(type: "varchar(40)", nullable: true),
                    referee_id = table.Column<long>(type: "INTEGER", nullable: false),
                    time_started = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    time_finished = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    duration = table.Column<string>(type: "character varying", nullable: true),
                    percentage = table.Column<int>(type: "INTEGER", nullable: true),
                    points_scored = table.Column<int>(type: "INTEGER", nullable: true),
                    points_available = table.Column<int>(type: "INTEGER", nullable: true),
                    passed = table.Column<bool>(type: "INTEGER", nullable: true),
                    certificate_url = table.Column<string>(type: "character varying", nullable: true),
                    minimum_pass_percentage = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    test_level = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    test_id = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_results", x => x.id);
                    table.ForeignKey(
                        name: "test_results__test_fkey",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "test_results__user_fkey",
                        column: x => x.referee_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tournament_invites",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tournament_id = table.Column<long>(type: "INTEGER", nullable: false),
                    participant_type = table.Column<string>(type: "character varying", nullable: false),
                    participant_id = table.Column<string>(type: "character varying", nullable: false),
                    initiator_user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tournament_manager_approval = table.Column<int>(type: "INTEGER", nullable: false),
                    tournament_manager_approval_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    participant_approval = table.Column<int>(type: "INTEGER", nullable: false),
                    participant_approval_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_invites", x => x.id);
                    table.ForeignKey(
                        name: "fk_tournament_invites_initiator",
                        column: x => x.initiator_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tournament_invites_tournament",
                        column: x => x.tournament_id,
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tournament_managers",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tournament_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    added_by_user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_managers", x => x.id);
                    table.ForeignKey(
                        name: "fk_tournament_managers_added_by_user",
                        column: x => x.added_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tournament_managers_tournament",
                        column: x => x.tournament_id,
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tournament_managers_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_attributes",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    prefix = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    attribute_value = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_attributes", x => new { x.user_id, x.prefix, x.key });
                    table.ForeignKey(
                        name: "fk_user_attributes_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_delicate_info",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    gender = table.Column<string>(type: "character varying", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_delicate_info", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_delicate_info_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referee_teams",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    team_id = table.Column<long>(type: "INTEGER", nullable: true),
                    referee_id = table.Column<long>(type: "INTEGER", nullable: true),
                    association_type = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referee_teams", x => x.id);
                    table.ForeignKey(
                        name: "referee_teams__team_fkey",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_teams__user_fkey",
                        column: x => x.referee_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "team_managers",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    team_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    added_by_user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_managers", x => x.id);
                    table.ForeignKey(
                        name: "fk_team_managers_added_by_user",
                        column: x => x.added_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_team_managers_team",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_team_managers_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team_status_changesets",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    team_id = table.Column<long>(type: "INTEGER", nullable: true),
                    previous_status = table.Column<string>(type: "character varying", nullable: true),
                    new_status = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_status_changesets", x => x.id);
                    table.ForeignKey(
                        name: "team_status_changesets__team_fkey",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tournament_team_participants",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tournament_id = table.Column<long>(type: "INTEGER", nullable: false),
                    team_id = table.Column<long>(type: "INTEGER", nullable: false),
                    team_name = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_team_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_tournament_team_participants_team",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tournament_team_participants_tournament",
                        column: x => x.tournament_id,
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    question_id = table.Column<long>(type: "INTEGER", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    correct = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answers", x => x.id);
                    table.ForeignKey(
                        name: "answers__question_fkey",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tournament_team_roster_entries",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tournament_team_participant_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    role = table.Column<int>(type: "INTEGER", nullable: false),
                    jersey_number = table.Column<string>(type: "character varying", maxLength: 5, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_team_roster_entries", x => x.id);
                    table.CheckConstraint("chk_roster_entry_jersey_number", "role != 0 OR (role = 0 AND jersey_number IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_tournament_team_roster_entries_participant",
                        column: x => x.tournament_team_participant_id,
                        principalTable: "tournament_team_participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tournament_team_roster_entries_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "referee_answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    referee_id = table.Column<long>(type: "INTEGER", nullable: false),
                    test_id = table.Column<long>(type: "INTEGER", nullable: false),
                    question_id = table.Column<long>(type: "INTEGER", nullable: false),
                    answer_id = table.Column<long>(type: "INTEGER", nullable: false),
                    test_attempt_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referee_answers", x => x.id);
                    table.ForeignKey(
                        name: "referee_answers__answer_fkey",
                        column: x => x.answer_id,
                        principalTable: "answers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_answers__question_fkey",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_answers__test_attempt_fkey",
                        column: x => x.test_attempt_id,
                        principalTable: "test_attempts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_answers__test_fkey",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "referee_answers__user_fkey",
                        column: x => x.referee_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "index_active_storage_attachments_on_blob_id",
                table: "active_storage_attachments",
                column: "blob_id");

            migrationBuilder.CreateIndex(
                name: "index_active_storage_attachments_uniqueness",
                table: "active_storage_attachments",
                columns: new[] { "record_type", "record_id", "name", "blob_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_active_storage_blobs_on_key",
                table: "active_storage_blobs",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_answers_question_id",
                table: "answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_certification_payments_certification_id",
                table: "certification_payments",
                column: "certification_id");

            migrationBuilder.CreateIndex(
                name: "IX_certification_payments_user_id",
                table: "certification_payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_certifications_on_level_and_version",
                table: "certifications",
                columns: new[] { "level", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_exported_csvs_on_user_id",
                table: "exported_csvs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_flipper_features_on_key",
                table: "flipper_features",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_flipper_gates_on_feature_key_and_key_and_value",
                table: "flipper_gates",
                columns: new[] { "feature_key", "key", "value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_national_governing_bodies_on_country_code",
                table: "national_governing_bodies",
                column: "countrycode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_national_governing_bodies_on_region",
                table: "national_governing_bodies",
                column: "region");

            migrationBuilder.CreateIndex(
                name: "index_national_governing_body_admins_on_ngb_id",
                table: "national_governing_body_admins",
                column: "national_governing_body_id");

            migrationBuilder.CreateIndex(
                name: "index_national_governing_body_admins_on_user_id",
                table: "national_governing_body_admins",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ngb_stats_on_ngb_id",
                table: "national_governing_body_stats",
                column: "national_governing_body_id");

            migrationBuilder.CreateIndex(
                name: "index_policy_manager_portability_requests_on_user_id",
                table: "policy_manager_portability_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_policy_manager_user_terms_on_state",
                table: "policy_manager_user_terms",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "index_policy_manager_user_terms_on_term_id",
                table: "policy_manager_user_terms",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "index_policy_manager_user_terms_on_user_id",
                table: "policy_manager_user_terms",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_test_id",
                table: "questions",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_answers_on_answer_id",
                table: "referee_answers",
                column: "answer_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_answers_on_question_id",
                table: "referee_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_answers_on_referee_id",
                table: "referee_answers",
                column: "referee_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_answers_on_test_attempt_id",
                table: "referee_answers",
                column: "test_attempt_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_answers_on_test_id",
                table: "referee_answers",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_certs_on_ref_id_and_cert_id",
                table: "referee_certifications",
                columns: new[] { "referee_id", "certification_id" },
                unique: true,
                filter: "(revoked_at IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_referee_certifications_certification_id",
                table: "referee_certifications",
                column: "certification_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_locations_on_referee_id_and_ngb_id",
                table: "referee_locations",
                columns: new[] { "referee_id", "national_governing_body_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referee_locations_national_governing_body_id",
                table: "referee_locations",
                column: "national_governing_body_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_teams_on_referee_id",
                table: "referee_teams",
                column: "referee_id");

            migrationBuilder.CreateIndex(
                name: "index_referee_teams_on_referee_id_and_association_type",
                table: "referee_teams",
                columns: new[] { "referee_id", "association_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_referee_teams_on_team_id",
                table: "referee_teams",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "index_roles_on_user_id",
                table: "roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_roles_on_user_id_and_access_type",
                table: "roles",
                columns: new[] { "user_id", "access_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_social_accounts_on_ownable_type_and_ownable_id",
                table: "social_accounts",
                columns: new[] { "ownable_type", "ownable_id" });

            migrationBuilder.CreateIndex(
                name: "index_team_managers_on_team_id_and_user_id",
                table: "team_managers",
                columns: new[] { "team_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_team_managers_added_by_user_id",
                table: "team_managers",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_managers_user_id",
                table: "team_managers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_team_status_changesets_on_team_id",
                table: "team_status_changesets",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "index_teams_on_national_governing_body_id",
                table: "teams",
                column: "national_governing_body_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempts_referee_id",
                table: "test_attempts",
                column: "referee_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempts_test_id",
                table: "test_attempts",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "index_test_results_on_referee_id",
                table: "test_results",
                column: "referee_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_results_test_id",
                table: "test_results",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_tests_certification_id",
                table: "tests",
                column: "certification_id");

            migrationBuilder.CreateIndex(
                name: "IX_tests_new_language_id",
                table: "tests",
                column: "new_language_id");

            migrationBuilder.CreateIndex(
                name: "index_tournament_invites_on_tournament_and_participant",
                table: "tournament_invites",
                columns: new[] { "tournament_id", "participant_type", "participant_id" },
                unique: true,
                filter: "tournament_manager_approval != 2 AND participant_approval != 2");

            migrationBuilder.CreateIndex(
                name: "IX_tournament_invites_initiator_user_id",
                table: "tournament_invites",
                column: "initiator_user_id");

            migrationBuilder.CreateIndex(
                name: "index_tournament_managers_on_tournament_id_and_user_id",
                table: "tournament_managers",
                columns: new[] { "tournament_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tournament_managers_added_by_user_id",
                table: "tournament_managers",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tournament_managers_user_id",
                table: "tournament_managers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_tournament_team_participants_on_tournament_and_team",
                table: "tournament_team_participants",
                columns: new[] { "tournament_id", "team_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tournament_team_participants_team_id",
                table: "tournament_team_participants",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "index_roster_entries_on_participant_and_user",
                table: "tournament_team_roster_entries",
                columns: new[] { "tournament_team_participant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tournament_team_roster_entries_user_id",
                table: "tournament_team_roster_entries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "index_tournaments_on_unique_id",
                table: "tournaments",
                column: "unique_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_user_delicate_info_on_user_id",
                table: "user_delicate_info",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_users_on_confirmation_token",
                table: "users",
                column: "confirmation_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_users_on_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_users_on_invitation_token",
                table: "users",
                column: "invitation_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_users_on_invitations_count",
                table: "users",
                column: "invitations_count");

            migrationBuilder.CreateIndex(
                name: "index_users_on_invited_by_id",
                table: "users",
                column: "invited_by_id");

            migrationBuilder.CreateIndex(
                name: "index_users_on_invited_by_type_and_invited_by_id",
                table: "users",
                columns: new[] { "invited_by_type", "invited_by_id" });

            migrationBuilder.CreateIndex(
                name: "index_users_on_reset_password_token",
                table: "users",
                column: "reset_password_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_users_on_unlock_token",
                table: "users",
                column: "unlock_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_language_id",
                table: "users",
                column: "language_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "active_storage_attachments");

            migrationBuilder.DropTable(
                name: "ar_internal_metadata");

            migrationBuilder.DropTable(
                name: "certification_payments");

            migrationBuilder.DropTable(
                name: "data_migrations");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "exported_csvs");

            migrationBuilder.DropTable(
                name: "flipper_features");

            migrationBuilder.DropTable(
                name: "flipper_gates");

            migrationBuilder.DropTable(
                name: "national_governing_body_admins");

            migrationBuilder.DropTable(
                name: "national_governing_body_stats");

            migrationBuilder.DropTable(
                name: "policy_manager_portability_requests");

            migrationBuilder.DropTable(
                name: "policy_manager_user_terms");

            migrationBuilder.DropTable(
                name: "referee_answers");

            migrationBuilder.DropTable(
                name: "referee_certifications");

            migrationBuilder.DropTable(
                name: "referee_locations");

            migrationBuilder.DropTable(
                name: "referee_teams");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "schema_migrations");

            migrationBuilder.DropTable(
                name: "social_accounts");

            migrationBuilder.DropTable(
                name: "team_managers");

            migrationBuilder.DropTable(
                name: "team_status_changesets");

            migrationBuilder.DropTable(
                name: "test_results");

            migrationBuilder.DropTable(
                name: "tournament_invites");

            migrationBuilder.DropTable(
                name: "tournament_managers");

            migrationBuilder.DropTable(
                name: "tournament_team_roster_entries");

            migrationBuilder.DropTable(
                name: "user_attributes");

            migrationBuilder.DropTable(
                name: "user_delicate_info");

            migrationBuilder.DropTable(
                name: "active_storage_blobs");

            migrationBuilder.DropTable(
                name: "policy_manager_terms");

            migrationBuilder.DropTable(
                name: "answers");

            migrationBuilder.DropTable(
                name: "test_attempts");

            migrationBuilder.DropTable(
                name: "tournament_team_participants");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "tournaments");

            migrationBuilder.DropTable(
                name: "tests");

            migrationBuilder.DropTable(
                name: "national_governing_bodies");

            migrationBuilder.DropTable(
                name: "certifications");

            migrationBuilder.DropTable(
                name: "languages");
        }
    }
}
