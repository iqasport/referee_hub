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

ActiveRecord::Schema.define(version: 2019_07_07_123227) do

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

  create_table "referees", force: :cascade do |t|
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
    t.index ["email"], name: "index_referees_on_email", unique: true
    t.index ["reset_password_token"], name: "index_referees_on_reset_password_token", unique: true
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

end
