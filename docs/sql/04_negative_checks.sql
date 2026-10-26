-- Each block below is SUPPOSED to fail (or to prove a point). Run them one at a time.
USE Belsoft;
GO

-- N1. Foreign key: a line for an order that doesn't exist.
INSERT INTO dbo.OrderLines (OrderId, ProductCode, Quantity, UnitPrice)
VALUES (9999, N'KB-01', 1, 49.90);                                             -- ①
GO

-- N2. CHECK constraints: an unknown status, a zero quantity, a blank name.
INSERT INTO dbo.Orders (CustomerName, Status) VALUES (N'Eve', N'Cancelled');
GO
INSERT INTO dbo.OrderLines (OrderId, ProductCode, Quantity, UnitPrice) VALUES (1, N'KB-01', 0, 49.90);
GO
INSERT INTO dbo.Orders (CustomerName) VALUES (N'   ');
GO

-- N3. Atomicity: the second line breaks a CHECK, so the WHOLE order must disappear.
SET XACT_ABORT ON;                                                             -- ②
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO dbo.Orders (CustomerName) VALUES (N'Half Order');
        DECLARE @newOrderId int = SCOPE_IDENTITY();                            -- ③
        INSERT INTO dbo.OrderLines (OrderId, ProductCode, Quantity, UnitPrice)
        VALUES (@newOrderId, N'KB-01', 1, 49.90);
        INSERT INTO dbo.OrderLines (OrderId, ProductCode, Quantity, UnitPrice)
        VALUES (@newOrderId, N'MUG-01', 0, 15.25);                             -- breaks CK_OrderLines_Quantity
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;                                   -- ④
    PRINT CONCAT(N'Rolled back: ', ERROR_MESSAGE());
END CATCH;
SELECT COUNT(*) AS HalfOrders FROM dbo.Orders WHERE CustomerName = N'Half Order';   -- 0
GO

-- N4. SQL injection: why parameters matter.
DECLARE @customer nvarchar(200) = N''' OR 1=1 --';                            -- ⑤

-- UNSAFE: user input glued into the SQL text.
DECLARE @unsafeSql nvarchar(max) =
    N'SELECT Id, CustomerName FROM dbo.Orders WHERE CustomerName = ''' + @customer + N'''';
PRINT @unsafeSql;                                                              -- ⑥
EXEC (@unsafeSql);                                                             -- every order comes back

-- SAFE: the SQL text and the value travel separately.
EXEC sys.sp_executesql                                                         -- ⑦
    N'SELECT Id, CustomerName FROM dbo.Orders WHERE CustomerName = @name',
    N'@name nvarchar(100)',
    @name = @customer;                                                         -- zero rows
GO