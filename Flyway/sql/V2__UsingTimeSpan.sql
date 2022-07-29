﻿ALTER TABLE booking.Reservations
    DROP COLUMN Time,
    DROP COLUMN Date,
    ADD COLUMN RequestedBookingDateTime TIMESTAMP NOT NULL,
    ADD COLUMN CreatedOn TIMESTAMP NOT NULL;

ALTER TABLE booking.Bookings 
    DROP COLUMN Time,
    DROP COLUMN Date,
    ADD COLUMN BookingDateTime TIMESTAMP NOT NULL,
    ADD COLUMN CreatedOn TIMESTAMP NOT NULL;
