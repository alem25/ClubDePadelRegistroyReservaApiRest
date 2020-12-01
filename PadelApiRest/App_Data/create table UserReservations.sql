CREATE TABLE [UserReservations]
(
    id int Primary key Identity(1,1) not null,
	username varchar(50) Foreign key references dbo.[User](username),
	rsvId int Foreign key references dbo.Reservation(rsvId)
)