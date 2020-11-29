CREATE TABLE Reservation
(
	rsvId int Primary key Identity(1,1) not null,
	courtId int not null,
	rsvdateTime int not null,
	rsvday varchar(50) not null,
	rsvtime varchar(50) not null
)