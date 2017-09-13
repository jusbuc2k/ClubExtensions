CREATE SCHEMA ClubEx authorization dbo;

CREATE TABLE ClubEx.PersonCache (
	[PersonID] varchar(25) NOT NULL,
	[FirstName] nvarchar(100) NOT NULL,
	[LastName] nvarchar(100) NOT NULL,
	[Name] nvarchar(250) NOT NULL,
	[BirthDate] nvarchar(100) datetime NULL,
	[Grade] int NULL,
	[Gender] char(1) NULL,
	[MedicalNotes] nvarchar(max) NULL,
	[CreatedAt] datetime NULL,
	[UpdatedAt] datetime NULL,
	[RecordCreatedDateTime] datetimeoffset NOT NULL,
	[RecordRecordUpdatedDateTime] datetimeoffset NOT NULL,
	CONSTRAINT PK_PersonCache PRIMARY KEY CLUSTERED ([PersonID])
);
GO

CREATE TABLE ClubEx.HouseholdCache (
	[HouseholdID] varchar(25) NOT NULL,
	[Name] nvarchar(250) NOT NULL,
	[PrimaryContactID] varchar(25) NULL,
	[PrimaryContactName] varchar(250) NULL,
	[RecordCreatedDateTime] datetimeoffset NOT NULL,
	[RecordUpdatedDateTime] datetimeoffset NOT NULL,
	CONSTRAINT PK_HouseholdCache PRIMARY KEY CLUSTERED ([HouseholdID])
);
GO

CREATE TABLE ClubEx.HouseholdPersonCache (
	[HouseholdID] varchar(25) NOT NULL,
	[PersonID] varchar(25) NOT NULL,
	[RecordCreatedDateTime] datetimeoffset NOT NULL,
	CONSTRAINT PK_HouseholdPerson PRIMARY KEY CLUSTERED ([HouseholdID],[PersonID])
);
GO