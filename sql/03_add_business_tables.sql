USE BA_Backend_DB;
GO

-- ============================================================
-- STORES (Tiendas/Puntos de venta)
-- ============================================================
CREATE TABLE dbo.Stores (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER NOT NULL,
    Name         NVARCHAR(200)    NOT NULL,
    Address      NVARCHAR(500)    NULL,
    City         NVARCHAR(100)    NULL,
    Comuna       NVARCHAR(100)    NULL,
    ContactName  NVARCHAR(200)    NULL,
    ContactPhone NVARCHAR(50)     NULL,
    IsActive     BIT              NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Stores_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
);
GO

CREATE INDEX IX_Stores_TenantId ON dbo.Stores(TenantId);
GO

-- ============================================================
-- COOLERS (Máquinas/Refrigeradores)
-- ============================================================
CREATE TABLE dbo.Coolers (
    Id             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId       UNIQUEIDENTIFIER NOT NULL,
    StoreId        UNIQUEIDENTIFIER NOT NULL,
    SerialNumber   NVARCHAR(100)    NULL,
    Model          NVARCHAR(100)    NULL,
    Capacity       INT              NOT NULL DEFAULT 0,
    Status         NVARCHAR(50)     NOT NULL DEFAULT 'Activo',
    IsActive       BIT              NOT NULL DEFAULT 1,
    InstalledAt    DATETIME2        NULL,
    LastRevisionAt DATETIME2        NULL,
    CreatedAt      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Coolers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Coolers_Stores  FOREIGN KEY (StoreId)  REFERENCES dbo.Stores(Id)
);
GO

CREATE INDEX IX_Coolers_TenantId ON dbo.Coolers(TenantId);
CREATE INDEX IX_Coolers_StoreId  ON dbo.Coolers(StoreId);
GO

-- ============================================================
-- NFC TAGS
-- ============================================================
CREATE TABLE dbo.NfcTags (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER NOT NULL,
    StoreId      UNIQUEIDENTIFIER NOT NULL,
    CoolerId     UNIQUEIDENTIFIER NULL,
    NfcUid       NVARCHAR(100)    NOT NULL,
    SecurityHash NVARCHAR(512)    NOT NULL,
    Status       NVARCHAR(50)     NOT NULL DEFAULT 'Pendiente',
    IsActive     BIT              NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    InstalledAt  DATETIME2        NULL,
    CONSTRAINT FK_NfcTags_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_NfcTags_Stores  FOREIGN KEY (StoreId)  REFERENCES dbo.Stores(Id),
    CONSTRAINT FK_NfcTags_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
    CONSTRAINT UQ_NfcTags_Uid_Tenant UNIQUE (NfcUid, TenantId)
);
GO

CREATE INDEX IX_NfcTags_NfcUid   ON dbo.NfcTags(NfcUid);
CREATE INDEX IX_NfcTags_TenantId ON dbo.NfcTags(TenantId);
CREATE INDEX IX_NfcTags_Status   ON dbo.NfcTags(Status);
GO

-- ============================================================
-- PRODUCTS (Productos)
-- ============================================================
CREATE TABLE dbo.Products (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId  UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(200)    NOT NULL,
    Type      NVARCHAR(50)     NOT NULL DEFAULT 'venta',
    Price     INT              NOT NULL DEFAULT 0,
    Stock     INT              NOT NULL DEFAULT 0,
    IsActive  BIT              NOT NULL DEFAULT 1,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
);
GO

CREATE INDEX IX_Products_TenantId ON dbo.Products(TenantId);
GO

-- ============================================================
-- ORDERS (Pedidos)
-- ============================================================
CREATE TABLE dbo.Orders (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    CoolerId     UNIQUEIDENTIFIER NOT NULL,
    NfcTagId     UNIQUEIDENTIFIER NOT NULL,
    Status       NVARCHAR(50)     NOT NULL DEFAULT 'PorPagar',
    Total        INT              NOT NULL DEFAULT 0,
    DispatchDate DATETIME2        NULL,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Orders_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Orders_Users   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Orders_Coolers FOREIGN KEY (CoolerId) REFERENCES dbo.Coolers(Id),
    CONSTRAINT FK_Orders_NfcTags FOREIGN KEY (NfcTagId) REFERENCES dbo.NfcTags(Id)
);
GO

CREATE INDEX IX_Orders_TenantId ON dbo.Orders(TenantId);
CREATE INDEX IX_Orders_UserId   ON dbo.Orders(UserId);
CREATE INDEX IX_Orders_Status   ON dbo.Orders(Status);
GO

-- ============================================================
-- ORDER ITEMS (Detalle del pedido)
-- ============================================================
CREATE TABLE dbo.OrderItems (
    Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    OrderId     UNIQUEIDENTIFIER NOT NULL,
    ProductId   UNIQUEIDENTIFIER NOT NULL,
    ProductName NVARCHAR(200)    NOT NULL,
    Quantity    INT              NOT NULL,
    UnitPrice   INT              NOT NULL,
    Subtotal    INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO

CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
GO

-- ============================================================
-- SEED DATA
-- ============================================================
DECLARE @AdminTenantId UNIQUEIDENTIFIER;
SELECT @AdminTenantId = Id FROM dbo.Tenants WHERE Slug = 'admin';

INSERT INTO dbo.Products (TenantId, Name, Type, Price, Stock)
VALUES
    (@AdminTenantId, 'Agua Mineral 500ml', 'venta', 990,  100),
    (@AdminTenantId, 'Refresco 350ml',     'venta', 1250,  80),
    (@AdminTenantId, 'Jugo Natural 250ml', 'venta', 1500,  60);
GO