-- Combined database schema for Trading Signals application

-- 1. Watchlists
CREATE TABLE IF NOT EXISTS public.cryptos_watchlist (
    crypto varchar NOT NULL,
    is_ath bool NOT NULL,
    signal_type varchar(50) DEFAULT 'near_52w_ath' NOT NULL,
    highest_price NUMERIC,
    market_cap DOUBLE PRECISION DEFAULT 0,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT cryptos_watchlist_pkey PRIMARY KEY (crypto, signal_type),
    CONSTRAINT cryptos_watchlist_signal_type_check CHECK (signal_type IN ('near_52w_ath', 'near_ath', 'ema9_above_ema21'))
);

CREATE TABLE IF NOT EXISTS public.forex_watchlist (
    pair varchar(10) NOT NULL,
    action varchar(10) NOT NULL,
    score_diff numeric(10, 2) NOT NULL,
    note text NULL,
    updated_at timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT forex_watchlist_pkey PRIMARY KEY (pair)
);

CREATE TABLE IF NOT EXISTS public.symbols_watchlist (
    symbol varchar NOT NULL,
    signal_type varchar(50) DEFAULT 'near_52w_ath' NOT NULL,
    volume bigint DEFAULT 0 NOT NULL,
    highest_price int8 NULL,
    lowest_price int8 NULL,
    auto_trade bool DEFAULT false NOT NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT symbols_watchlist_pkey PRIMARY KEY (symbol, signal_type),
    CONSTRAINT symbols_watchlist_signal_type_check CHECK (signal_type IN ('near_52w_ath', 'ema9_above_ema21', 'top_growth_20d'))
);
CREATE INDEX IF NOT EXISTS idx_symbols_watchlist_signal_type ON public.symbols_watchlist (signal_type);
CREATE INDEX IF NOT EXISTS idx_symbols_watchlist_volume ON public.symbols_watchlist (volume DESC);

CREATE TABLE IF NOT EXISTS public.world_symbols_watchlist (
    symbol varchar NOT NULL,
    country varchar NOT NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_symbol_country UNIQUE (symbol, country)
);

CREATE TABLE IF NOT EXISTS public.price_alerts (
    symbol varchar NOT NULL,
    asset_type varchar(20) NOT NULL CHECK (asset_type IN ('crypto', 'stock', 'gold', 'silver', 'forex')),
    alert_price numeric(20, 8) NOT NULL,
    operator varchar(2) DEFAULT '<=' NOT NULL CHECK (operator IN ('<=', '>=')),
    is_active bool DEFAULT true NOT NULL,
    last_notified_at timestamptz NULL,
    created_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT price_alerts_pkey PRIMARY KEY (symbol, asset_type)
);
CREATE INDEX IF NOT EXISTS idx_price_alerts_active ON public.price_alerts(is_active, asset_type);

CREATE TABLE IF NOT EXISTS public.futures_watchlist (
    symbol varchar NOT NULL,
    signal_type varchar(50) DEFAULT 'near_52w_high' NOT NULL,
    highest_price numeric(20, 8) NULL,
    market_cap numeric(20, 8) DEFAULT 0 NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT futures_watchlist_pkey PRIMARY KEY (symbol, signal_type),
    CONSTRAINT futures_watchlist_signal_type_check CHECK (signal_type IN ('near_52w_high', 'ema9_above_ema21'))
);

-- 2. User Info & Trading
CREATE TABLE IF NOT EXISTS public.user_info (
    id varchar NOT NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    otp int8 NULL,
    CONSTRAINT user_info_pkey PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS trading_news_signals (
    id SERIAL PRIMARY KEY,
    content TEXT NOT NULL,
    raw_prompt TEXT,
    model_used VARCHAR(50),
    status VARCHAR(20) DEFAULT 'done',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_created_at ON trading_news_signals(created_at DESC);

-- 3. Journal & Community
CREATE TABLE IF NOT EXISTS journal_entries (
    id SERIAL PRIMARY KEY,
    user_id VARCHAR(255) NOT NULL,
    asset_type VARCHAR(50) NOT NULL,
    symbol VARCHAR(50),
    quantity DOUBLE PRECISION NOT NULL,
    price DOUBLE PRECISION NOT NULL,
    current_price DOUBLE PRECISION,
    currency VARCHAR(10) NOT NULL DEFAULT 'VND',
    entry_date TIMESTAMP NOT NULL,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_journal_user_id ON journal_entries(user_id);

CREATE TABLE IF NOT EXISTS public.community_posts (
    id SERIAL PRIMARY KEY,
    user_id VARCHAR NOT NULL,
    user_name VARCHAR NOT NULL,
    user_code VARCHAR NOT NULL,
    content TEXT NOT NULL,
    image TEXT,
    likes INT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_community_posts_created_at ON community_posts(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_community_posts_user_id ON community_posts(user_id);

CREATE TABLE IF NOT EXISTS public.community_comments (
    id SERIAL PRIMARY KEY,
    post_id INT NOT NULL REFERENCES community_posts(id) ON DELETE CASCADE,
    user_id VARCHAR NOT NULL,
    user_name VARCHAR NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_community_comments_post_id ON community_comments(post_id);

-- 4. Real Estate & System Settings
CREATE TABLE IF NOT EXISTS real_estate_prices (
    id SERIAL PRIMARY KEY,
    region TEXT NOT NULL,
    location TEXT,
    price_text TEXT,
    price_numeric BIGINT,
    property_type TEXT,
    area FLOAT DEFAULT 0,
    url TEXT,
    fetched_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_real_estate_region ON real_estate_prices(region);
CREATE INDEX IF NOT EXISTS idx_real_estate_type ON real_estate_prices(property_type);
CREATE INDEX IF NOT EXISTS idx_real_estate_fetched_at ON real_estate_prices(fetched_at);

CREATE TABLE IF NOT EXISTS public.system_settings (
    key varchar(50) PRIMARY KEY,
    value varchar(255) NOT NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL
);
INSERT INTO public.system_settings (key, value) VALUES 
('scan_stock_vn', 'true'),
('scan_stock_us', 'true'),
('scan_crypto', 'true'),
('scan_futures', 'true'),
('ai_enabled', 'true'),
('ai_prompt_template', '')
ON CONFLICT (key) DO NOTHING;

CREATE TABLE IF NOT EXISTS public.triggered_alerts (
    id SERIAL PRIMARY KEY,
    asset_type varchar(20) NOT NULL,
    symbol varchar(50) NOT NULL,
    price numeric(20, 8) NOT NULL,
    message text NOT NULL,
    is_read bool DEFAULT false NOT NULL,
    created_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_triggered_alerts_unread ON public.triggered_alerts(is_read);

-- 5. Macro Intelligence & OSINT
CREATE TABLE IF NOT EXISTS news_groups (
    id SERIAL PRIMARY KEY,
    user_id VARCHAR(255) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    conclusion TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_news_groups_user_id ON news_groups(user_id);

CREATE TABLE IF NOT EXISTS news_items (
    id SERIAL PRIMARY KEY,
    group_id INTEGER NOT NULL REFERENCES news_groups(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    source_url TEXT,
    importance INTEGER CHECK (importance BETWEEN 1 AND 5) NOT NULL,
    status VARCHAR(10) CHECK (status IN ('active', 'expired')) DEFAULT 'active',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS osint_signals (
    id VARCHAR(255) PRIMARY KEY,
    source_news_id INTEGER NOT NULL REFERENCES news_items(id) ON DELETE CASCADE,
    category VARCHAR(255) NOT NULL,
    signal TEXT NOT NULL,
    confidence DOUBLE PRECISION NOT NULL,
    reason TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS osint_theses (
    id VARCHAR(255) PRIMARY KEY,
    thesis TEXT NOT NULL,
    confidence DOUBLE PRECISION NOT NULL,
    supporting_evidence TEXT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'active',
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS osint_world_state (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    state_json JSONB NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS osint_proposed_changes (
    id VARCHAR(255) PRIMARY KEY,
    target_entity VARCHAR(255) NOT NULL,
    field_name VARCHAR(255) NOT NULL,
    old_value TEXT,
    new_value TEXT NOT NULL,
    confidence DOUBLE PRECISION NOT NULL,
    reason TEXT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
