# This file is auto-generated from the current state of the database. Instead
# of editing this file, please use the migrations feature of Active Record to
# incrementally modify your database, and then regenerate this schema definition.
#
# Note that this schema.rb definition is the authoritative source for your
# database schema. If you need to create the application database on another
# system, you should be using db:schema:load, not running all the migrations
# from scratch. The latter is a flawed and unsustainable approach (the more migrations
# you'll amass, the slower it'll run and the greater likelihood for issues).
#
# It's strongly recommended that you check this file into your version control system.

ActiveRecord::Schema.define(version: 2019_11_23_214158) do

  # These are extensions that must be enabled in order to support this database
  enable_extension "plpgsql"

  create_table "answers", force: :cascade do |t|
    t.integer "question_id", null: false
    t.text "description", null: false
    t.boolean "correct", default: false, null: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
  end

  create_table "certifications", force: :cascade do |t|
    t.integer "level", null: false
    t.string "display_name", default: "", null: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["level"], name: "index_certifications_on_level", unique: true
  end

  create_table "national_governing_bodies", force: :cascade do |t|
    t.string "name", null: false
    t.string "website"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.integer "player_count", default: 0, null: false
    t.string "image_url"
    t.string "country"
    t.string "acronym"
  end

  create_table "national_governing_body_stats", force: :cascade do |t|
    t.bigint "national_governing_body_id"
    t.integer "total_referees_count", default: 0
    t.integer "head_referees_count", default: 0
    t.integer "assistant_referees_count", default: 0
    t.integer "snitch_referees_count", default: 0
    t.integer "competitive_teams_count", default: 0
    t.integer "developing_teams_count", default: 0
    t.integer "inactive_teams_count", default: 0
    t.integer "youth_teams_count", default: 0
    t.integer "university_teams_count", default: 0
    t.integer "community_teams_count", default: 0
    t.integer "team_status_change_count", default: 0
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["national_governing_body_id"], name: "ngb_stats_on_ngb_id"
  end

  create_table "questions", force: :cascade do |t|
    t.integer "test_id", null: false
    t.text "description", null: false
    t.integer "points_available", default: 1, null: false
    t.text "feedback"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
  end

  create_table "referee_answers", force: :cascade do |t|
    t.bigint "referee_id", null: false
    t.bigint "test_id", null: false
    t.bigint "question_id", null: false
    t.bigint "answer_id", null: false
    t.bigint "test_attempt_id", null: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["answer_id"], name: "index_referee_answers_on_answer_id"
    t.index ["question_id"], name: "index_referee_answers_on_question_id"
    t.index ["referee_id"], name: "index_referee_answers_on_referee_id"
    t.index ["test_attempt_id"], name: "index_referee_answers_on_test_attempt_id"
    t.index ["test_id"], name: "index_referee_answers_on_test_id"
  end

  create_table "referee_certifications", force: :cascade do |t|
    t.integer "referee_id", null: false
    t.integer "certification_id", null: false
    t.datetime "received_at"
    t.datetime "revoked_at"
    t.datetime "renewed_at"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.datetime "needs_renewal_at"
    t.index ["referee_id", "certification_id"], name: "index_referee_certifications_on_referee_id_and_certification_id", unique: true, where: "(revoked_at IS NULL)"
  end

  create_table "referee_locations", force: :cascade do |t|
    t.integer "referee_id", null: false
    t.integer "national_governing_body_id", null: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["referee_id", "national_governing_body_id"], name: "index_referee_locations_on_referee_id_and_ngb_id", unique: true
  end

  create_table "referee_teams", force: :cascade do |t|
    t.bigint "team_id"
    t.bigint "referee_id"
    t.integer "association_type", default: 0
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["referee_id", "association_type"], name: "index_referee_teams_on_referee_id_and_association_type", unique: true
    t.index ["referee_id"], name: "index_referee_teams_on_referee_id"
    t.index ["team_id"], name: "index_referee_teams_on_team_id"
  end

  create_table "roles", force: :cascade do |t|
    t.bigint "user_id"
    t.integer "access_type", default: 0, null: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["user_id", "access_type"], name: "index_roles_on_user_id_and_access_type", unique: true
    t.index ["user_id"], name: "index_roles_on_user_id"
  end

  create_table "social_accounts", force: :cascade do |t|
    t.string "ownable_type"
    t.bigint "ownable_id"
    t.string "url", null: false
    t.integer "account_type", default: 0, null: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["ownable_type", "ownable_id"], name: "index_social_accounts_on_ownable_type_and_ownable_id"
  end

  create_table "team_status_changesets", force: :cascade do |t|
    t.bigint "team_id"
    t.string "previous_status"
    t.string "new_status"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["team_id"], name: "index_team_status_changesets_on_team_id"
  end

  create_table "teams", force: :cascade do |t|
    t.string "name", null: false
    t.string "city", null: false
    t.string "state"
    t.string "country", null: false
    t.integer "status", default: 0
    t.integer "group_affiliation", default: 0
    t.bigint "national_governing_body_id"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.index ["national_governing_body_id"], name: "index_teams_on_national_governing_body_id"
  end

  create_table "test_attempts", force: :cascade do |t|
    t.integer "test_id"
    t.integer "referee_id"
    t.integer "test_level"
    t.datetime "next_attempt_at"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
  end

  create_table "test_results", force: :cascade do |t|
    t.integer "cm_link_result_id"
    t.integer "referee_id", null: false
    t.time "time_started"
    t.time "time_finished"
    t.string "duration"
    t.integer "percentage"
    t.integer "points_scored"
    t.integer "points_available"
    t.boolean "passed"
    t.string "certificate_url"
    t.integer "minimum_pass_percentage"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.integer "test_level", default: 0
    t.integer "test_id"
    t.index ["referee_id"], name: "index_test_results_on_referee_id"
  end

  create_table "tests", force: :cascade do |t|
    t.integer "level", default: 0
    t.string "name"
    t.integer "certification_id"
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.text "description", null: false
    t.integer "time_limit", default: 18, null: false
    t.integer "minimum_pass_percentage", default: 80, null: false
    t.text "positive_feedback"
    t.text "negative_feedback"
    t.string "language"
    t.boolean "active", default: false, null: false
    t.integer "testable_question_count", default: 0, null: false
  end

  create_table "users", force: :cascade do |t|
    t.string "email", default: "", null: false
    t.string "encrypted_password", default: "", null: false
    t.string "reset_password_token"
    t.datetime "reset_password_sent_at"
    t.datetime "remember_created_at"
    t.integer "sign_in_count", default: 0, null: false
    t.datetime "current_sign_in_at"
    t.datetime "last_sign_in_at"
    t.inet "current_sign_in_ip"
    t.inet "last_sign_in_ip"
    t.string "first_name"
    t.string "last_name"
    t.text "bio"
    t.string "pronouns"
    t.boolean "show_pronouns", default: false
    t.datetime "created_at", null: false
    t.datetime "updated_at", null: false
    t.datetime "submitted_payment_at"
    t.datetime "getting_started_dismissed_at"
    t.boolean "admin", default: false
    t.string "confirmation_token"
    t.datetime "confirmed_at"
    t.datetime "confirmation_sent_at"
    t.integer "failed_attempts", default: 0, null: false
    t.string "unlock_token"
    t.datetime "locked_at"
    t.index ["confirmation_token"], name: "index_users_on_confirmation_token", unique: true
    t.index ["email"], name: "index_users_on_email", unique: true
    t.index ["reset_password_token"], name: "index_users_on_reset_password_token", unique: true
    t.index ["unlock_token"], name: "index_users_on_unlock_token", unique: true
  end

  add_foreign_key "national_governing_body_stats", "national_governing_bodies"
  add_foreign_key "referee_teams", "teams"
  add_foreign_key "referee_teams", "users", column: "referee_id"
  add_foreign_key "roles", "users"
  add_foreign_key "team_status_changesets", "teams"
  add_foreign_key "teams", "national_governing_bodies"
end
