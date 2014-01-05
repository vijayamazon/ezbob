IF OBJECT_ID('UiControlGroups') IS NULL
BEGIN
	CREATE TABLE UiControlGroups (
		UiControlGroupID INT NOT NULL,
		UiControlGroupName NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UiControlGroups PRIMARY KEY (UiControlGroupID),
		CONSTRAINT UC_UiControlGroups UNIQUE (UiControlGroupName),
		CONSTRAINT CHK_UiControlGroups CHECK (UiControlGroupName != '')
	)

	INSERT INTO UiControlGroups (UiControlGroupID, UiControlGroupName) VALUES
		(1, 'Personal Info'),
		(2, 'Company Info')

	CREATE TABLE UiControlGroupRelevance (
		ID INT IDENTITY(1, 1) NOT NULL,
		UiControlID INT NOT NULL,
		UiControlGroupID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UiControlGroupRelevance PRIMARY KEY (ID),
		CONSTRAINT FK_UiControlGroupRelevance_Ctrl FOREIGN KEY (UiControlID) REFERENCES UiControls(UiControlID),
		CONSTRAINT FK_UiControlGroupRelevance_Grp FOREIGN KEY (UiControlGroupID) REFERENCES UiControlGroups(UiControlGroupID),
		CONSTRAINT UC_UiControlGroupRelevance UNIQUE (UiControlID, UiControlGroupID)
	)

	SELECT CONVERT(INT, 0) AS ID, UiControlName AS Name
	INTO
		#t
	FROM
		UiControls
	WHERE
		1 = 0

	INSERT INTO #t (ID, Name) VALUES
		(1, 'first_name'),
		(1, 'gender'),
		(1, 'last_name'),
		(1, 'birth_date_day'),
		(1, 'birth_date_month'),
		(1, 'birth_date_year'),
		(1, 'consent_to_search'),
		(1, 'marital_status'),
		(1, 'middle_name'),
		(1, 'continue'),
		(1, 'own_other_property'),
		(1, 'residential_status'),
		(1, 'time_at_address'),
		(1, 'daytime_phone'),
		(1, 'mobile_phone'),
		(2, 'add_director'),
		(2, 'director_birth_date_day'),
		(2, 'director_birth_date_month'),
		(2, 'director_birth_date_year'),
		(2, 'director_email'),
		(2, 'director_first_name'),
		(2, 'director_gender'),
		(2, 'director_last_name'),
		(2, 'director_middle_name'),
		(2, 'director_phone'),
		(2, 'remove_director'),
		(2, 'type_of_business'),
		(2, 'online_turnover'),
		(2, 'overall_turnover'),
		(2, 'company_continue'),
		(2, 'employee_count'),
		(2, 'employee_count_change'),
		(2, 'top_earning_employee_count'),
		(2, 'bottom_earning_employee_count'),
		(2, 'total_monthly_salary'),
		(2, 'limited_company_name'),
		(2, 'limited_company_number'),
		(2, 'limited_phone_number'),
		(2, 'limited_property_owned_by_company'),
		(2, 'limited_rent_months_left)'),
		(2, 'limited_years_in_company'),
		(2, 'nonlimited_company_name'),
		(2, 'nonlimited_phone_number'),
		(2, 'nonlimited_property_owned_by_company'),
		(2, 'nonlimited_rent_months_left)'),
		(2, 'nonlimited_time_at_address'),
		(2, 'nonlimited_time_in_business'),
		(2, 'nonlimited_years_in_company')

	INSERT INTO UiControlGroupRelevance (UiControlGroupID, UiControlID)
	SELECT
		#t.ID,
		c.UiControlID
	FROM
		UiControls c
		INNER JOIN #t ON c.UiControlName = 'personal-info:' + #t.Name

	DROP TABLE #t
END
GO
