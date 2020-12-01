CREATE TABLE [User]
(
	username varchar(50) Primary key not null,
	password varchar(50) not null,
	email varchar(50) not null,
	phone varchar(12) not null,
	birthDate DateTime not null
)