USE BA_Backend_DB;
GO

-- ============================================================
-- Seed de datos para cliente@test.com (tenant admin)
-- Este script crea los registros necesarios para probar:
--   GET /api/v1/cliente/home
--   GET /api/v1/cliente/products
--   POST /api/v1/cliente/orders
--   POST /api/v1/cliente/orders/{id}/items
--   POST /api/v1/cliente/orders/{id}/confirm
--   POST /api/v1/cliente/tech-support
--   POST /api/v1/cliente/nfc/report-damaged
-- ============================================================

DECLARE @TenantId UNIQUEIDENTIFIER;
DECLARE @UserId UNIQUEIDENTIFIER;
DECLARE @StoreId UNIQUEIDENTIFIER;
DECLARE @CoolerId UNIQUEIDENTIFIER;
DECLARE @NfcTagId NVARCHAR(450) = 'NFC-CLIENTE-001';
DECLARE @OrderNfcTagId UNIQUEIDENTIFIER = NEWID();
DECLARE @Product1Id UNIQUEIDENTIFIER = 'a1111111-1111-1111-1111-111111111111';
DECLARE @Product2Id UNIQUEIDENTIFIER = 'a2222222-2222-2222-2222-222222222222';
DECLARE @Product3Id UNIQUEIDENTIFIER = 'a3333333-3333-3333-3333-333333333333';
DECLARE @OrderId UNIQUEIDENTIFIER = 'b1111111-1111-1111-1111-111111111111';
DECLARE @OrderItemId UNIQUEIDENTIFIER = 'c1111111-1111-1111-1111-111111111111';
DECLARE @TechSupportId UNIQUEIDENTIFIER = 'd1111111-1111-1111-1111-111111111111';

SELECT @TenantId = Id FROM dbo.Tenants WHERE Slug = 'admin';
SELECT @UserId = Id FROM dbo.Users WHERE Email = 'cliente@test.com' AND TenantId = @TenantId;

IF @TenantId IS NULL
BEGIN
    RAISERROR('Tenant admin no existe. Asegure que la base tenga un tenant con slug admin.', 16, 1);
    RETURN;
END;

IF @UserId IS NULL
BEGIN
    RAISERROR('Usuario cliente@test.com no existe dentro del tenant admin.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Stores WHERE TenantId = @TenantId AND Name = 'Tienda Cliente Test')
BEGIN
    SET @StoreId = NEWID();
    INSERT INTO dbo.Stores (Id, TenantId, Name, Address, ContactName, ContactPhone, IsActive, CreatedAt)
    VALUES (@StoreId, @TenantId, N'Tienda Cliente Test', N'Av. Cliente 123', N'Cliente Test', N'+56 9 7777 7777', 1, GETDATE());
END
ELSE
BEGIN
    SELECT @StoreId = Id FROM dbo.Stores WHERE TenantId = @TenantId AND Name = 'Tienda Cliente Test';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Coolers WHERE StoreId = @StoreId AND SerialNumber = 'CL-CLIENTE-001')
BEGIN
    SET @CoolerId = NEWID();
    INSERT INTO dbo.Coolers (Id, StoreId, SerialNumber, Model, Capacity, Status, CreatedAt, LastMaintenanceAt)
    VALUES (@CoolerId, @StoreId, 'CL-CLIENTE-001', 'Cooler Cliente', 120, 'Operativo', GETDATE(), GETDATE());
END
ELSE
BEGIN
    SELECT @CoolerId = Id FROM dbo.Coolers WHERE StoreId = @StoreId AND SerialNumber = 'CL-CLIENTE-001';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.NfcTags WHERE TagId = @NfcTagId)
BEGIN
    INSERT INTO dbo.NfcTags (TagId, CoolerId, SecurityHash, IsEnrolled, CreatedAt, EnrolledAt)
    VALUES (@NfcTagId, @CoolerId, 'hash-cliente-001', 1, GETDATE(), GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = @Product1Id)
BEGIN
    INSERT INTO dbo.Products (Id, TenantId, Name, Type, Price, Stock, IsActive, CreatedAt)
    VALUES
        (@Product1Id, @TenantId, N'Agua Cliente 500ml', 'venta', 990, 100, 1, GETDATE()),
        (@Product2Id, @TenantId, N'Refresco Cliente 350ml', 'venta', 1250, 80, 1, GETDATE()),
        (@Product3Id, @TenantId, N'Jugo Cliente 250ml', 'venta', 1500, 60, 1, GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Id = @OrderId)
BEGIN
    INSERT INTO dbo.Orders (Id, TenantId, UserId, CoolerId, NfcTagId, Status, Total, DispatchDate, CreatedAt)
    VALUES (@OrderId, @TenantId, @UserId, @CoolerId, @OrderNfcTagId, N'PorPagar', 0, NULL, GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OrderItems WHERE Id = @OrderItemId)
BEGIN
    INSERT INTO dbo.OrderItems (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal)
    VALUES (@OrderItemId, @OrderId, @Product1Id, N'Agua Cliente 500ml', 1, 990, 990);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TechSupportRequests WHERE Id = @TechSupportId)
BEGIN
    INSERT INTO dbo.TechSupportRequests (Id, TenantId, UserId, NfcTagId, CoolerId, FaultType, Description, PhotoUrls, ScheduledDate, Status, CreatedAt)
    VALUES (@TechSupportId, @TenantId, @UserId, NULL, @CoolerId, N'Mantenimiento', N'Solicitud de soporte demostración para cliente@test.com', N'[]', DATEADD(day, 3, GETDATE()), N'Pendiente', GETDATE());
END;

PRINT 'Datos creados para cliente@test.com';
PRINT 'Tenant: admin';
PRINT 'Usuario: cliente@test.com';
PRINT 'StoreId: ' + CONVERT(NVARCHAR(36), @StoreId);
PRINT 'CoolerId: ' + CONVERT(NVARCHAR(36), @CoolerId);
PRINT 'NfcUid: NFC-CLIENTE-001';
PRINT 'OrderId: ' + CONVERT(NVARCHAR(36), @OrderId);
PRINT 'TechSupportId: ' + CONVERT(NVARCHAR(36), @TechSupportId);
GO
