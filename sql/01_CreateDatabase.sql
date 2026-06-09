-- ============================================================
-- MES 生产执行系统 — 01 创建数据库
-- 执行顺序：本文件最先执行
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MES')
BEGIN
    CREATE DATABASE MES
    COLLATE Chinese_PRC_CI_AS;       -- 支持中文排序
    PRINT '数据库 MES 创建成功';
END
ELSE
BEGIN
    PRINT '数据库 MES 已存在，跳过创建';
END
GO

USE MES;
GO
PRINT '已切换到 MES 数据库';
GO
