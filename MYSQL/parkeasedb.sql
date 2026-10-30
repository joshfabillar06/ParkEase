-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 30, 2026 at 10:05 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `parkeasedb`
--

-- --------------------------------------------------------

--
-- Table structure for table `parkingrates`
--

CREATE TABLE `parkingrates` (
  `RateID` int(11) NOT NULL DEFAULT 1,
  `SedanRate` decimal(10,2) NOT NULL,
  `SuvRate` decimal(10,2) NOT NULL,
  `VanRate` decimal(10,2) NOT NULL,
  `MotorcycleRate` decimal(10,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `parkingrates`
--

INSERT INTO `parkingrates` (`RateID`, `SedanRate`, `SuvRate`, `VanRate`, `MotorcycleRate`) VALUES
(1, 200.00, 75.00, 100.00, 300.00);

-- --------------------------------------------------------

--
-- Table structure for table `parkingslots`
--

CREATE TABLE `parkingslots` (
  `SlotID` int(11) NOT NULL,
  `SlotName` varchar(10) NOT NULL,
  `Status` varchar(20) DEFAULT 'Available',
  `IsArchived` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `parkingslots`
--

INSERT INTO `parkingslots` (`SlotID`, `SlotName`, `Status`, `IsArchived`) VALUES
(1, 'P1', 'Occupied', 0),
(2, 'P2', 'Available', 0),
(3, 'P3', 'Available', 0),
(4, 'P4', 'Available', 0),
(5, 'P5', 'Available', 0),
(6, 'P6', 'Available', 0),
(7, 'P7', 'Available', 0),
(8, 'P8', 'Available', 0),
(9, 'P9', 'Available', 0),
(10, 'P10', 'Available', 0),
(11, 'P11', 'Available', 0),
(12, 'P12', 'Available', 0),
(13, 'P13', 'Available', 0),
(14, 'P14', 'Available', 0),
(15, 'P15', 'Available', 0),
(16, 'P16', 'Available', 0),
(17, 'P17', 'Available', 0),
(18, 'P18', 'Available', 0),
(19, 'P19', 'Available', 0),
(20, 'P20', 'Available', 0),
(21, 'P21', 'Available', 0),
(22, 'P22', 'Available', 0),
(23, 'P23', 'Available', 0),
(24, 'P24', 'Available', 0),
(25, 'P25', 'Available', 0),
(26, 'P26', 'Available', 0),
(27, 'P27', 'Available', 0),
(28, 'P28', 'Available', 0),
(29, 'P29', 'Available', 0),
(30, 'P30', 'Available', 0);

-- --------------------------------------------------------

--
-- Table structure for table `transactions`
--

CREATE TABLE `transactions` (
  `TransactionID` varchar(50) NOT NULL,
  `PlateNumber` varchar(20) NOT NULL,
  `VehicleType` varchar(50) NOT NULL,
  `SlotID` int(11) DEFAULT NULL,
  `EntryTime` datetime NOT NULL,
  `ExitTime` datetime DEFAULT NULL,
  `ParkingDuration` int(11) NOT NULL,
  `HourlyRate` decimal(10,2) NOT NULL,
  `TotalAmount` decimal(10,2) NOT NULL,
  `AmountReceived` decimal(10,2) NOT NULL,
  `ChangeDue` decimal(10,2) NOT NULL,
  `PaymentMethod` varchar(20) NOT NULL,
  `PaymentStatus` varchar(20) DEFAULT 'Paid',
  `ProcessedBy` int(11) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `transactions`
--

INSERT INTO `transactions` (`TransactionID`, `PlateNumber`, `VehicleType`, `SlotID`, `EntryTime`, `ExitTime`, `ParkingDuration`, `HourlyRate`, `TotalAmount`, `AmountReceived`, `ChangeDue`, `PaymentMethod`, `PaymentStatus`, `ProcessedBy`, `CreatedAt`) VALUES
('TKT-0FB8A679', 'GDD 123', 'SUV', 2, '2026-10-30 12:30:53', '2026-10-30 12:30:53', 4, 75.00, 300.00, 400.00, 100.00, 'Cash', 'Paid', 1, '2026-10-30 12:30:53'),
('TKT-23EE79DB', 'ABC 1234', 'SUV', 3, '2026-10-31 03:28:49', '2026-10-31 04:03:25', 1, 75.00, 75.00, 75.00, 0.00, 'Cash', 'Paid', 1, '2026-10-31 03:28:49'),
('TKT-651B7922', 'TIEW 467', 'Motorcycle', 1, '2026-10-27 13:34:16', '2026-10-27 13:34:16', 24, 300.00, 7200.00, 7201.00, 1.00, 'GCash', 'Paid', 1, '2026-10-27 13:34:16'),
('TKT-85BAF08B', 'CND 4485', 'Motorcycle', 1, '2026-10-31 04:25:46', NULL, 0, 300.00, 0.00, 0.00, 0.00, 'Pending', 'Parked', 1, '2026-10-31 04:25:46'),
('TKT-D387FEB4', 'ADC 432', 'Van', 3, '2026-10-30 12:50:59', '2026-10-30 12:52:13', 1, 100.00, 100.00, 200.00, 100.00, 'Cash', 'Paid', 1, '2026-10-30 12:50:59'),
('TKT-DEEE09A9', 'CCD 1244', 'Van', 1, '2026-10-31 04:03:02', '2026-10-31 04:03:52', 1, 100.00, 100.00, 100.00, 0.00, 'Cash', 'Paid', 1, '2026-10-31 04:03:02');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `UserID` int(11) NOT NULL,
  `EmployeeID` varchar(20) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Role` varchar(20) NOT NULL,
  `AccountStatus` varchar(20) DEFAULT 'Active',
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`UserID`, `EmployeeID`, `FullName`, `Username`, `PasswordHash`, `Role`, `AccountStatus`, `CreatedAt`) VALUES
(1, 'EMP-1001', 'System Administrator', 'admin', 'admin123', 'Admin', 'Active', '2026-10-27 00:18:52'),
(2, 'EMP-1042', 'Juan Dela Cruz', 'staff', 'staff123', 'Staff', 'Active', '2026-10-27 00:18:52'),
(3, '2323431', 'Andro Muda', 'mudaks', '1234', 'Staff', 'Active', '2026-10-30 12:09:42');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `parkingrates`
--
ALTER TABLE `parkingrates`
  ADD PRIMARY KEY (`RateID`);

--
-- Indexes for table `parkingslots`
--
ALTER TABLE `parkingslots`
  ADD PRIMARY KEY (`SlotID`),
  ADD UNIQUE KEY `SlotName` (`SlotName`);

--
-- Indexes for table `transactions`
--
ALTER TABLE `transactions`
  ADD PRIMARY KEY (`TransactionID`),
  ADD KEY `SlotID` (`SlotID`),
  ADD KEY `ProcessedBy` (`ProcessedBy`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`UserID`),
  ADD UNIQUE KEY `EmployeeID` (`EmployeeID`),
  ADD UNIQUE KEY `Username` (`Username`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `parkingslots`
--
ALTER TABLE `parkingslots`
  MODIFY `SlotID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=31;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `UserID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `transactions`
--
ALTER TABLE `transactions`
  ADD CONSTRAINT `transactions_ibfk_1` FOREIGN KEY (`SlotID`) REFERENCES `parkingslots` (`SlotID`),
  ADD CONSTRAINT `transactions_ibfk_2` FOREIGN KEY (`ProcessedBy`) REFERENCES `users` (`UserID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
