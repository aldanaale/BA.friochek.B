-- ============================================================
-- BA.FrioCheck — Schema Total (Idempotente)
-- Aplica sobre BD existente o nueva. Safe to re-run.
-- Incorpora: schema base + Fase1 (roles/integración) + Fase2 (Supervisores/EjecutivosComerciales)
-- ============================================================
USE BD_FC
GO
SET NOCOUNT ON
GO

-- ── [00] EF Migrations History ────────────────────────────────
IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NULL
CREATE TABLE dbo.__EFMigrationsHistory (
    MigrationId    NVARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion NVARCHAR(32)  NOT NULL
)
GO

-- ── [01] Tenants ──────────────────────────────────────────────
IF OBJECT_ID('dbo.Tenants', 'U') IS NULL
    CREATE TABLE dbo.Tenants (
        Id                    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name                  NVARCHAR(200)    NOT NULL,
        Slug                  NVARCHAR(100)    NOT NULL,
        IsActive              BIT              NOT NULL DEFAULT 1,
        IntegrationType       INT              NOT NULL DEFAULT 0,
        ExternalOrderUrl      NVARCHAR(MAX)    NULL,
        IntegrationConfigJson NVARCHAR(MAX)    NULL,
        RedirectTemplate      NVARCHAR(MAX)    NULL,
        CreatedAt             DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Tenants_Slug UNIQUE (Slug)
    )
ELSE BEGIN
    IF COL_LENGTH('dbo.Tenants','IntegrationType')       IS NULL ALTER TABLE dbo.Tenants ADD IntegrationType       INT           NOT NULL DEFAULT 0
    IF COL_LENGTH('dbo.Tenants','ExternalOrderUrl')      IS NULL ALTER TABLE dbo.Tenants ADD ExternalOrderUrl      NVARCHAR(MAX) NULL
    IF COL_LENGTH('dbo.Tenants','IntegrationConfigJson') IS NULL ALTER TABLE dbo.Tenants ADD IntegrationConfigJson NVARCHAR(MAX) NULL
    IF COL_LENGTH('dbo.Tenants','RedirectTemplate')      IS NULL ALTER TABLE dbo.Tenants ADD RedirectTemplate      NVARCHAR(MAX) NULL
END
GO

-- ── [02] Stores ───────────────────────────────────────────────
IF OBJECT_ID('dbo.Stores', 'U') IS NULL
    CREATE TABLE dbo.Stores (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId     UNIQUEIDENTIFIER NOT NULL,
        Name         NVARCHAR(255)    NOT NULL,
        Address      NVARCHAR(500)    NOT NULL,
        City         NVARCHAR(150)    NOT NULL DEFAULT '',
        District     NVARCHAR(150)    NOT NULL DEFAULT '',
        ContactName  NVARCHAR(200)    NULL,
        ContactPhone NVARCHAR(50)     NULL,
        Latitude     FLOAT            NULL,
        Longitude    FLOAT            NULL,
        IsActive     BIT              NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Stores_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Stores_TenantId' AND object_id=OBJECT_ID('dbo.Stores'))
    CREATE INDEX IX_Stores_TenantId ON dbo.Stores(TenantId)
GO

-- ── [03] Users ────────────────────────────────────────────────
-- Role: Admin=1, Cliente=2, Transportista=3, Tecnico=4, PlatformAdmin=5, Supervisor=6, EjecutivoComercial=7
-- ClientType (nullable): Retail=1, Wholesale=2, Chain=3, Horeca=4, Institutional=5, Vending=6
-- TransportType (nullable): ProductCarrier=1, MachineCarrier=2, FreightForwarder=3, LastMile=4
IF OBJECT_ID('dbo.Users', 'U') IS NULL
    CREATE TABLE dbo.Users (
        Id                       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        StoreId                  UNIQUEIDENTIFIER NULL,
        Email                    NVARCHAR(256)    NOT NULL,
        PasswordHash             NVARCHAR(MAX)    NOT NULL,
        Name                     NVARCHAR(100)    NOT NULL,
        LastName                 NVARCHAR(100)    NOT NULL,
        Role                     TINYINT          NOT NULL,
        ClientType               TINYINT          NULL,
        TransportType            TINYINT          NULL,
        IsActive                 BIT              NOT NULL DEFAULT 1,
        IsLocked                 BIT              NOT NULL DEFAULT 0,
        ActiveSessionId          NVARCHAR(MAX)    NULL,
        CurrentDeviceFingerprint NVARCHAR(MAX)    NULL,
        LastLoginAt              DATETIME2        NULL,
        CreatedAt                DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy                NVARCHAR(MAX)    NULL,
        UpdatedAt                DATETIME2        NULL,
        UpdatedBy                NVARCHAR(MAX)    NULL,
        IsDeleted                BIT              NOT NULL DEFAULT 0,
        DeletedAt                DATETIME2        NULL,
        CONSTRAINT FK_Users_Tenants      FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
        CONSTRAINT UQ_Users_Email_Tenant UNIQUE (Email, TenantId)
    )
ELSE BEGIN
    IF COL_LENGTH('dbo.Users','ClientType')    IS NULL ALTER TABLE dbo.Users ADD ClientType    TINYINT NULL
    IF COL_LENGTH('dbo.Users','TransportType') IS NULL ALTER TABLE dbo.Users ADD TransportType TINYINT NULL
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Users_TenantId' AND object_id=OBJECT_ID('dbo.Users'))
    CREATE INDEX IX_Users_TenantId ON dbo.Users(TenantId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Users_Email' AND object_id=OBJECT_ID('dbo.Users'))
    CREATE INDEX IX_Users_Email ON dbo.Users(Email)
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Users_Stores')
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Stores FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id)
GO

-- ── [04] Coolers ──────────────────────────────────────────────
IF OBJECT_ID('dbo.Coolers', 'U') IS NULL
    CREATE TABLE dbo.Coolers (
        Id                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        StoreId           UNIQUEIDENTIFIER NOT NULL,
        Name              NVARCHAR(200)    NOT NULL DEFAULT '',
        SerialNumber      NVARCHAR(100)    NOT NULL,
        Model             NVARCHAR(MAX)    NOT NULL,
        Capacity          INT              NOT NULL DEFAULT 0,
        Status            NVARCHAR(50)     NOT NULL DEFAULT 'SinAsignar',
        LastMaintenanceAt DATETIME2        NULL,
        CreatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy         NVARCHAR(MAX)    NULL,
        UpdatedAt         DATETIME2        NULL,
        UpdatedBy         NVARCHAR(MAX)    NULL,
        IsDeleted         BIT              NOT NULL DEFAULT 0,
        DeletedAt         DATETIME2        NULL,
        CONSTRAINT FK_Coolers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
        CONSTRAINT FK_Coolers_Stores  FOREIGN KEY (StoreId)  REFERENCES dbo.Stores(Id),
        CONSTRAINT CK_Coolers_Status  CHECK (Status IN (
            'SinAsignar','Activo','Inactivo','Mantenimiento','EnTransito','FallaReportada','DadoDeBaja'
        ))
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Coolers_TenantId' AND object_id=OBJECT_ID('dbo.Coolers'))
    CREATE INDEX IX_Coolers_TenantId ON dbo.Coolers(TenantId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Coolers_StoreId' AND object_id=OBJECT_ID('dbo.Coolers'))
    CREATE INDEX IX_Coolers_StoreId  ON dbo.Coolers(StoreId)
GO

-- ── [05] NfcTags ──────────────────────────────────────────────
IF OBJECT_ID('dbo.NfcTags', 'U') IS NULL
    CREATE TABLE dbo.NfcTags (
        TagId        NVARCHAR(450)    NOT NULL PRIMARY KEY,
        CoolerId     UNIQUEIDENTIFIER NOT NULL,
        SecurityHash NVARCHAR(MAX)    NOT NULL,
        IsEnrolled   BIT              NOT NULL DEFAULT 0,
        Status       NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
        EnrolledAt   DATETIME2        NULL,
        CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy    NVARCHAR(MAX)    NULL,
        UpdatedAt    DATETIME2        NULL,
        UpdatedBy    NVARCHAR(MAX)    NULL,
        IsDeleted    BIT              NOT NULL DEFAULT 0,
        DeletedAt    DATETIME2        NULL,
        CONSTRAINT FK_NfcTags_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
        CONSTRAINT CK_NfcTags_Status  CHECK (Status IN (
            'Pendiente','Instalado','Activo','Inactivo','Danado','DadoDeBaja'
        ))
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_NfcTags_CoolerId' AND object_id=OBJECT_ID('dbo.NfcTags'))
    CREATE INDEX IX_NfcTags_CoolerId ON dbo.NfcTags(CoolerId)
GO

-- ── [06] Products ─────────────────────────────────────────────
-- Stock fue eliminado en Fase1 (se usa ExternalSku para integración con mayoristas)
IF OBJECT_ID('dbo.Products', 'U') IS NULL
    CREATE TABLE dbo.Products (
        Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId    UNIQUEIDENTIFIER NOT NULL,
        Name        NVARCHAR(MAX)    NOT NULL,
        Type        NVARCHAR(50)     NOT NULL DEFAULT 'Venta',
        Price       INT              NOT NULL DEFAULT 0,
        ExternalSku NVARCHAR(MAX)    NULL,
        IsActive    BIT              NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy   NVARCHAR(MAX)    NULL,
        UpdatedAt   DATETIME2        NULL,
        UpdatedBy   NVARCHAR(MAX)    NULL,
        IsDeleted   BIT              NOT NULL DEFAULT 0,
        DeletedAt   DATETIME2        NULL,
        CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
        CONSTRAINT CK_Products_Type    CHECK (Type IN ('Venta','Servicio','Insumo'))
    )
ELSE BEGIN
    IF COL_LENGTH('dbo.Products','ExternalSku') IS NULL     ALTER TABLE dbo.Products ADD ExternalSku NVARCHAR(MAX) NULL
    IF COL_LENGTH('dbo.Products','Stock')       IS NOT NULL ALTER TABLE dbo.Products DROP COLUMN Stock
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Products_TenantId' AND object_id=OBJECT_ID('dbo.Products'))
    CREATE INDEX IX_Products_TenantId ON dbo.Products(TenantId)
GO

-- ── [07] ActiveSessions (Dapper — fuera de EF Core) ──────────
IF OBJECT_ID('dbo.ActiveSessions', 'U') IS NULL
    CREATE TABLE dbo.ActiveSessions (
        SessionId NVARCHAR(64)     NOT NULL PRIMARY KEY,
        UserId    UNIQUEIDENTIFIER NOT NULL,
        IsRevoked BIT              NOT NULL DEFAULT 0,
        ExpiresAt DATETIME2        NOT NULL,
        CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_ActiveSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ActiveSessions_UserId' AND object_id=OBJECT_ID('dbo.ActiveSessions'))
    CREATE INDEX IX_ActiveSessions_UserId ON dbo.ActiveSessions(UserId)
GO

-- ── [08] UserSessions ─────────────────────────────────────────
IF OBJECT_ID('dbo.UserSessions', 'U') IS NULL
    CREATE TABLE dbo.UserSessions (
        Id                 UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId             UNIQUEIDENTIFIER NOT NULL,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        DeviceId           NVARCHAR(MAX)    NOT NULL,
        DeviceFingerprint  NVARCHAR(MAX)    NOT NULL,
        AccessToken        NVARCHAR(MAX)    NOT NULL,
        IssuedAt           DATETIME2        NOT NULL,
        ExpiresAt          DATETIME2        NULL,
        LastActivityAt     DATETIME2        NULL,
        IsActive           BIT              NOT NULL DEFAULT 1,
        InvalidationReason NVARCHAR(MAX)    NULL,
        ClosureReason      NVARCHAR(MAX)    NULL,
        InvalidatedAt      DATETIME2        NULL,
        ClosedAt           DATETIME2        NULL,
        CreatedAt          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_UserSessions_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
        CONSTRAINT FK_UserSessions_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_UserSessions_UserId' AND object_id=OBJECT_ID('dbo.UserSessions'))
    CREATE INDEX IX_UserSessions_UserId   ON dbo.UserSessions(UserId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_UserSessions_TenantId' AND object_id=OBJECT_ID('dbo.UserSessions'))
    CREATE INDEX IX_UserSessions_TenantId ON dbo.UserSessions(TenantId)
GO

-- ── [09] PasswordResetTokens ──────────────────────────────────
IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NULL
    CREATE TABLE dbo.PasswordResetTokens (
        Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId    UNIQUEIDENTIFIER NOT NULL,
        TokenHash NVARCHAR(MAX)    NOT NULL,
        IsUsed    BIT              NOT NULL DEFAULT 0,
        ExpiresAt DATETIME2        NOT NULL,
        CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    )
GO

-- ── [10] Tecnicos ─────────────────────────────────────────────
IF OBJECT_ID('dbo.Tecnicos', 'U') IS NULL
    CREATE TABLE dbo.Tecnicos (
        UserId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId    UNIQUEIDENTIFIER NOT NULL,
        IsAvailable BIT              NOT NULL DEFAULT 1,
        Specialty   NVARCHAR(MAX)    NULL,
        CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Tecnicos_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Tecnicos_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Tecnicos_TenantId' AND object_id=OBJECT_ID('dbo.Tecnicos'))
    CREATE INDEX IX_Tecnicos_TenantId ON dbo.Tecnicos(TenantId)
GO

-- ── [11] Transportistas ───────────────────────────────────────
IF OBJECT_ID('dbo.Transportistas', 'U') IS NULL
    CREATE TABLE dbo.Transportistas (
        UserId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        IsAvailable   BIT              NOT NULL DEFAULT 1,
        VehiclePlate  NVARCHAR(MAX)    NULL,
        TransportType TINYINT          NULL,
        CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Transportistas_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Transportistas_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
    )
ELSE
    IF COL_LENGTH('dbo.Transportistas','TransportType') IS NULL
        ALTER TABLE dbo.Transportistas ADD TransportType TINYINT NULL
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Transportistas_TenantId' AND object_id=OBJECT_ID('dbo.Transportistas'))
    CREATE INDEX IX_Transportistas_TenantId ON dbo.Transportistas(TenantId)
GO

-- ── [12] Supervisores (Fase 2) ────────────────────────────────
IF OBJECT_ID('dbo.Supervisores', 'U') IS NULL
    CREATE TABLE dbo.Supervisores (
        UserId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId    UNIQUEIDENTIFIER NOT NULL,
        Zone        NVARCHAR(100)    NULL,
        IsAvailable BIT              NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Supervisores_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id)   ON DELETE NO ACTION,
        CONSTRAINT FK_Supervisores_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id) ON DELETE NO ACTION
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Supervisores_TenantId' AND object_id=OBJECT_ID('dbo.Supervisores'))
    CREATE INDEX IX_Supervisores_TenantId ON dbo.Supervisores(TenantId)
GO

-- ── [13] EjecutivosComerciales (Fase 2) ───────────────────────
IF OBJECT_ID('dbo.EjecutivosComerciales', 'U') IS NULL
    CREATE TABLE dbo.EjecutivosComerciales (
        UserId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId    UNIQUEIDENTIFIER NOT NULL,
        Territory   NVARCHAR(100)    NULL,
        IsAvailable BIT              NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_EjecutivosComerciales_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id)   ON DELETE NO ACTION,
        CONSTRAINT FK_EjecutivosComerciales_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id) ON DELETE NO ACTION
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_EjecutivosComerciales_TenantId' AND object_id=OBJECT_ID('dbo.EjecutivosComerciales'))
    CREATE INDEX IX_EjecutivosComerciales_TenantId ON dbo.EjecutivosComerciales(TenantId)
GO

-- ── [14] Orders ───────────────────────────────────────────────
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
    CREATE TABLE dbo.Orders (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        UserId          UNIQUEIDENTIFIER NOT NULL,
        CoolerId        UNIQUEIDENTIFIER NOT NULL,
        NfcTagId        NVARCHAR(450)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL DEFAULT 'PorPagar',
        DispatchDate    DATETIME2        NULL,
        ExternalOrderId NVARCHAR(MAX)    NULL,
        ExternalStatus  NVARCHAR(MAX)    NULL,
        CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Orders_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
        CONSTRAINT FK_Orders_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Orders_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
        CONSTRAINT FK_Orders_NfcTags FOREIGN KEY (NfcTagId) REFERENCES dbo.NfcTags(TagId),
        CONSTRAINT CK_Orders_Status  CHECK (Status IN (
            'PorPagar','Pagado','EnProceso','Despachado','Entregado','Cancelado','Rechazado'
        ))
    )
ELSE BEGIN
    IF COL_LENGTH('dbo.Orders','ExternalOrderId') IS NULL ALTER TABLE dbo.Orders ADD ExternalOrderId NVARCHAR(MAX) NULL
    IF COL_LENGTH('dbo.Orders','ExternalStatus')  IS NULL ALTER TABLE dbo.Orders ADD ExternalStatus  NVARCHAR(MAX) NULL
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_TenantId' AND object_id=OBJECT_ID('dbo.Orders'))
    CREATE INDEX IX_Orders_TenantId ON dbo.Orders(TenantId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_UserId' AND object_id=OBJECT_ID('dbo.Orders'))
    CREATE INDEX IX_Orders_UserId   ON dbo.Orders(UserId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_Status' AND object_id=OBJECT_ID('dbo.Orders'))
    CREATE INDEX IX_Orders_Status   ON dbo.Orders(Status)
GO

-- ── [15] OrderItems ───────────────────────────────────────────
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
    CREATE TABLE dbo.OrderItems (
        Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        OrderId     UNIQUEIDENTIFIER NOT NULL,
        ProductId   UNIQUEIDENTIFIER NOT NULL,
        ProductName NVARCHAR(MAX)    NOT NULL,
        Quantity    INT              NOT NULL,
        UnitPrice   INT              NOT NULL,
        CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_OrderItems_OrderId' AND object_id=OBJECT_ID('dbo.OrderItems'))
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId)
GO

-- ── [16] Routes ───────────────────────────────────────────────
IF OBJECT_ID('dbo.Routes', 'U') IS NULL
    CREATE TABLE dbo.Routes (
        Id             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        TransportistId UNIQUEIDENTIFIER NOT NULL,
        Date           DATETIME2        NOT NULL,
        Status         NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
        CreatedAt      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Routes_Tenants        FOREIGN KEY (TenantId)       REFERENCES dbo.Tenants(Id),
        CONSTRAINT FK_Routes_Transportistas FOREIGN KEY (TransportistId) REFERENCES dbo.Transportistas(UserId),
        CONSTRAINT CK_Routes_Status         CHECK (Status IN ('Pendiente','EnCurso','Completada','Cancelada'))
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Routes_TenantId' AND object_id=OBJECT_ID('dbo.Routes'))
    CREATE INDEX IX_Routes_TenantId       ON dbo.Routes(TenantId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Routes_TransportistId' AND object_id=OBJECT_ID('dbo.Routes'))
    CREATE INDEX IX_Routes_TransportistId ON dbo.Routes(TransportistId)
GO

-- ── [17] RouteStops ───────────────────────────────────────────
IF OBJECT_ID('dbo.RouteStops', 'U') IS NULL
    CREATE TABLE dbo.RouteStops (
        Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        RouteId   UNIQUEIDENTIFIER NOT NULL,
        OrderId   UNIQUEIDENTIFIER NOT NULL,
        StoreId   UNIQUEIDENTIFIER NOT NULL,
        StopOrder INT              NOT NULL,
        Status    NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
        ArrivalAt DATETIME2        NULL,
        Notes     NVARCHAR(500)    NULL,
        CONSTRAINT FK_RouteStops_Routes FOREIGN KEY (RouteId) REFERENCES dbo.Routes(Id),
        CONSTRAINT FK_RouteStops_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_RouteStops_Stores FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id),
        CONSTRAINT CK_RouteStops_Status CHECK (Status IN ('Pendiente','EnCamino','Llegado','Entregado','Fallido'))
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_RouteStops_RouteId' AND object_id=OBJECT_ID('dbo.RouteStops'))
    CREATE INDEX IX_RouteStops_RouteId ON dbo.RouteStops(RouteId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_RouteStops_OrderId' AND object_id=OBJECT_ID('dbo.RouteStops'))
    CREATE INDEX IX_RouteStops_OrderId ON dbo.RouteStops(OrderId)
GO

-- ── [18] Mermas ───────────────────────────────────────────────
IF OBJECT_ID('dbo.Mermas', 'U') IS NULL
    CREATE TABLE dbo.Mermas (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        TransportistId  UNIQUEIDENTIFIER NOT NULL,
        CoolerId        UNIQUEIDENTIFIER NOT NULL,
        ProductId       UNIQUEIDENTIFIER NOT NULL,
        ProductName     NVARCHAR(200)    NOT NULL,
        ScannedNfcTagId NVARCHAR(450)    NOT NULL,
        Quantity        INT              NOT NULL,
        Reason          NVARCHAR(50)     NOT NULL,
        PhotoUrl        NVARCHAR(MAX)    NOT NULL,
        Description     NVARCHAR(500)    NULL,
        CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Mermas_Tenants        FOREIGN KEY (TenantId)        REFERENCES dbo.Tenants(Id),
        CONSTRAINT FK_Mermas_Transportistas FOREIGN KEY (TransportistId)  REFERENCES dbo.Transportistas(UserId),
        CONSTRAINT FK_Mermas_Coolers        FOREIGN KEY (CoolerId)        REFERENCES dbo.Coolers(Id),
        CONSTRAINT FK_Mermas_Products       FOREIGN KEY (ProductId)       REFERENCES dbo.Products(Id),
        CONSTRAINT FK_Mermas_NfcTags        FOREIGN KEY (ScannedNfcTagId) REFERENCES dbo.NfcTags(TagId)
    )
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Mermas_TenantId' AND object_id=OBJECT_ID('dbo.Mermas'))
    CREATE INDEX IX_Mermas_TenantId ON dbo.Mermas(TenantId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Mermas_CoolerId' AND object_id=OBJECT_ID('dbo.Mermas'))
    CREATE INDEX IX_Mermas_CoolerId ON dbo.Mermas(CoolerId)
GO

-- ── [19] TechSupportRequests ──────────────────────────────────
IF OBJECT_ID('dbo.TechSupportRequests', 'U') IS NULL
    CREATE TABLE dbo.TechSupportRequests (
        Id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        UserId        UNIQUEIDENTIFIER NOT NULL,
        CoolerId      UNIQUEIDENTIFIER NOT NULL,
        NfcTagId      NVARCHAR(450)    NULL,
        FaultType     NVARCHAR(MAX)    NOT NULL,
        Description   NVARCHAR(MAX)    NOT NULL,
        PhotoUrls     NVARCHAR(MAX)    NOT NULL DEFAULT '[]',
        ScheduledDate DATETIME2        NOT NULL,
        Status        NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
        CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_TechSupport_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
        CONSTRAINT FK_TechSupport_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
        CONSTRAINT FK_TechSupport_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
        CONSTRAINT FK_TechSupport_NfcTags FOREIGN KEY (NfcTagId) REFERENCES dbo.NfcTags(TagId),
        CONSTRAINT CK_TechSupport_Status  CHECK (Status IN ('Pendiente','Asignado','EnProceso','Resuelto','Cancelado'))
    )
-- ── [20] OperationCertificates ──────────────────────────────────
IF OBJECT_ID('dbo.OperationCertificates', 'U') IS NULL
    CREATE TABLE dbo.OperationCertificates (
        Id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        OrderId             UNIQUEIDENTIFIER NOT NULL,
        UserId              UNIQUEIDENTIFIER NOT NULL,
        SignatureBase64     NVARCHAR(MAX)    NOT NULL,
        IpAddress           NVARCHAR(50)     NOT NULL,
        DeviceFingerprint   NVARCHAR(256)    NOT NULL,
        ServerHash          NVARCHAR(256)    NOT NULL,
        Latitude            FLOAT            NOT NULL,
        Longitude           FLOAT            NOT NULL,
        AcceptanceTimestamp DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt           DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy           NVARCHAR(100)    NULL,
        UpdatedAt           DATETIME2        NULL,
        UpdatedBy           NVARCHAR(100)    NULL,
        IsDeleted           BIT              NOT NULL DEFAULT 0,
        DeletedAt           DATETIME2        NULL,
        CONSTRAINT FK_Certificates_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
        CONSTRAINT FK_Certificates_Orders  FOREIGN KEY (OrderId)  REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_Certificates_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id)
    )

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Certificates_TenantId' AND object_id=OBJECT_ID('dbo.OperationCertificates'))
    CREATE INDEX IX_Certificates_TenantId ON dbo.OperationCertificates(TenantId)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Certificates_OrderId' AND object_id=OBJECT_ID('dbo.OperationCertificates'))
    CREATE INDEX IX_Certificates_OrderId ON dbo.OperationCertificates(OrderId)
GO

PRINT 'Schema BA.FrioCheck aplicado — 20 tablas OK.'
GO
