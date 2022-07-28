CREATE SCHEMA booking

CREATE TABLE booking.Locations (
    Id UUID PRIMARY KEY,
    Name VARCHAR(400) NOT NULL
)

CREATE TABLE booking.Reservations (
    Id UUID PRIMARY KEY,
    Date DATE NOT NULL,
    Time TIME NOT NULL,
    Seats NUMERIC NOT NULL,
    SpecialRequest VARCHAR(400),
    LocationId UUID NOT NULL,
    Status VARCHAR(400) NOT NULL,
    CONSTRAINT FK_Reservation_Location FOREIGN KEY(LocationId) REFERENCES Locations(Id)
)
    
CREATE TABLE booking.Bookings (
    Id UUID PRIMARY KEY,
    Date DATE NOT NULL,
    Time TIME NOT NULL,
    Seats NUMERIC NOT NULL,
    SpecialRequest VARCHAR(400),
    LocationId UUID NOT NULL,
    ReservationId UUID NOT NULL,
    CONSTRAINT  FK_Booking_Reservation FOREIGN KEY(ReservationId) REFERENCES Reservations(Id),
    CONSTRAINT FK_Booking_Location FOREIGN KEY(LocationId) REFERENCES Locations(Id)
)