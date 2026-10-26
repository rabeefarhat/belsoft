-- Everyday queries over Belsoft orders. Read-only: safe to run any time.
USE Belsoft;
GO

-- Q1. Orders that are still pending, newest first.
SELECT o.Id, o.CustomerName, o.Status, o.CreatedAt                             -- ①
FROM dbo.Orders AS o
WHERE o.Status = N'Pending'
ORDER BY o.Id DESC;

-- Q2. Orders with their lines (INNER JOIN: only orders that have lines).
SELECT o.Id AS OrderId, o.CustomerName, l.ProductCode, l.Quantity, l.UnitPrice,
       l.Quantity * l.UnitPrice AS LineTotal                                   -- ②
FROM dbo.Orders AS o
INNER JOIN dbo.OrderLines AS l ON l.OrderId = o.Id                             -- ③
ORDER BY o.Id, l.Id;

-- Q3. The same with LEFT JOIN: every order, lines where they exist.
SELECT o.Id AS OrderId, o.CustomerName, l.ProductCode, l.Quantity
FROM dbo.Orders AS o
LEFT JOIN dbo.OrderLines AS l ON l.OrderId = o.Id
ORDER BY o.Id, l.Id;

-- Q4. Orders without any lines.
SELECT o.Id, o.CustomerName
FROM dbo.Orders AS o
LEFT JOIN dbo.OrderLines AS l ON l.OrderId = o.Id
WHERE l.Id IS NULL;                                                            -- ④

-- Q4b. Same question, often clearer: "where no line exists".
SELECT o.Id, o.CustomerName
FROM dbo.Orders AS o
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrderLines AS l WHERE l.OrderId = o.Id);  -- ⑤

-- Q5. Total per order (the SQL version of Order.Total).
SELECT o.Id, o.CustomerName, SUM(l.Quantity * l.UnitPrice) AS Total
FROM dbo.Orders AS o
INNER JOIN dbo.OrderLines AS l ON l.OrderId = o.Id
GROUP BY o.Id, o.CustomerName                                                  -- ⑥
ORDER BY o.Id;

-- Q6. Number of orders and total spent per customer (customers with no lines show 0.00).
SELECT o.CustomerName,
       COUNT(DISTINCT o.Id)                             AS OrderCount,         -- ⑦
       COALESCE(SUM(l.Quantity * l.UnitPrice), 0)       AS TotalSpent          -- ⑧
FROM dbo.Orders AS o
LEFT JOIN dbo.OrderLines AS l ON l.OrderId = o.Id
GROUP BY o.CustomerName
ORDER BY TotalSpent DESC;

-- Q7. Customers who spent more than 100 (HAVING filters groups, after GROUP BY).
SELECT o.CustomerName, SUM(l.Quantity * l.UnitPrice) AS TotalSpent
FROM dbo.Orders AS o
INNER JOIN dbo.OrderLines AS l ON l.OrderId = o.Id
GROUP BY o.CustomerName
HAVING SUM(l.Quantity * l.UnitPrice) > 100;                                    -- ⑨

-- Q8. NULL is not a value: this returns NOTHING, even though order 5 has NULL lines in Q3.
SELECT o.Id
FROM dbo.Orders AS o
LEFT JOIN dbo.OrderLines AS l ON l.OrderId = o.Id
WHERE l.Id = NULL;                                                             -- ⑩