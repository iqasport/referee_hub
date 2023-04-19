using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying", nullable: false),
                    filename = table.Column<string>(type: "character varying", nullable: false),
                    content_type = table.Column<string>(type: "character varying", nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    byte_size = table.Column<long>(type: "bigint", nullable: false),
                    checksum = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ar_internal_metadata_pkey", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    level = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "''"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0")
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
                name: "exported_csvs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    export_options = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exported_csvs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flipper_features",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flipper_features", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flipper_gates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    feature_key = table.Column<string>(type: "character varying", nullable: false),
                    key = table.Column<string>(type: "character varying", nullable: false),
                    value = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flipper_gates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    long_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "'english'"),
                    short_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "'en'"),
                    long_region = table.Column<string>(type: "character varying", nullable: true),
                    short_region = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "national_governing_bodies",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    website = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    player_count = table.Column<int>(type: "integer", nullable: false),
                    image_url = table.Column<string>(type: "character varying", nullable: true),
                    country = table.Column<string>(type: "character varying", nullable: true),
                    acronym = table.Column<string>(type: "character varying", nullable: true),
                    region = table.Column<int>(type: "integer", nullable: true),
                    membership_status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_national_governing_bodies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy_manager_terms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: true),
                    rule = table.Column<string>(type: "character varying", nullable: true),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    accepted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    rejected_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ownable_type = table.Column<string>(type: "character varying", nullable: true),
                    ownable_id = table.Column<long>(type: "bigint", nullable: true),
                    url = table.Column<string>(type: "character varying", nullable: false),
                    account_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "active_storage_attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    record_type = table.Column<string>(type: "character varying", nullable: false),
                    record_id = table.Column<long>(type: "bigint", nullable: false),
                    blob_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    level = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    name = table.Column<string>(type: "character varying", nullable: true),
                    certification_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    time_limit = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "18"),
                    minimum_pass_percentage = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "80"),
                    positive_feedback = table.Column<string>(type: "text", nullable: true),
                    negative_feedback = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "character varying", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    testable_question_count = table.Column<int>(type: "integer", nullable: false),
                    recertification = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    new_language_id = table.Column<long>(type: "bigint", nullable: true)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "''"),
                    encrypted_password = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "''"),
                    reset_password_token = table.Column<string>(type: "character varying", nullable: true),
                    reset_password_sent_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    remember_created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    sign_in_count = table.Column<int>(type: "integer", nullable: false),
                    current_sign_in_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_sign_in_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    current_sign_in_ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    last_sign_in_ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    first_name = table.Column<string>(type: "character varying", nullable: true),
                    last_name = table.Column<string>(type: "character varying", nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    pronouns = table.Column<string>(type: "character varying", nullable: true),
                    show_pronouns = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    submitted_payment_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    admin = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    confirmation_token = table.Column<string>(type: "character varying", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    confirmation_sent_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    unlock_token = table.Column<string>(type: "character varying", nullable: true),
                    locked_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    invitation_token = table.Column<string>(type: "character varying", nullable: true),
                    invitation_created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    invitation_sent_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    invitation_accepted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    invitation_limit = table.Column<int>(type: "integer", nullable: true),
                    invited_by_type = table.Column<string>(type: "character varying", nullable: true),
                    invited_by_id = table.Column<long>(type: "bigint", nullable: true),
                    invitations_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    export_name = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    stripe_customer_id = table.Column<string>(type: "character varying", nullable: true),
                    language_id = table.Column<long>(type: "bigint", nullable: true)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    national_governing_body_id = table.Column<long>(type: "bigint", nullable: true),
                    total_referees_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    head_referees_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    assistant_referees_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    snitch_referees_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    competitive_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    developing_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    inactive_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    youth_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    university_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    community_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    team_status_change_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_teams_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    uncertified_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    start = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    scorekeeper_referees_count = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0")
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    city = table.Column<string>(type: "character varying", nullable: false),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    country = table.Column<string>(type: "character varying", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    group_affiliation = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    national_governing_body_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    test_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    points_available = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1"),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    certification_id = table.Column<long>(type: "bigint", nullable: false),
                    stripe_session_id = table.Column<string>(type: "character varying", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    national_governing_body_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    expire_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    term_id = table.Column<long>(type: "bigint", nullable: true),
                    state = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referee_id = table.Column<long>(type: "bigint", nullable: false),
                    certification_id = table.Column<long>(type: "bigint", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    renewed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    needs_renewal_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referee_id = table.Column<long>(type: "bigint", nullable: false),
                    national_governing_body_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    association_type = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0")
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    access_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    test_id = table.Column<long>(type: "bigint", nullable: true),
                    referee_id = table.Column<long>(type: "bigint", nullable: true),
                    test_level = table.Column<int>(type: "integer", nullable: true),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referee_id = table.Column<long>(type: "bigint", nullable: false),
                    time_started = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    time_finished = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    duration = table.Column<string>(type: "character varying", nullable: true),
                    percentage = table.Column<int>(type: "integer", nullable: true),
                    points_scored = table.Column<int>(type: "integer", nullable: true),
                    points_available = table.Column<int>(type: "integer", nullable: true),
                    passed = table.Column<bool>(type: "boolean", nullable: true),
                    certificate_url = table.Column<string>(type: "character varying", nullable: true),
                    minimum_pass_percentage = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    test_level = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    test_id = table.Column<long>(type: "bigint", nullable: true)
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
                name: "referee_teams",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    team_id = table.Column<long>(type: "bigint", nullable: true),
                    referee_id = table.Column<long>(type: "bigint", nullable: true),
                    association_type = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "0"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "team_status_changesets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    team_id = table.Column<long>(type: "bigint", nullable: true),
                    previous_status = table.Column<string>(type: "character varying", nullable: true),
                    new_status = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    correct = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "referee_answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referee_id = table.Column<long>(type: "bigint", nullable: false),
                    test_id = table.Column<long>(type: "bigint", nullable: false),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    answer_id = table.Column<long>(type: "bigint", nullable: false),
                    test_attempt_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "team_status_changesets");

            migrationBuilder.DropTable(
                name: "test_results");

            migrationBuilder.DropTable(
                name: "active_storage_blobs");

            migrationBuilder.DropTable(
                name: "policy_manager_terms");

            migrationBuilder.DropTable(
                name: "answers");

            migrationBuilder.DropTable(
                name: "test_attempts");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "national_governing_bodies");

            migrationBuilder.DropTable(
                name: "tests");

            migrationBuilder.DropTable(
                name: "certifications");

            migrationBuilder.DropTable(
                name: "languages");
        }
    }
}
