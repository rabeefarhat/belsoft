-- Creates the tables behind Belsoft orders.
-- Safe to re-run: drops the child table first, then the parent, then recreates both (all data is lost).
USE Belsoft;                                                                   -- ①
GO                                                                             -- ②

SET XACT_ABORT ON;

DROP TABLE IF EXISTS dbo.OrderLines;                                           -- ③
DROP TABLE IF EXISTS dbo.Orders;
GO

CREATE TABLE dbo.Orders
(
    Id           int IDENTITY(1, 1) NOT NULL,                                  -- ④
    CustomerName nvarchar(100)      NOT NULL,                                  -- ⑤
    Status       nvarchar(20)       NOT NULL
        CONSTRAINT DF_Orders_Status DEFAULT (N'Pending'),                      -- ⑥
    CreatedAt    datetimeoffset(7)  NOT NULL
        CONSTRAINT DF_Orders_CreatedAt DEFAULT (SYSDATETIMEOFFSET()),          -- ⑦

    CONSTRAINT PK_Orders PRIMARY KEY (Id),                                     -- ⑧
    CONSTRAINT CK_Orders_Status CHECK (Status IN (N'Pending', N'Paid', N'Shipped')),  -- ⑨
    CONSTRAINT CK_Orders_CustomerName_NotBlank CHECK (LEN(CustomerName) > 0)   -- ⑩
);
GO

CREATE TABLE dbo.OrderLines
(
    Id          int IDENTITY(1, 1) NOT NULL,
    OrderId     int                NOT NULL,                                   -- ⑪
    ProductCode nvarchar(20)       NOT NULL,
    Quantity    int                NOT NULL,
    UnitPrice   decimal(18, 2)     NOT NULL,                                   -- ⑫

    CONSTRAINT PK_OrderLines PRIMARY KEY (Id),
    CONSTRAINT FK_OrderLines_Orders_OrderId
        FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id)                       -- ⑬
        ON DELETE CASCADE,                                                     -- ⑭
    CONSTRAINT CK_OrderLines_Quantity CHECK (Quantity BETWEEN 1 AND 1000),     -- ⑮
    CONSTRAINT CK_OrderLines_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_OrderLines_ProductCode_NotBlank CHECK (LEN(ProductCode) > 0)
);
GO

-- SQL Server does not index foreign keys automatically; every "lines of order N" lookup needs this.
CREATE INDEX IX_OrderLines_OrderId ON dbo.OrderLines (OrderId);               -- ⑯
GO