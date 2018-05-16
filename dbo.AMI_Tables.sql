CREATE TABLE [dbo].[AMI_Tables] (
    [Agregator_Code] NCHAR (20)     NOT NULL,
    [Voltage]        DECIMAL (4, 2) NOT NULL,
    [Current]        DECIMAL (4, 2) NOT NULL,
    [ActivePower]    DECIMAL (4, 2) NOT NULL,
    [ReactivePower]  DECIMAL (4, 2) NOT NULL,
    [DateAndTime]       DATETIME       NOT NULL
);

