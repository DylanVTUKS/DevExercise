-- Inserts the sample data into the DB

USE IOT_Devices;
GO

IF EXISTS (SELECT 1 FROM dbo.DeviceType)
BEGIN
    PRINT 'Data already exists, seed was not completed.';
END
ELSE
BEGIN
    --Device Types
    INSERT INTO dbo.DeviceType (Name, Description) VALUES
        (N'Asset Tracker',      N'GPS tracker for vehicles'),
        (N'Temperature Sensor', N'Wireless sensor for food safety'),
        (N'Sensor Hub',         N'Groups data from sensors and uploads');

    DECLARE @Tracker INT = (SELECT Id FROM dbo.DeviceType WHERE Name = N'Asset Tracker');
    DECLARE @Temp    INT = (SELECT Id FROM dbo.DeviceType WHERE Name = N'Temperature Sensor');
    DECLARE @Sensor  INT = (SELECT Id FROM dbo.DeviceType WHERE Name = N'Sensor Hub');

    --Firmware
    INSERT INTO dbo.Firmware (DeviceTypeId, Version, ReleaseDate, Notes) VALUES
        (@Tracker, N'1.0.0',   '2026-01-08', N'First release'),
        (@Tracker, N'1.1.0',   '2027-02-09', N'Fixed tracking bug'),
        (@Tracker, N'2.0.0',   '2025-03-10', N'Adds MCU sleep mode'),
        (@Temp,    N'A.0.0.2', '2024-04-20', N'Day one patch'),
        (@Temp,    N'1.2.0',   '2023-05-21', N'Battery life improvements'),
        (@Sensor,  N'3.0.0',   '2022-06-12', N'5G support'),
        (@Sensor,  N'3.1.0',   '2021-07-22', N'Security patch');

    /*Groups
    HardwareComp
    --Warehouse A
        -- Freezer
    --Warehouse B
    --Fleet
        -- TukTuks
    
    */

    INSERT INTO dbo.DeviceGroup (Name, ParentGroupId) VALUES (N'HardwareComp', NULL);
    DECLARE @HardwareComp INT = (SELECT Id FROM dbo.DeviceGroup WHERE Name = N'HardwareComp');

    INSERT INTO dbo.DeviceGroup (Name, ParentGroupId) VALUES
        (N'Warehouse A', @HardwareComp),
        (N'Warehouse B', @HardwareComp),
        (N'Fleet',       @HardwareComp);

    DECLARE @WhA   INT = (SELECT Id FROM dbo.DeviceGroup WHERE Name = N'Warehouse A');
    DECLARE @Fleet INT = (SELECT Id FROM dbo.DeviceGroup WHERE Name = N'Fleet');

    INSERT INTO dbo.DeviceGroup (Name, ParentGroupId) VALUES
        (N'Freezer',      @WhA),
        (N'TukTuks',      @Fleet);

    DECLARE @WhB   INT = (SELECT Id FROM dbo.DeviceGroup WHERE Name = N'Warehouse B');
    DECLARE @Freeze  INT = (SELECT Id FROM dbo.DeviceGroup WHERE Name = N'Freezer');
    DECLARE @TukTuk INT = (SELECT Id FROM dbo.DeviceGroup WHERE Name = N'TukTuks');

    --Firmware IDS
    DECLARE @Tuk100 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Tracker AND Version = N'1.0.0');
    DECLARE @Tuk110 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Tracker AND Version = N'1.1.0');
    DECLARE @Tuk200 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Tracker AND Version = N'2.0.0');
    DECLARE @Tmp100 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Temp    AND Version = N'A.0.0.2');
    DECLARE @Tmp120 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Temp    AND Version = N'1.2.0');
    DECLARE @Hub300 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Sensor  AND Version = N'3.0.0');
    DECLARE @Hub310 INT = (SELECT Id FROM dbo.Firmware WHERE DeviceTypeId = @Sensor  AND Version = N'3.1.0');

    --Devices
    INSERT INTO dbo.Device (SerialNumber, Name, DeviceTypeId, FirmwareId, GroupId) VALUES
        (N'AT-0001', N'TukTuk 1 Tracker',       @Tracker, @Tuk200, @TukTuk ),
        (N'AT-0002', N'TukTuk 2 Tracker',       @Tracker, @Tuk110, @TukTuk ),
        (N'AT-0003', N'Forklift Tracker',       @Tracker, @Tuk100, @WhA    ),
        (N'TS-0001', N'Freezer 1 Sensor',       @Temp,    @Tmp120, @Freeze ),
        (N'TS-0002', N'Freezer 2 Sensor',       @Temp,    @Tmp100, @Freeze ),
        (N'TS-0003', N'Dock Sensor',            @Temp,    @Tmp120, @WhB    ),
        (N'HB-0001', N'Warehouse A Hub',        @Sensor,  @Hub310, @WhA    ),
        (N'HB-0002', N'Warehouse B Hub',        @Sensor,  @Hub300, @WhB    ),
        (N'AT-0004', N'Spare Tracker',          @Tracker, @Tuk200, @HardwareComp);

    PRINT 'Seed data added.';
END
GO


SELECT 'DeviceType'  AS TableName, COUNT(*) AS Rows FROM dbo.DeviceType
UNION ALL SELECT 'Firmware',    COUNT(*) FROM dbo.Firmware
UNION ALL SELECT 'DeviceGroup', COUNT(*) FROM dbo.DeviceGroup
UNION ALL SELECT 'Device',      COUNT(*) FROM dbo.Device;
GO