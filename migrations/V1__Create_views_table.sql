CREATE TABLE views (
    id BIGSERIAL PRIMARY KEY,
    product_id BIGINT NOT NULL,
    timestamp timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC')
);

CREATE TABLE payments (
    id BIGSERIAL PRIMARY KEY,
    product_id BIGINT NOT NULL,
    timestamp timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    amount DECIMAL NOT NULL
);

CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY,
    created_at timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    event_type VARCHAR(100) NOT NULL,
    payload TEXT NOT NULL,
    processed BOOLEAN NOT NULL DEFAULT FALSE,
    processed_at timestamptz
);

CREATE TABLE reports (
    id UUID PRIMARY KEY,
    product_id BIGINT NOT NULL,
    start_gap timestamptz NOT NULL,
    end_gap timestamptz NOT NULL,
    status VARCHAR(20) NOT NULL,
    decoration_id VARCHAR(20) NOT NULL,
    ratio DOUBLE PRECISION,
    payments_count INTEGER,
    created_at timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC')
);

CREATE TABLE billing_reports (
    id UUID PRIMARY KEY,
    user_id VARCHAR(255) NOT NULL,
    last_payment_time timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC')
);

CREATE TABLE billing_operations (
    id UUID PRIMARY KEY,
    user_id VARCHAR(255) NOT NULL,
    operation_time timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    amount DECIMAL(10, 2) NOT NULL,
    description TEXT
);