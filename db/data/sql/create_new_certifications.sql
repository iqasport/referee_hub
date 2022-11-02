-- for new version of the rulebook ##VERSION## copies existing certification details (level, display_name)
-- and inserts the new certifications
INSERT INTO certifications (level, display_name, created_at, updated_at, version)
Select level, display_name, now()as created_at, now() as updated_at, ##VERSION## as version from 
	(select level, display_name from certifications
	group by level, display_name) 
