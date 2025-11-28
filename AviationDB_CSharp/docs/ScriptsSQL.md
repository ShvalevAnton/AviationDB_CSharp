## Создание таблиц БД Demo
```sql
-- Table: bookings.aircrafts_data

-- DROP TABLE IF EXISTS bookings.aircrafts_data;

CREATE TABLE IF NOT EXISTS bookings.aircrafts_data
(
    aircraft_code character(3) COLLATE pg_catalog."default" NOT NULL,
    model jsonb NOT NULL,
    range integer NOT NULL,
    CONSTRAINT aircrafts_pkey PRIMARY KEY (aircraft_code),
    CONSTRAINT aircrafts_range_check CHECK (range > 0)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.aircrafts_data
    OWNER to postgres;

COMMENT ON TABLE bookings.aircrafts_data
    IS 'Aircrafts (internal data)';

COMMENT ON COLUMN bookings.aircrafts_data.aircraft_code
    IS 'Aircraft code, IATA';

COMMENT ON COLUMN bookings.aircrafts_data.model
    IS 'Aircraft model';

COMMENT ON COLUMN bookings.aircrafts_data.range
    IS 'Maximal flying distance, km';


-- Table: bookings.airports_data

-- DROP TABLE IF EXISTS bookings.airports_data;

CREATE TABLE IF NOT EXISTS bookings.airports_data
(
    airport_code character(3) COLLATE pg_catalog."default" NOT NULL,
    airport_name jsonb NOT NULL,
    city jsonb NOT NULL,
    coordinates geometry(Point,4326) NOT NULL,
    timezone text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT airports_data_pkey PRIMARY KEY (airport_code)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.airports_data
    OWNER to postgres;

COMMENT ON TABLE bookings.airports_data
    IS 'Airports (internal data)';

COMMENT ON COLUMN bookings.airports_data.airport_code
    IS 'Airport code';

COMMENT ON COLUMN bookings.airports_data.airport_name
    IS 'Airport name';

COMMENT ON COLUMN bookings.airports_data.city
    IS 'City';

COMMENT ON COLUMN bookings.airports_data.coordinates
    IS 'Airport coordinates (longitude and latitude)';

COMMENT ON COLUMN bookings.airports_data.timezone
    IS 'Airport time zone';

-- Table: bookings.boarding_passes

-- DROP TABLE IF EXISTS bookings.boarding_passes;

CREATE TABLE IF NOT EXISTS bookings.boarding_passes
(
    ticket_no character(13) COLLATE pg_catalog."default" NOT NULL,
    flight_id integer NOT NULL,
    boarding_no integer NOT NULL,
    seat_no character varying(4) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT boarding_passes_pkey PRIMARY KEY (ticket_no, flight_id),
    CONSTRAINT boarding_passes_flight_id_boarding_no_key UNIQUE (flight_id, boarding_no),
    CONSTRAINT boarding_passes_flight_id_seat_no_key UNIQUE (flight_id, seat_no),
    CONSTRAINT boarding_passes_ticket_no_fkey FOREIGN KEY (ticket_no, flight_id)
        REFERENCES bookings.ticket_flights (ticket_no, flight_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.boarding_passes
    OWNER to postgres;

COMMENT ON TABLE bookings.boarding_passes
    IS 'Boarding passes';

COMMENT ON COLUMN bookings.boarding_passes.ticket_no
    IS 'Ticket number';

COMMENT ON COLUMN bookings.boarding_passes.flight_id
    IS 'Flight ID';

COMMENT ON COLUMN bookings.boarding_passes.boarding_no
    IS 'Boarding pass number';

COMMENT ON COLUMN bookings.boarding_passes.seat_no
    IS 'Seat number';

-- Table: bookings.bookings

-- DROP TABLE IF EXISTS bookings.bookings;

CREATE TABLE IF NOT EXISTS bookings.bookings
(
    book_ref character(6) COLLATE pg_catalog."default" NOT NULL,
    book_date timestamp with time zone NOT NULL,
    total_amount numeric(10,2) NOT NULL,
    CONSTRAINT bookings_pkey PRIMARY KEY (book_ref)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.bookings
    OWNER to postgres;

COMMENT ON TABLE bookings.bookings
    IS 'Bookings';

COMMENT ON COLUMN bookings.bookings.book_ref
    IS 'Booking number';

COMMENT ON COLUMN bookings.bookings.book_date
    IS 'Booking date';

COMMENT ON COLUMN bookings.bookings.total_amount
    IS 'Total booking cost';

-- Table: bookings.flights

-- DROP TABLE IF EXISTS bookings.flights;

CREATE TABLE IF NOT EXISTS bookings.flights
(
    flight_id integer NOT NULL DEFAULT nextval('flights_flight_id_seq'::regclass),
    flight_no character(6) COLLATE pg_catalog."default" NOT NULL,
    scheduled_departure timestamp with time zone NOT NULL,
    scheduled_arrival timestamp with time zone NOT NULL,
    departure_airport character(3) COLLATE pg_catalog."default" NOT NULL,
    arrival_airport character(3) COLLATE pg_catalog."default" NOT NULL,
    status character varying(20) COLLATE pg_catalog."default" NOT NULL,
    aircraft_code character(3) COLLATE pg_catalog."default" NOT NULL,
    actual_departure timestamp with time zone,
    actual_arrival timestamp with time zone,
    CONSTRAINT flights_pkey PRIMARY KEY (flight_id),
    CONSTRAINT flights_flight_no_scheduled_departure_key UNIQUE (flight_no, scheduled_departure),
    CONSTRAINT flights_aircraft_code_fkey FOREIGN KEY (aircraft_code)
        REFERENCES bookings.aircrafts_data (aircraft_code) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT flights_arrival_airport_fkey FOREIGN KEY (arrival_airport)
        REFERENCES bookings.airports_data (airport_code) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT flights_departure_airport_fkey FOREIGN KEY (departure_airport)
        REFERENCES bookings.airports_data (airport_code) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT flights_check CHECK (scheduled_arrival > scheduled_departure),
    CONSTRAINT flights_check1 CHECK (actual_arrival IS NULL OR actual_departure IS NOT NULL AND actual_arrival IS NOT NULL AND actual_arrival > actual_departure),
    CONSTRAINT flights_status_check CHECK (status::text = ANY (ARRAY['On Time'::character varying::text, 'Delayed'::character varying::text, 'Departed'::character varying::text, 'Arrived'::character varying::text, 'Scheduled'::character varying::text, 'Cancelled'::character varying::text]))
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.flights
    OWNER to postgres;

COMMENT ON TABLE bookings.flights
    IS 'Flights';

COMMENT ON COLUMN bookings.flights.flight_id
    IS 'Flight ID';

COMMENT ON COLUMN bookings.flights.flight_no
    IS 'Flight number';

COMMENT ON COLUMN bookings.flights.scheduled_departure
    IS 'Scheduled departure time';

COMMENT ON COLUMN bookings.flights.scheduled_arrival
    IS 'Scheduled arrival time';

COMMENT ON COLUMN bookings.flights.departure_airport
    IS 'Airport of departure';

COMMENT ON COLUMN bookings.flights.arrival_airport
    IS 'Airport of arrival';

COMMENT ON COLUMN bookings.flights.status
    IS 'Flight status';

COMMENT ON COLUMN bookings.flights.aircraft_code
    IS 'Aircraft code, IATA';

COMMENT ON COLUMN bookings.flights.actual_departure
    IS 'Actual departure time';

COMMENT ON COLUMN bookings.flights.actual_arrival
    IS 'Actual arrival time';

-- Table: bookings.seats

-- DROP TABLE IF EXISTS bookings.seats;

CREATE TABLE IF NOT EXISTS bookings.seats
(
    aircraft_code character(3) COLLATE pg_catalog."default" NOT NULL,
    seat_no character varying(4) COLLATE pg_catalog."default" NOT NULL,
    fare_conditions character varying(10) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT seats_pkey PRIMARY KEY (aircraft_code, seat_no),
    CONSTRAINT seats_aircraft_code_fkey FOREIGN KEY (aircraft_code)
        REFERENCES bookings.aircrafts_data (aircraft_code) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT seats_fare_conditions_check CHECK (fare_conditions::text = ANY (ARRAY['Economy'::character varying::text, 'Comfort'::character varying::text, 'Business'::character varying::text]))
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.seats
    OWNER to postgres;

COMMENT ON TABLE bookings.seats
    IS 'Seats';

COMMENT ON COLUMN bookings.seats.aircraft_code
    IS 'Aircraft code, IATA';

COMMENT ON COLUMN bookings.seats.seat_no
    IS 'Seat number';

COMMENT ON COLUMN bookings.seats.fare_conditions
    IS 'Travel class';

-- Table: bookings.spatial_ref_sys

-- DROP TABLE IF EXISTS bookings.spatial_ref_sys;

CREATE TABLE IF NOT EXISTS bookings.spatial_ref_sys
(
    srid integer NOT NULL,
    auth_name character varying(256) COLLATE pg_catalog."default",
    auth_srid integer,
    srtext character varying(2048) COLLATE pg_catalog."default",
    proj4text character varying(2048) COLLATE pg_catalog."default",
    CONSTRAINT spatial_ref_sys_pkey PRIMARY KEY (srid),
    CONSTRAINT spatial_ref_sys_srid_check CHECK (srid > 0 AND srid <= 998999)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.spatial_ref_sys
    OWNER to postgres;

REVOKE ALL ON TABLE bookings.spatial_ref_sys FROM PUBLIC;

GRANT SELECT ON TABLE bookings.spatial_ref_sys TO PUBLIC;

GRANT ALL ON TABLE bookings.spatial_ref_sys TO postgres;

-- Table: bookings.ticket_flights

-- DROP TABLE IF EXISTS bookings.ticket_flights;

CREATE TABLE IF NOT EXISTS bookings.ticket_flights
(
    ticket_no character(13) COLLATE pg_catalog."default" NOT NULL,
    flight_id integer NOT NULL,
    fare_conditions character varying(10) COLLATE pg_catalog."default" NOT NULL,
    amount numeric(10,2) NOT NULL,
    CONSTRAINT ticket_flights_pkey PRIMARY KEY (ticket_no, flight_id),
    CONSTRAINT ticket_flights_flight_id_fkey FOREIGN KEY (flight_id)
        REFERENCES bookings.flights (flight_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT ticket_flights_ticket_no_fkey FOREIGN KEY (ticket_no)
        REFERENCES bookings.tickets (ticket_no) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT ticket_flights_amount_check CHECK (amount >= 0::numeric),
    CONSTRAINT ticket_flights_fare_conditions_check CHECK (fare_conditions::text = ANY (ARRAY['Economy'::character varying::text, 'Comfort'::character varying::text, 'Business'::character varying::text]))
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.ticket_flights
    OWNER to postgres;

COMMENT ON TABLE bookings.ticket_flights
    IS 'Flight segment';

COMMENT ON COLUMN bookings.ticket_flights.ticket_no
    IS 'Ticket number';

COMMENT ON COLUMN bookings.ticket_flights.flight_id
    IS 'Flight ID';

COMMENT ON COLUMN bookings.ticket_flights.fare_conditions
    IS 'Travel class';

COMMENT ON COLUMN bookings.ticket_flights.amount
    IS 'Travel cost';

-- Table: bookings.tickets

-- DROP TABLE IF EXISTS bookings.tickets;

CREATE TABLE IF NOT EXISTS bookings.tickets
(
    ticket_no character(13) COLLATE pg_catalog."default" NOT NULL,
    book_ref character(6) COLLATE pg_catalog."default" NOT NULL,
    passenger_id character varying(20) COLLATE pg_catalog."default" NOT NULL,
    passenger_name text COLLATE pg_catalog."default" NOT NULL,
    contact_data jsonb,
    CONSTRAINT tickets_pkey PRIMARY KEY (ticket_no),
    CONSTRAINT tickets_book_ref_fkey FOREIGN KEY (book_ref)
        REFERENCES bookings.bookings (book_ref) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS bookings.tickets
    OWNER to postgres;

COMMENT ON TABLE bookings.tickets
    IS 'Tickets';

COMMENT ON COLUMN bookings.tickets.ticket_no
    IS 'Ticket number';

COMMENT ON COLUMN bookings.tickets.book_ref
    IS 'Booking number';

COMMENT ON COLUMN bookings.tickets.passenger_id
    IS 'Passenger ID';

COMMENT ON COLUMN bookings.tickets.passenger_name
    IS 'Passenger name';

COMMENT ON COLUMN bookings.tickets.contact_data
    IS 'Passenger contact information';

```