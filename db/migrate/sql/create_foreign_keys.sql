BEGIN TRANSACTION;

ALTER TABLE
  public.certification_payments
ADD CONSTRAINT certification_payments__certification_fkey FOREIGN KEY (certification_id) REFERENCES certifications(id);

ALTER TABLE
  public.certification_payments
ADD CONSTRAINT certification_payments__user_fkey FOREIGN KEY (user_id) REFERENCES users(id);

ALTER TABLE
  public.answers
ADD CONSTRAINT answers__question_fkey FOREIGN KEY (question_id) REFERENCES questions(id);

ALTER TABLE
  public.national_governing_body_admins
ADD CONSTRAINT national_governing_body_admins__national_governing_body_fkey FOREIGN KEY (national_governing_body_id) REFERENCES national_governing_bodies(id);

ALTER TABLE
  public.national_governing_body_admins
ADD CONSTRAINT national_governing_body_admins__user_fkey FOREIGN KEY (user_id) REFERENCES users(id);

ALTER TABLE
  public.national_governing_body_stats
ADD CONSTRAINT national_governing_body_stats__national_governing_body_fkey FOREIGN KEY (national_governing_body_id) REFERENCES national_governing_bodies(id);

ALTER TABLE
  public.policy_manager_portability_requests
ADD CONSTRAINT policy_manager_portability_requests__user_fkey FOREIGN KEY (user_id) REFERENCES users(id);

ALTER TABLE
  public.policy_manager_user_terms
ADD CONSTRAINT policy_manager_user_terms__user_fkey FOREIGN KEY (user_id) REFERENCES users(id);

ALTER TABLE
  public.policy_manager_user_terms
ADD CONSTRAINT policy_manager_user_terms__term_fkey FOREIGN KEY (term_id) REFERENCES policy_manager_terms(id);

ALTER TABLE
  public.questions
ADD CONSTRAINT questions__test_fkey FOREIGN KEY (test_id) REFERENCES tests(id);

ALTER TABLE
  public.referee_answers
ADD CONSTRAINT referee_answers__user_fkey FOREIGN KEY (referee_id) REFERENCES users(id);

ALTER TABLE
  public.referee_answers
ADD CONSTRAINT referee_answers__test_fkey FOREIGN KEY (test_id) REFERENCES tests(id);

ALTER TABLE
  public.referee_answers
ADD CONSTRAINT referee_answers__test_attempt_fkey FOREIGN KEY (test_attempt_id) REFERENCES test_attempts(id);

ALTER TABLE
  public.referee_answers
ADD CONSTRAINT referee_answers__question_fkey FOREIGN KEY (question_id) REFERENCES questions(id);

ALTER TABLE
  public.referee_answers
ADD CONSTRAINT referee_answers__answer_fkey FOREIGN KEY (answer_id) REFERENCES answers(id);

ALTER TABLE
  public.referee_certifications
ADD CONSTRAINT referee_certifications__user_fkey FOREIGN KEY (referee_id) REFERENCES users(id);

ALTER TABLE
  public.referee_certifications
ADD CONSTRAINT referee_certifications__certification_fkey FOREIGN KEY (certification_id) REFERENCES certifications(id);

ALTER TABLE
  public.referee_locations
ADD CONSTRAINT referee_locations__user_fkey FOREIGN KEY (referee_id) REFERENCES users(id);

ALTER TABLE
  public.referee_locations
ADD CONSTRAINT referee_locations__national_governing_body_fkey FOREIGN KEY (national_governing_body_id) REFERENCES national_governing_bodies(id);

ALTER TABLE
  public.referee_teams
ADD CONSTRAINT referee_teams__user_fkey FOREIGN KEY (referee_id) REFERENCES users(id);

ALTER TABLE
  public.referee_teams
ADD CONSTRAINT referee_teams__team_fkey FOREIGN KEY (team_id) REFERENCES teams(id);

ALTER TABLE
  public.roles
ADD CONSTRAINT roles__user_fkey FOREIGN KEY (user_id) REFERENCES users(id);

ALTER TABLE
  public.team_status_changesets
ADD CONSTRAINT team_status_changesets__team_fkey FOREIGN KEY (team_id) REFERENCES teams(id);

ALTER TABLE
  public.test_attempts
ADD CONSTRAINT test_attempts__user_fkey FOREIGN KEY (referee_id) REFERENCES users(id);

ALTER TABLE
  public.test_attempts
ADD CONSTRAINT test_attempts__test_fkey FOREIGN KEY (test_id) REFERENCES tests(id);

ALTER TABLE
  public.test_results
ADD CONSTRAINT test_results__user_fkey FOREIGN KEY (referee_id) REFERENCES users(id);

ALTER TABLE
  public.test_results
ADD CONSTRAINT test_results__test_fkey FOREIGN KEY (test_id) REFERENCES tests(id);


ALTER TABLE
  public.tests
ADD CONSTRAINT tests__certification_fkey FOREIGN KEY (certification_id) REFERENCES certifications(id);

ALTER TABLE
  public.tests
ADD CONSTRAINT tests__language_fkey FOREIGN KEY (new_language_id) REFERENCES languages(id);

ALTER TABLE
  public.users
ADD CONSTRAINT users__language_fkey FOREIGN KEY (language_id) REFERENCES languages(id);

COMMIT