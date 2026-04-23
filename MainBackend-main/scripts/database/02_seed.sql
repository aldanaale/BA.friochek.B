-- ============================================================
-- BA.FrioCheck — Seed Data Masivo y Robusto (DEDUPLICADO)
-- ============================================================
USE BD_FC
GO
SET NOCOUNT ON

DECLARE @PwdHash NVARCHAR(MAX) = '$2a$12$od3g/JLcOi6SO1XsyY81g.7voK41sj2TnYiBHvPg97dmkhLhX0nJe' -- DevPass123!
DECLARE @Now DATETIME2 = GETDATE()

-- ── 1. Tenants ────────────────────────────────────────────────
DECLARE @Tenants TABLE (Id UNIQUEIDENTIFIER, Name NVARCHAR(100), Slug NVARCHAR(100))
INSERT INTO @Tenants VALUES 
('07F3C367-013D-4D08-96C2-546D638F8FB8', 'Savory Chile',   'savory-chile'),
('364F88F0-AF8A-4B7A-823A-93AF0E8730B5', 'Bresler Chile',  'bresler-chile'),
('742AEE89-7259-4652-A6A8-6D2F1BAB353C', 'Coppelia Chile', 'coppelia-chile'),
('A1111111-1111-1111-1111-111111111111', 'Coca-Cola CL',   'coca-cola'),
('B2222222-2222-2222-2222-222222222222', 'Pepsi Chile',    'pepsi')

INSERT INTO dbo.Tenants (Id, Name, Slug, IsActive, CreatedAt, IsDeleted)
SELECT Id, Name, Slug, 1, @Now, 0 FROM @Tenants t
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tenants WHERE Id = t.Id)

IF NOT EXISTS (SELECT 1 FROM dbo.Tenants WHERE Slug = 'admin')
    INSERT INTO dbo.Tenants (Id, Name, Slug, IsActive, CreatedAt, IsDeleted)
    VALUES ('3F66B6EC-4915-439C-85FD-C857357408C1', 'Admin Global', 'admin', 1, @Now, 0)

-- ── 2. Usuarios de prueba fijos para tests ──────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'admin1@total.com')
    INSERT INTO dbo.Users (Id, TenantId, Email, PasswordHash, Name, LastName, Role, IsActive, IsLocked, CreatedAt, IsDeleted)
    VALUES (NEWID(), '3F66B6EC-4915-439C-85FD-C857357408C1', 'admin1@total.com', '$2a$12$MLP7.oH.vvhLG1t1vPAB1uPkw7EWPTR69YccfYis1Zk.1XkoJHd3i', 'Admin', 'Global', 5, 1, 0, @Now, 0)

-- ── 3. Usuarios por tenant ────────────────────────────────────
DECLARE @tId UNIQUEIDENTIFIER, @tSlug NVARCHAR(100)
DECLARE t_cursor CURSOR FOR SELECT Id, Slug FROM @Tenants
OPEN t_cursor
FETCH NEXT FROM t_cursor INTO @tId, @tSlug
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Crear 1 Admin, 1 Cliente, 1 Transportista, 1 Tecnico
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE TenantId = @tId AND Role = 1)
        INSERT INTO dbo.Users (Id, TenantId, Email, PasswordHash, Name, LastName, Role, IsActive, IsLocked, CreatedAt, IsDeleted)
        VALUES (NEWID(), @tId, 'admin@' + @tSlug + '.cl', @PwdHash, 'Admin', @tSlug, 1, 1, 0, @Now, 0)

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE TenantId = @tId AND Role = 2)
        INSERT INTO dbo.Users (Id, TenantId, Email, PasswordHash, Name, LastName, Role, IsActive, IsLocked, CreatedAt, IsDeleted)
        VALUES (NEWID(), @tId, 'client1@' + @tSlug + '.cl', @PwdHash, 'Cliente', @tSlug, 2, 1, 0, @Now, 0)

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE TenantId = @tId AND Role = 3)
        INSERT INTO dbo.Users (Id, TenantId, Email, PasswordHash, Name, LastName, Role, IsActive, IsLocked, CreatedAt, IsDeleted)
        VALUES (NEWID(), @tId, 'trans1@' + @tSlug + '.cl', @PwdHash, 'Transportista', @tSlug, 3, 1, 0, @Now, 0)

    FETCH NEXT FROM t_cursor INTO @tId, @tSlug
END
CLOSE t_cursor
DEALLOCATE t_cursor

-- ── 4. Perfiles y Stores ──────────────────────────────────────
INSERT INTO dbo.Transportistas (UserId, TenantId, IsAvailable, VehiclePlate, CreatedAt)
SELECT Id, TenantId, 1, 'PLATE-' + LEFT(CAST(NEWID() AS VARCHAR(36)), 6), @Now
FROM dbo.Users WHERE Role = 3 AND Id NOT IN (SELECT UserId FROM dbo.Transportistas)

IF (SELECT COUNT(*) FROM dbo.Stores) < 5
BEGIN
    INSERT INTO dbo.Stores (Id, TenantId, Name, Address, City, District, ContactName, IsActive, CreatedAt, IsDeleted)
    SELECT NEWID(), Id, 'Principal Store ' + Slug, 'Av. Siempre Viva 123', 'Santiago', 'Centro', 'Encargado', 1, @Now, 0 FROM @Tenants
END

-- ── 5. Coolers y NFC ──────────────────────────────────────────
IF (SELECT COUNT(*) FROM dbo.Coolers) < 5
BEGIN
    INSERT INTO dbo.Coolers (Id, TenantId, StoreId, Name, SerialNumber, Model, Capacity, Status, CreatedAt, IsDeleted)
    SELECT NEWID(), TenantId, Id, 'Cooler ' + Name, 'SN-' + LEFT(CAST(Id AS VARCHAR(36)), 8), 'M-2024', 500, 'Activo', @Now, 0
    FROM dbo.Stores
END

INSERT INTO dbo.NfcTags (TagId, CoolerId, SecurityHash, IsEnrolled, Status, TenantId, CreatedAt, IsDeleted)
SELECT 'TAG-' + SerialNumber, Id, 'HASH-123', 1, 'Activo', TenantId, @Now, 0
FROM dbo.Coolers WHERE Id NOT IN (SELECT CoolerId FROM dbo.NfcTags)

-- ── 6. Órdenes (Ahora con UserId y NfcTagId obligatorios) ──────
IF (SELECT COUNT(*) FROM dbo.Orders) < 5
BEGIN
    INSERT INTO dbo.Orders (Id, TenantId, UserId, CoolerId, NfcTagId, Status, DispatchDate, CreatedAt)
    SELECT 
        NEWID(), 
        c.TenantId, 
        (SELECT TOP 1 Id FROM dbo.Users WHERE TenantId = c.TenantId AND Role = 2),
        c.Id,
        n.TagId,
        'Confirmado',
        @Now,
        @Now
    FROM dbo.Coolers c
    INNER JOIN dbo.NfcTags n ON c.Id = n.CoolerId
    WHERE (SELECT TOP 1 Id FROM dbo.Users WHERE TenantId = c.TenantId AND Role = 2) IS NOT NULL
END

-- ── 7. Rutas y Paradas ────────────────────────────────────────
DECLARE @routeDate DATE = CAST(GETUTCDATE() AS DATE)

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE CAST(Date AS DATE) = @routeDate)
BEGIN
    INSERT INTO dbo.Routes (Id, TenantId, TransportistaId, Date, Status, CreatedAt)
    SELECT NEWID(), TenantId, UserId, @routeDate, 'En Proceso', @Now
    FROM dbo.Transportistas
END

IF (SELECT COUNT(*) FROM dbo.RouteStops) < 5
BEGIN
    INSERT INTO dbo.RouteStops (Id, RouteId, OrderId, StoreId, StopOrder, Status, TenantId)
    SELECT NEWID(), r.Id, o.Id, c.StoreId, 1, 'Pendiente', r.TenantId
    FROM dbo.Routes r
    INNER JOIN dbo.Orders o ON r.TenantId = o.TenantId
    INNER JOIN dbo.Coolers c ON o.CoolerId = c.Id
    WHERE CAST(r.Date AS DATE) = @routeDate
END

-- ── 8. OrderItems ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.OrderItems)
BEGIN
    INSERT INTO dbo.OrderItems (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TenantId)
    SELECT NEWID(), o.Id, NEWID(), 'Producto Demo', 12, 1200, o.TenantId
    FROM dbo.Orders o
END

PRINT 'Seed Masivo BA.FrioCheck — ¡Población Robusta Completada con Éxito!'
GO
