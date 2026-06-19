\c teamsupport_db;

CREATE TABLE IF NOT EXISTS customers (
    "CustomerId" VARCHAR(50) PRIMARY KEY,
    "CompanyName" TEXT,
    "ErpId" VARCHAR(100),
    "RazonSocial" TEXT,
    "TipoCliente" VARCHAR(100),
    "ValsoftCompany" VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS customer_tickets (
    "CustomerId" VARCHAR(50) NOT NULL,
    "TicketNumber" VARCHAR(50) NOT NULL
);

INSERT INTO customers ("CustomerId", "CompanyName", "ErpId", "RazonSocial", "TipoCliente", "ValsoftCompany")
VALUES
    ('CUST-001', 'Acme Corporation', 'ERP-ACME', 'Acme Corporation S.A.', 'Enterprise', 'Valsoft ES'),
    ('CUST-002', 'Globex Industries', 'ERP-GLOB', 'Globex Industries Ltd.', 'SMB', 'Valsoft PT'),
    ('CUST-003', 'Initech Solutions', 'ERP-INIT', 'Initech Solutions S.L.', 'Enterprise', 'Valsoft ES')
ON CONFLICT ("CustomerId") DO NOTHING;
