-- Sample data. Run after 01_create_schema.sql (which resets the identity counters, so ids are 1..5).
USE Belsoft;
GO

SET XACT_ABORT ON;

BEGIN TRANSACTION;                                                             -- ①

INSERT INTO dbo.Orders (CustomerName, Status)                                  -- ②
VALUES
    (N'Ada Lovelace',   N'Pending'),   -- Id 1
    (N'Alan Turing',    N'Paid'),      -- Id 2
    (N'Grace Hopper',   N'Pending'),   -- Id 3
    (N'Ada Lovelace',   N'Paid'),      -- Id 4: two lines
    (N'Linus Torvalds', N'Pending');   -- Id 5: no lines yet

INSERT INTO dbo.OrderLines (OrderId, ProductCode, Quantity, UnitPrice)        -- ③
VALUES
    (1, N'KB-01',  1,  49.90),
    (2, N'MON-27', 1, 120.00),
    (3, N'MUG-01', 1,  15.25),
    (4, N'KB-01',  2,  49.90),
    (4, N'MUG-01', 4,  15.25);

COMMIT TRANSACTION;                                                            -- ④
GO

SELECT * FROM dbo.Orders;                                                      -- ⑤
SELECT * FROM dbo.OrderLines;