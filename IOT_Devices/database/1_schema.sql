/*
Creates the IOT_Devices database and the tables. 
When re run the tables are dropped and re created. 
*/

IF DB_ID('IOT_Devices') IS NULL     --Create if it doesnt exist
    CREATE DATABASE IOT_Devices;
GO

USE IOT_Devices;
GO

-- Drop tables to make from scratch 
IF OBJECT_ID('dbo.Device', 'U')       IS NOT NULL DROP TABLE dbo.Device;
IF OBJECT_ID('dbo.Firmware', 'U')     IS NOT NULL DROP TABLE dbo.Firmware;
IF OBJECT_ID('dbo.DeviceGroup', 'U')  IS NOT NULL DROP TABLE dbo.DeviceGroup;
IF OBJECT_ID('dbo.DeviceType', 'U')   IS NOT NULL DROP TABLE dbo.DeviceType;
GO


-- DeviceType, the type of hardware 
CREATE TABLE dbo.DeviceType (
    Id              INT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(100)     NOT NULL,
    Description     NVARCHAR(500)     NULL,
    CONSTRAINT      PK_DeviceType      PRIMARY KEY (Id),
    CONSTRAINT      UQ_DeviceType_Name UNIQUE (Name)
);
GO

-- Firmware, the software for one device type. 
CREATE TABLE dbo.Firmware (
    Id            INT IDENTITY(1,1) NOT NULL,
    DeviceTypeId  INT               NOT NULL,   
    Version       NVARCHAR(50)      NOT NULL,
    ReleaseDate   DATE              NOT NULL,
    Notes         NVARCHAR(500)     NULL,
    CONSTRAINT PK_Firmware PRIMARY KEY (Id),
    CONSTRAINT FK_Firmware_DeviceType
        FOREIGN KEY (DeviceTypeId) REFERENCES dbo.DeviceType (Id),
    CONSTRAINT UQ_Firmware_DeviceType_Version UNIQUE (DeviceTypeId, Version) --One deviceType cannot have duplicate firmware versions
);
GO
-- DeviceGroup
CREATE TABLE dbo.DeviceGroup (
    Id             INT IDENTITY(1,1) NOT NULL,
    Name           NVARCHAR(100)     NOT NULL,
    ParentGroupId  INT               NULL,
    CONSTRAINT PK_DeviceGroup PRIMARY KEY (Id),
    CONSTRAINT FK_DeviceGroup_Parent
        FOREIGN KEY (ParentGroupId) REFERENCES dbo.DeviceGroup (Id), 
    CONSTRAINT CK_DeviceGroup_NotOwnParent --Group cant be its own parent
        CHECK (ParentGroupId IS NULL OR ParentGroupId <> Id),
    CONSTRAINT UQ_DeviceGroup_Parent_Name UNIQUE (ParentGroupId, Name)
);
GO

-- Device, physical unit
CREATE TABLE dbo.Device (
    Id            INT IDENTITY(1,1) NOT NULL,
    SerialNumber  NVARCHAR(50)      NOT NULL,
    Name          NVARCHAR(100)     NOT NULL,
    DeviceTypeId  INT               NOT NULL,
    FirmwareId    INT               NOT NULL,
    GroupId       INT               NULL,
    CreatedAt     DATETIME2         NOT NULL CONSTRAINT DF_Device_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Device PRIMARY KEY (Id),
    CONSTRAINT UQ_Device_SerialNumber UNIQUE (SerialNumber),
    CONSTRAINT FK_Device_DeviceType
        FOREIGN KEY (DeviceTypeId) REFERENCES dbo.DeviceType (Id),
    CONSTRAINT FK_Device_Firmware
        FOREIGN KEY (FirmwareId) REFERENCES dbo.Firmware (Id),
    CONSTRAINT FK_Device_Group
        FOREIGN KEY (GroupId) REFERENCES dbo.DeviceGroup (Id)
);
GO


--Indexes on foreign keys used often. 
CREATE INDEX IX_Firmware_DeviceTypeId ON dbo.Firmware (DeviceTypeId);
CREATE INDEX IX_Device_DeviceTypeId   ON dbo.Device (DeviceTypeId);
CREATE INDEX IX_Device_FirmwareId     ON dbo.Device (FirmwareId);
CREATE INDEX IX_Device_GroupId        ON dbo.Device (GroupId);
GO