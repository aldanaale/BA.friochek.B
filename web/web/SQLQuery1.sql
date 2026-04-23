
-- [00] CREAR BASE DE DATOS
-- ----------------------------------------------------------
CREATE DATABASE BD_FC

USE BD_FC


-- [01] TENANTS
-- Empresas clientes del SaaS. Raiz de todo el modelo.
-- ----------------------------------------------------------

CREATE TABLE dbo.Tenants (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Name      NVARCHAR(200)    NOT NULL,
    Slug      NVARCHAR(100)    NOT NULL,
    IsActive  BIT              NOT NULL DEFAULT 1,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Tenants_Slug UNIQUE (Slug)
)


-- [02] USERS
-- Usuarios del sistema. Cada uno pertenece a un Tenant.
-- Roles: 0 SuperAdmin | 1 Admin | 2 Cliente | 3 Transportista | 4 Tecnico

CREATE TABLE dbo.Users (
    Id                       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId                 UNIQUEIDENTIFIER NOT NULL,
    StoreId                  UNIQUEIDENTIFIER NULL,
    CreatedBy                UNIQUEIDENTIFIER NULL,
    Email                    NVARCHAR(150)    NOT NULL,
    PasswordHash             NVARCHAR(MAX)    NOT NULL,
    FullName                 NVARCHAR(100)    NOT NULL,
    Role                     TINYINT          NOT NULL,
    IsActive                 BIT              NOT NULL DEFAULT 1,
    IsLocked                 BIT              NOT NULL DEFAULT 0,
    ActiveSessionId          NVARCHAR(MAX)    NULL,
    CurrentDeviceFingerprint NVARCHAR(MAX)    NULL,
    LastLoginAt              DATETIME2        NULL,
    CreatedAt                DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Users_Tenants      FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_Users_Email_Tenant UNIQUE (Email, TenantId)
)

CREATE INDEX IX_Users_TenantId ON dbo.Users(TenantId)
CREATE INDEX IX_Users_Email    ON dbo.Users(Email)


-- [03] STORES
-- Tiendas o puntos de venta de un Tenant.
-- ----------------------------------------------------------

CREATE TABLE dbo.Stores (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER NOT NULL,
    Name         NVARCHAR(255)    NOT NULL,
    Address      NVARCHAR(500)    NOT NULL,
    ContactName  NVARCHAR(200)    NULL,
    ContactPhone NVARCHAR(50)     NULL,
    Latitude     FLOAT            NULL,
    Longitude    FLOAT            NULL,
    IsActive     BIT              NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Stores_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
)

CREATE INDEX IX_Stores_TenantId ON dbo.Stores(TenantId)

-- FK de Users hacia Stores se agrega despues de crear Stores
ALTER TABLE dbo.Users
ADD CONSTRAINT FK_Users_Stores FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id)


-- [04] COOLERS
-- Refrigeradores instalados en tiendas. Unidad central de trazabilidad NFC.
-- Estados: SinAsignar | Activo | Inactivo | Mantenimiento
--          EnTransito | FallaReportada | DadoDeBaja
-- ----------------------------------------------------------

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

    CONSTRAINT FK_Coolers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Coolers_Stores  FOREIGN KEY (StoreId)  REFERENCES dbo.Stores(Id),
    CONSTRAINT CK_Coolers_Status  CHECK (Status IN (
        'SinAsignar', 'Activo', 'Inactivo',
        'Mantenimiento', 'EnTransito', 'FallaReportada', 'DadoDeBaja'
    ))
)

CREATE INDEX IX_Coolers_TenantId ON dbo.Coolers(TenantId)
CREATE INDEX IX_Coolers_StoreId  ON dbo.Coolers(StoreId)


-- [05] NFCTAGS
-- Tags NFC fisicos vinculados a un Cooler.
-- Estados: Pendiente | Instalado | Activo | Inactivo | DaÃ±ado | DadoDeBaja
-- ----------------------------------------------------------

CREATE TABLE dbo.NfcTags (
    TagId        NVARCHAR(450)    NOT NULL PRIMARY KEY,
    CoolerId     UNIQUEIDENTIFIER NOT NULL,
    SecurityHash NVARCHAR(MAX)    NOT NULL,
    IsEnrolled   BIT              NOT NULL DEFAULT 0,
    Status       NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
    EnrolledAt   DATETIME2        NULL,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_NfcTags_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
    CONSTRAINT CK_NfcTags_Status  CHECK (Status IN (
        'Pendiente', 'Instalado', 'Activo',
        'Inactivo', 'Dañado', 'DadoDeBaja'
    ))
)

CREATE INDEX IX_NfcTags_CoolerId ON dbo.NfcTags(CoolerId)
CREATE INDEX IX_NfcTags_Status   ON dbo.NfcTags(Status)


-- [06] PRODUCTS
-- Productos disponibles por Tenant.
-- Tipos: Venta | Servicio | Insumo
-- ----------------------------------------------------------

CREATE TABLE dbo.Products (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId  UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    Type      NVARCHAR(50)     NOT NULL DEFAULT 'Venta',
    Price     INT              NOT NULL DEFAULT 0,
    Stock     INT              NOT NULL DEFAULT 0,
    IsActive  BIT              NOT NULL DEFAULT 1,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT CK_Products_Type    CHECK (Type IN ('Venta', 'Servicio', 'Insumo'))
)

CREATE INDEX IX_Products_TenantId ON dbo.Products(TenantId)


-- [07] ACTIVE SESSIONS
-- Sesiones activas en tiempo real. Controla sesion unica por dispositivo.
-- ----------------------------------------------------------

CREATE TABLE dbo.ActiveSessions (
    SessionId NVARCHAR(64)     NOT NULL PRIMARY KEY,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    IsRevoked BIT              NOT NULL DEFAULT 0,
    ExpiresAt DATETIME2        NOT NULL,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_ActiveSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
)

CREATE INDEX IX_ActiveSessions_UserId ON dbo.ActiveSessions(UserId)


-- [08] USER SESSIONS
-- Historial completo de sesiones con auditoria de dispositivos.
-- Nota: JwtToken eliminado por duplicar AccessToken.
-- ----------------------------------------------------------

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

CREATE INDEX IX_UserSessions_UserId   ON dbo.UserSessions(UserId)
CREATE INDEX IX_UserSessions_TenantId ON dbo.UserSessions(TenantId)


-- [09] PASSWORD RESET TOKENS
-- Tokens temporales para restablecimiento de contraseÃ±a.
-- ----------------------------------------------------------

CREATE TABLE dbo.PasswordResetTokens (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(MAX)    NOT NULL,
    IsUsed    BIT              NOT NULL DEFAULT 0,
    ExpiresAt DATETIME2        NOT NULL,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
)


-- [10] TECNICOS
-- Perfil extendido de usuarios con rol Tecnico (relacion 1 a 1 con Users).
-- ----------------------------------------------------------

CREATE TABLE dbo.Tecnicos (
    UserId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TenantId    UNIQUEIDENTIFIER NOT NULL,
    IsAvailable BIT              NOT NULL DEFAULT 1,
    Specialty   NVARCHAR(MAX)    NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Tecnicos_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Tecnicos_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
)

CREATE INDEX IX_Tecnicos_TenantId ON dbo.Tecnicos(TenantId)


-- [11] TRANSPORTISTAS
-- Perfil extendido de usuarios con rol Transportista (relacion 1 a 1 con Users).
-- ----------------------------------------------------------

CREATE TABLE dbo.Transportistas (
    UserId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TenantId     UNIQUEIDENTIFIER NOT NULL,
    IsAvailable  BIT              NOT NULL DEFAULT 1,
    VehiclePlate NVARCHAR(MAX)    NULL,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Transportistas_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Transportistas_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
)

CREATE INDEX IX_Transportistas_TenantId ON dbo.Transportistas(TenantId)


-- [12] ORDERS
-- Pedidos generados desde un Cooler via escaneo NFC.
-- Estados: PorPagar | Pagado | EnProceso | Despachado
--          Entregado | Cancelado | Rechazado
-- Nota: Total no se almacena, se calcula en el Service.
-- ----------------------------------------------------------

CREATE TABLE dbo.Orders (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    CoolerId     UNIQUEIDENTIFIER NOT NULL,
    NfcTagId     NVARCHAR(450)    NOT NULL,
    Status       NVARCHAR(50)     NOT NULL DEFAULT 'PorPagar',
    DispatchDate DATETIME2        NULL,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Orders_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Orders_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Orders_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
    CONSTRAINT FK_Orders_NfcTags FOREIGN KEY (NfcTagId) REFERENCES dbo.NfcTags(TagId),
    CONSTRAINT CK_Orders_Status  CHECK (Status IN (
        'PorPagar', 'Pagado', 'EnProceso',
        'Despachado', 'Entregado', 'Cancelado', 'Rechazado'
    ))
)

CREATE INDEX IX_Orders_TenantId ON dbo.Orders(TenantId)
CREATE INDEX IX_Orders_UserId   ON dbo.Orders(UserId)
CREATE INDEX IX_Orders_Status   ON dbo.Orders(Status)


-- [13] ORDER ITEMS
-- Detalle de productos dentro de un pedido.
-- Nota: Subtotal no se almacena, se calcula como Quantity * UnitPrice.
-- Nota: ProductName es snapshot historico intencional.
-- ----------------------------------------------------------

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

CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId)


-- [14] ROUTES
-- Rutas de reparto asignadas a un Transportista.
-- Estados: Pendiente | EnCurso | Completada | Cancelada
-- ----------------------------------------------------------

CREATE TABLE dbo.Routes (
    Id             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId       UNIQUEIDENTIFIER NOT NULL,
    TransportistId UNIQUEIDENTIFIER NOT NULL,
    Date           DATETIME2        NOT NULL,
    Status         NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
    CreatedAt      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Routes_Tenants        FOREIGN KEY (TenantId)       REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Routes_Transportistas FOREIGN KEY (TransportistId) REFERENCES dbo.Transportistas(UserId),
    CONSTRAINT CK_Routes_Status         CHECK (Status IN (
        'Pendiente', 'EnCurso', 'Completada', 'Cancelada'
    ))
)

CREATE INDEX IX_Routes_TenantId       ON dbo.Routes(TenantId)
CREATE INDEX IX_Routes_TransportistId ON dbo.Routes(TransportistId)


-- [15] ROUTE STOPS
-- Paradas individuales dentro de una ruta de reparto.
-- Estados: Pendiente | EnCamino | Llegado | Entregado | Fallido
-- ----------------------------------------------------------

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
    CONSTRAINT CK_RouteStops_Status CHECK (Status IN (
        'Pendiente', 'EnCamino', 'Llegado', 'Entregado', 'Fallido'
    ))
)

CREATE INDEX IX_RouteStops_RouteId ON dbo.RouteStops(RouteId)
CREATE INDEX IX_RouteStops_OrderId ON dbo.RouteStops(OrderId)


-- [16] MERMAS
-- Registro de productos perdidos o daÃ±ados en transporte o bodega.
-- Nota: ProductName es snapshot historico intencional.
-- ----------------------------------------------------------

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

CREATE INDEX IX_Mermas_TenantId ON dbo.Mermas(TenantId)
CREATE INDEX IX_Mermas_CoolerId ON dbo.Mermas(CoolerId)


-- [17] TECH SUPPORT REQUESTS
-- Solicitudes de soporte tecnico sobre fallas en Coolers.
-- Estados: Pendiente | Asignado | EnProceso | Resuelto | Cancelado
-- ----------------------------------------------------------

CREATE TABLE dbo.TechSupportRequests (
    Id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId      UNIQUEIDENTIFIER NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    CoolerId      UNIQUEIDENTIFIER NOT NULL,
    NfcTagId      NVARCHAR(450)    NULL,
    FaultType     NVARCHAR(MAX)    NOT NULL,
    Description   NVARCHAR(MAX)    NOT NULL,
    PhotoUrls     NVARCHAR(MAX)    NOT NULL,
    ScheduledDate DATETIME2        NOT NULL,
    Status        NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_TechSupport_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_TechSupport_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
    CONSTRAINT FK_TechSupport_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
    CONSTRAINT FK_TechSupport_NfcTags FOREIGN KEY (NfcTagId) REFERENCES dbo.NfcTags(TagId),
    CONSTRAINT CK_TechSupport_Status  CHECK (Status IN (
        'Pendiente', 'Asignado', 'EnProceso', 'Resuelto', 'Cancelado'
    ))
)

CREATE INDEX IX_TechSupport_TenantId ON dbo.TechSupportRequests(TenantId)
CREATE INDEX IX_TechSupport_CoolerId ON dbo.TechSupportRequests(CoolerId)
CREATE INDEX IX_TechSupport_Status   ON dbo.TechSupportRequests(Status)



-- [19] VERIFICACION FINAL
-- Muestra todas las tablas creadas con su conteo de columnas.
-- ----------------------------------------------------------
SELECT
    t.TABLE_NAME         AS Tabla,
    COUNT(c.COLUMN_NAME) AS TotalColumnas
FROM INFORMATION_SCHEMA.TABLES t
JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_SCHEMA = 'dbo'
GROUP BY t.TABLE_NAME
ORDER BY t.TABLE_NAME


-- [20] VERIFICACION FINAL
-- Muestra todas las tablas creadas con su conteo de columnas.
-- ----------------------------------------------------------
SELECT
    t.TABLE_NAME  AS Tabla,
    c.COLUMN_NAME AS Columna,
    c.DATA_TYPE   AS Tipo,
    c.CHARACTER_MAXIMUM_LENGTH AS Longitud,
    c.IS_NULLABLE AS Nullable,
    c.COLUMN_DEFAULT AS ValorDefault
FROM INFORMATION_SCHEMA.TABLES t
JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_SCHEMA = 'dbo'
ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION

--

-- LIMPIEZA
DELETE FROM dbo.PasswordResetTokens
DELETE FROM dbo.UserSessions
DELETE FROM dbo.ActiveSessions
DELETE FROM dbo.TechSupportRequests
DELETE FROM dbo.Mermas
DELETE FROM dbo.RouteStops
DELETE FROM dbo.Routes
DELETE FROM dbo.OrderItems
DELETE FROM dbo.Orders
DELETE FROM dbo.NfcTags
DELETE FROM dbo.Coolers
DELETE FROM dbo.Products
DELETE FROM dbo.Transportistas
DELETE FROM dbo.Tecnicos
DELETE FROM dbo.Users
DELETE FROM dbo.Stores
DELETE FROM dbo.Tenants


-----

DECLARE @T1 UNIQUEIDENTIFIER = NEWID()
DECLARE @T2 UNIQUEIDENTIFIER = NEWID()
DECLARE @T3 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Tenants (Id, Name, Slug, IsActive) VALUES
    (@T1, 'Coca-Cola Chile', 'coca-cola-chile', 1),
    (@T2, 'Pepsi Latam',     'pepsi-latam',      1),
    (@T3, 'RedBull Chile',   'redbull-chile',    1)

DECLARE @S1 UNIQUEIDENTIFIER = NEWID()
DECLARE @S2 UNIQUEIDENTIFIER = NEWID()
DECLARE @S3 UNIQUEIDENTIFIER = NEWID()
DECLARE @S4 UNIQUEIDENTIFIER = NEWID()
DECLARE @S5 UNIQUEIDENTIFIER = NEWID()
DECLARE @S6 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Stores (Id, TenantId, Name, Address, ContactName, ContactPhone, Latitude, Longitude, IsActive) VALUES
    (@S1, @T1, 'Minimarket El Centro',  'Av. Libertador 1234', 'Juan Perez',  '+56911111111', -33.4489, -70.6693, 1),
    (@S2, @T1, 'Botilleria San Miguel', 'Calle Gran 456',       'Maria Lopez', '+56922222222', -33.4978, -70.6614, 1),
    (@S3, @T2, 'Kiosco Providencia',    'Av. Providencia 789',  'Carlos Ruiz', '+56933333333', -33.4326, -70.6093, 1),
    (@S4, @T2, 'Almacen Las Condes',    'Av. Apoquindo 321',    'Ana Torres',  '+56944444444', -33.4103, -70.5796, 1),
    (@S5, @T3, 'Minimarket Maipu',      'Av. Pajaritos 654',    'Pedro Soto',  '+56955555555', -33.5115, -70.7584, 1),
    (@S6, @T3, 'Botilleria Nunoa',      'Av. Irarrazaval 987',  'Luisa Vera',  '+56966666666', -33.4563, -70.5986, 1)

DECLARE @U1  UNIQUEIDENTIFIER = NEWID()
DECLARE @U2  UNIQUEIDENTIFIER = NEWID()
DECLARE @U3  UNIQUEIDENTIFIER = NEWID()
DECLARE @U4  UNIQUEIDENTIFIER = NEWID()
DECLARE @U5  UNIQUEIDENTIFIER = NEWID()
DECLARE @U6  UNIQUEIDENTIFIER = NEWID()
DECLARE @U7  UNIQUEIDENTIFIER = NEWID()
DECLARE @U8  UNIQUEIDENTIFIER = NEWID()
DECLARE @U9  UNIQUEIDENTIFIER = NEWID()
DECLARE @U10 UNIQUEIDENTIFIER = NEWID()
DECLARE @U11 UNIQUEIDENTIFIER = NEWID()
DECLARE @U12 UNIQUEIDENTIFIER = NEWID()
DECLARE @U13 UNIQUEIDENTIFIER = NEWID()
DECLARE @U14 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Users (Id, TenantId, StoreId, Email, PasswordHash, FullName, Role, IsActive, IsLocked) VALUES
    (@U1,  @T1, @S1, 'admin@coca-cola.cl',  'hash_1',  'Roberto Admin',    1, 1, 0),
    (@U2,  @T2, @S3, 'admin@pepsi.cl',      'hash_2',  'Valentina Admin',  1, 1, 0),
    (@U3,  @T1, @S1, 'tec1@coca-cola.cl',   'hash_3',  'Diego Tecnico',    2, 1, 0),
    (@U4,  @T2, @S3, 'tec1@pepsi.cl',       'hash_4',  'Felipe Tecnico',   2, 1, 0),
    (@U5,  @T1, @S1, 'trans1@coca-cola.cl', 'hash_5',  'Sebastian Trans',  3, 1, 0),
    (@U6,  @T2, @S3, 'trans1@pepsi.cl',     'hash_6',  'Marcelo Trans',    3, 1, 0),
    (@U7,  @T1, @S2, 'tec2@coca-cola.cl',   'hash_7',  'Andres Tecnico',   2, 1, 0),
    (@U8,  @T2, @S4, 'tec2@pepsi.cl',       'hash_8',  'Camilo Tecnico',   2, 1, 0),
    (@U9,  @T1, @S2, 'trans2@coca-cola.cl', 'hash_9',  'Rodrigo Trans',    3, 1, 0),
    (@U10, @T2, @S4, 'trans2@pepsi.cl',     'hash_10', 'Gonzalo Trans',    3, 1, 0),
    (@U11, @T3, @S5, 'tec1@redbull.cl',     'hash_11', 'Nicolas Tecnico',  2, 1, 0),
    (@U12, @T3, @S6, 'trans1@redbull.cl',   'hash_12', 'Matias Trans',     3, 1, 0),
    (@U13, @T3, @S5, 'tec2@redbull.cl',     'hash_13', 'Cristian Tecnico', 2, 1, 0),
    (@U14, @T3, @S6, 'trans2@redbull.cl',   'hash_14', 'Ignacio Trans',    3, 1, 0)

-- Tecnicos: cada UserId unico
INSERT INTO dbo.Tecnicos (UserId, TenantId, IsAvailable, Specialty) VALUES
    (@U3,  @T1, 1, 'Refrigeracion industrial'),
    (@U4,  @T2, 1, 'Electricidad y refrigeracion'),
    (@U7,  @T1, 0, 'Mecanica general'),
    (@U8,  @T2, 1, 'Electronica'),
    (@U11, @T3, 1, 'Climatizacion'),
    (@U13, @T3, 0, 'Soldadura y tuberias')

-- Transportistas: cada UserId unico
INSERT INTO dbo.Transportistas (UserId, TenantId, IsAvailable, VehiclePlate) VALUES
    (@U5,  @T1, 1, 'ABCD12'),
    (@U6,  @T2, 1, 'EFGH34'),
    (@U9,  @T1, 1, 'IJKL56'),
    (@U10, @T2, 0, 'MNOP78'),
    (@U12, @T3, 1, 'QRST90'),
    (@U14, @T3, 0, 'UVWX11')

DECLARE @C1 UNIQUEIDENTIFIER = NEWID()
DECLARE @C2 UNIQUEIDENTIFIER = NEWID()
DECLARE @C3 UNIQUEIDENTIFIER = NEWID()
DECLARE @C4 UNIQUEIDENTIFIER = NEWID()
DECLARE @C5 UNIQUEIDENTIFIER = NEWID()
DECLARE @C6 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Coolers (Id, TenantId, StoreId, Name, SerialNumber, Model, Capacity, Status) VALUES
    (@C1, @T1, @S1, 'Cooler Entrada',    'SN-001', 'Whirlpool WRT518', 200, 'Activo'),
    (@C2, @T1, @S2, 'Cooler Bodega',     'SN-002', 'Samsung RT38K',    180, 'Inactivo'),
    (@C3, @T2, @S3, 'Cooler Principal',  'SN-003', 'LG GR-B247',       220, 'Activo'),
    (@C4, @T2, @S4, 'Cooler Secundario', 'SN-004', 'Mabe RMB520',      150, 'Mantenimiento'),
    (@C5, @T3, @S5, 'Cooler Exhibicion', 'SN-005', 'Electrolux EM75',  300, 'SinAsignar'),
    (@C6, @T3, @S6, 'Cooler Sala',       'SN-006', 'Bosch KGN39',      175, 'FallaReportada')

DECLARE @N1 NVARCHAR(450) = 'TAG-UID-001'
DECLARE @N2 NVARCHAR(450) = 'TAG-UID-002'
DECLARE @N3 NVARCHAR(450) = 'TAG-UID-003'
DECLARE @N4 NVARCHAR(450) = 'TAG-UID-004'
DECLARE @N5 NVARCHAR(450) = 'TAG-UID-005'
DECLARE @N6 NVARCHAR(450) = 'TAG-UID-006'

-- 'DadoDeBaja' reemplaza 'Danado' para evitar problemas de encoding con la n
INSERT INTO dbo.NfcTags (TagId, CoolerId, SecurityHash, IsEnrolled, Status) VALUES
    (@N1, @C1, 'hash_seg_001', 1, 'Activo'),
    (@N2, @C2, 'hash_seg_002', 1, 'Instalado'),
    (@N3, @C3, 'hash_seg_003', 1, 'Activo'),
    (@N4, @C4, 'hash_seg_004', 0, 'Pendiente'),
    (@N5, @C5, 'hash_seg_005', 0, 'Pendiente'),
    (@N6, @C6, 'hash_seg_006', 1, 'DadoDeBaja')

DECLARE @P1 UNIQUEIDENTIFIER = NEWID()
DECLARE @P2 UNIQUEIDENTIFIER = NEWID()
DECLARE @P3 UNIQUEIDENTIFIER = NEWID()
DECLARE @P4 UNIQUEIDENTIFIER = NEWID()
DECLARE @P5 UNIQUEIDENTIFIER = NEWID()
DECLARE @P6 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Products (Id, TenantId, Name, Type, Price, Stock, IsActive) VALUES
    (@P1, @T1, 'Coca-Cola 350ml',    'Venta', 990,  500, 1),
    (@P2, @T1, 'Coca-Cola 500ml',    'Venta', 1290, 300, 1),
    (@P3, @T2, 'Pepsi 350ml',        'Venta', 890,  400, 1),
    (@P4, @T2, 'Pepsi Max 500ml',    'Venta', 1190, 250, 1),
    (@P5, @T3, 'RedBull 250ml',      'Venta', 1990, 150, 1),
    (@P6, @T3, 'RedBull Sugar Free', 'Venta', 2190, 100, 1)

DECLARE @O1 UNIQUEIDENTIFIER = NEWID()
DECLARE @O2 UNIQUEIDENTIFIER = NEWID()
DECLARE @O3 UNIQUEIDENTIFIER = NEWID()
DECLARE @O4 UNIQUEIDENTIFIER = NEWID()
DECLARE @O5 UNIQUEIDENTIFIER = NEWID()
DECLARE @O6 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Orders (Id, TenantId, UserId, CoolerId, NfcTagId, Status) VALUES
    (@O1, @T1, @U1, @C1, @N1, 'PorPagar'),
    (@O2, @T1, @U1, @C1, @N1, 'Pagado'),
    (@O3, @T1, @U1, @C2, @N2, 'EnProceso'),
    (@O4, @T2, @U2, @C3, @N3, 'Despachado'),
    (@O5, @T2, @U2, @C3, @N3, 'Entregado'),
    (@O6, @T2, @U2, @C4, @N4, 'Cancelado')

INSERT INTO dbo.OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice) VALUES
    (@O1, @P1, 'Coca-Cola 350ml',  3, 990),
    (@O2, @P2, 'Coca-Cola 500ml',  2, 1290),
    (@O3, @P1, 'Coca-Cola 350ml',  5, 990),
    (@O4, @P3, 'Pepsi 350ml',      4, 890),
    (@O5, @P4, 'Pepsi Max 500ml',  1, 1190),
    (@O6, @P3, 'Pepsi 350ml',      2, 890)

DECLARE @R1 UNIQUEIDENTIFIER = NEWID()
DECLARE @R2 UNIQUEIDENTIFIER = NEWID()
DECLARE @R3 UNIQUEIDENTIFIER = NEWID()
DECLARE @R4 UNIQUEIDENTIFIER = NEWID()
DECLARE @R5 UNIQUEIDENTIFIER = NEWID()
DECLARE @R6 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.Routes (Id, TenantId, TransportistId, Date, Status) VALUES
    (@R1, @T1, @U5,  '2026-04-01 08:00', 'Completada'),
    (@R2, @T1, @U5,  '2026-04-02 08:00', 'EnCurso'),
    (@R3, @T1, @U9,  '2026-04-03 08:00', 'Pendiente'),
    (@R4, @T2, @U6,  '2026-04-01 09:00', 'Completada'),
    (@R5, @T2, @U6,  '2026-04-02 09:00', 'EnCurso'),
    (@R6, @T2, @U10, '2026-04-03 09:00', 'Pendiente')

INSERT INTO dbo.RouteStops (RouteId, OrderId, StoreId, StopOrder, Status) VALUES
    (@R1, @O1, @S1, 1, 'Entregado'),
    (@R1, @O2, @S2, 2, 'Entregado'),
    (@R2, @O3, @S1, 1, 'EnCamino'),
    (@R4, @O4, @S3, 1, 'Entregado'),
    (@R5, @O5, @S4, 1, 'Llegado'),
    (@R6, @O6, @S3, 1, 'Pendiente')

INSERT INTO dbo.Mermas (TenantId, TransportistId, CoolerId, ProductId, ProductName, ScannedNfcTagId, Quantity, Reason, PhotoUrl, Description) VALUES
    (@T1, @U5,  @C1, @P1, 'Coca-Cola 350ml', @N1, 3, 'Rotura',  'https://storage/m1.jpg', 'Botellas rotas'),
    (@T1, @U5,  @C2, @P2, 'Coca-Cola 500ml', @N2, 1, 'Vencido', 'https://storage/m2.jpg', 'Producto vencido'),
    (@T1, @U9,  @C1, @P1, 'Coca-Cola 350ml', @N1, 2, 'Rotura',  'https://storage/m3.jpg', 'Caida en descarga'),
    (@T2, @U6,  @C3, @P3, 'Pepsi 350ml',     @N3, 4, 'Derrame', 'https://storage/m4.jpg', 'Derrame por presion'),
    (@T2, @U6,  @C4, @P4, 'Pepsi Max 500ml', @N4, 2, 'Vencido', 'https://storage/m5.jpg', 'Lote vencido'),
    (@T2, @U10, @C3, @P3, 'Pepsi 350ml',     @N3, 1, 'Rotura',  'https://storage/m6.jpg', 'Rotura en almacen')

INSERT INTO dbo.TechSupportRequests (TenantId, UserId, CoolerId, NfcTagId, FaultType, Description, PhotoUrls, ScheduledDate, Status) VALUES
    (@T1, @U1, @C1, @N1, 'Falla electrica',   'Cooler no enciende',     'https://storage/t1.jpg', '2026-04-05', 'Pendiente'),
    (@T1, @U1, @C2, @N2, 'Fuga de gas',       'Perdida de temperatura', 'https://storage/t2.jpg', '2026-04-06', 'Asignado'),
    (@T1, @U3, @C1, @N1, 'Puerta danada',     'Puerta no cierra',       'https://storage/t3.jpg', '2026-04-07', 'EnProceso'),
    (@T2, @U2, @C3, @N3, 'Compresor ruidoso', 'Ruido al funcionar',     'https://storage/t4.jpg', '2026-04-05', 'Resuelto'),
    (@T2, @U2, @C4, @N4, 'Panel de control',  'Display sin temperatura','https://storage/t5.jpg', '2026-04-08', 'Pendiente'),
    (@T2, @U4, @C3, @N3, 'Termostato danado', 'Temperatura inestable',  'https://storage/t6.jpg', '2026-04-09', 'Cancelado')

INSERT INTO dbo.ActiveSessions (SessionId, UserId, IsRevoked, ExpiresAt) VALUES
    ('sess-001', @U1, 0, '2026-04-03 12:00'),
    ('sess-002', @U2, 0, '2026-04-03 13:00'),
    ('sess-003', @U3, 0, '2026-04-03 14:00'),
    ('sess-004', @U4, 1, '2026-04-02 10:00'),
    ('sess-005', @U5, 0, '2026-04-03 15:00'),
    ('sess-006', @U6, 1, '2026-04-02 11:00')

INSERT INTO dbo.UserSessions (UserId, TenantId, DeviceId, DeviceFingerprint, AccessToken, IssuedAt, ExpiresAt, IsActive) VALUES
    (@U1, @T1, 'device-001', 'fp-001', 'token-001', '2026-04-02 08:00', '2026-04-03 08:00', 1),
    (@U2, @T2, 'device-002', 'fp-002', 'token-002', '2026-04-02 09:00', '2026-04-03 09:00', 1),
    (@U3, @T1, 'device-003', 'fp-003', 'token-003', '2026-04-02 10:00', '2026-04-03 10:00', 1),
    (@U4, @T2, 'device-004', 'fp-004', 'token-004', '2026-04-01 08:00', '2026-04-02 08:00', 0),
    (@U5, @T1, 'device-005', 'fp-005', 'token-005', '2026-04-02 11:00', '2026-04-03 11:00', 1),
    (@U6, @T2, 'device-006', 'fp-006', 'token-006', '2026-04-01 09:00', '2026-04-02 09:00', 0)

INSERT INTO dbo.PasswordResetTokens (UserId, TokenHash, IsUsed, ExpiresAt) VALUES
    (@U1, 'resethash-001', 0, '2026-04-03 08:00'),
    (@U2, 'resethash-002', 1, '2026-04-01 08:00'),
    (@U3, 'resethash-003', 0, '2026-04-03 10:00'),
    (@U4, 'resethash-004', 1, '2026-04-01 09:00'),
    (@U5, 'resethash-005', 0, '2026-04-03 12:00'),
    (@U6, 'resethash-006', 0, '2026-04-03 14:00')


    -----

    -- [01] Tenants
SELECT * FROM dbo.Tenants

-- [02] Stores
SELECT * FROM dbo.Stores

-- [03] Users
SELECT * FROM dbo.Users

-- [04] Tecnicos
SELECT * FROM dbo.Tecnicos

-- [05] Transportistas
SELECT * FROM dbo.Transportistas

-- [06] Coolers
SELECT * FROM dbo.Coolers

-- [07] NfcTags
SELECT * FROM dbo.NfcTags

-- [08] Products
SELECT * FROM dbo.Products

-- [09] Orders
SELECT * FROM dbo.Orders

-- [10] OrderItems
SELECT * FROM dbo.OrderItems

-- [11] Routes
SELECT * FROM dbo.Routes

-- [12] RouteStops
SELECT * FROM dbo.RouteStops

-- [13] Mermas
SELECT * FROM dbo.Mermas

-- [14] TechSupportRequests
SELECT * FROM dbo.TechSupportRequests

-- [15] ActiveSessions
SELECT * FROM dbo.ActiveSessions

-- [16] UserSessions
SELECT * FROM dbo.UserSessions

-- [17] PasswordResetTokens
SELECT * FROM dbo.PasswordResetTokens

-- [18] Conteo general por tabla
SELECT
    t.TABLE_NAME         AS Tabla,
    COUNT(c.COLUMN_NAME) AS TotalColumnas
FROM INFORMATION_SCHEMA.TABLES t
JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_SCHEMA = 'dbo'
GROUP BY t.TABLE_NAME
ORDER BY t.TABLE_NAME

ALTER TABLE dbo.NfcTags
DROP CONSTRAINT CK_NfcTags_Status

ALTER TABLE dbo.NfcTags
ADD CONSTRAINT CK_NfcTags_Status CHECK (Status IN (
    'Pendiente', 'Instalado', 'Activo',
    'Inactivo', 'Dañado', 'DadoDeBaja'
))


