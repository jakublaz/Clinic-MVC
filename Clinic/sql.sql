-- DROP TABLE VISITS;
UPDATE AspNetUsers
SET Activated = 0
WHERE UserName = 'alakot';

-- UPDATE AspNetUsers
-- SET Specjalization = 'neurologist'
-- WHERE UserName = 'Doctor1';

-- DELETE FROM AspNetUserRoles
-- WHERE UserId NOT IN (SELECT Id FROM AspNetUsers);

-- DROP TABLE Visits;

-- CREATE TABLE Visits (
--     PatientId TEXT,
--     DoctorId TEXT NOT NULL,
--     Date DATETIME NOT NULL,
--     Description TEXT,
--     PRIMARY KEY (DoctorId, Date)
-- );

-- SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, Specjalization, AspNetUsers.Id FROM AspNetUsers INNER JOIN AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id WHERE AspNetRoles.Name = 'Doctor';
-- INSERT INTO Visits (DoctorId, Date) VALUES (12, '2020-06-12 12:00:00')

-- DELETE FROM Visits
-- WHERE VisitId = 1;

-- SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, AspNetUsers.Id, AspNetUsers.Activated FROM AspNetUsers INNER JOIN AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id WHERE AspNetRoles.Name = 'Patient' AND Activated = 0;

-- UPDATE AspNetUsers SET Activated = 0 WHERE Id = '8e314213-321a-49b3-a2a5-47e0ba25a7c2';

-- SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, PatientId, DoctorId, Date FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId;

-- SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, DoctorId, Date FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId WHERE Date(Date) BETWEEN '2023-11-13' AND '2023-11-17' ORDER BY Date;

-- SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, PatientId, DoctorId, Date FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId WHERE Date > datetime('now') AND (PatientId IS NULL OR PatientId = '') AND Specjalization = 'home' ORDER BY Date;

-- SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, PatientId, DoctorId, Date FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId WHERE Date(Date) = Date('2023-11-24') AND (PatientId IS NOT NULL OR PatientId = '') ORDER BY Date;

-- SELECT 
--     D.Name || ' ' || D.Surname AS DoctorFullName, 
--     P.Name || ' ' || P.Surname AS PatientFullName, 
--     V.PatientId, 
--     V.DoctorId, 
--     V.Date, 
--     V.Description
-- FROM 
--     Visits V
-- INNER JOIN 
--     AspNetUsers D ON V.DoctorId = D.Id
-- LEFT JOIN 
--     AspNetUsers P ON V.PatientId = P.Id
-- WHERE 
--     Date(V.Date) = Date('2023-11-23') 
--     AND (V.PatientId IS NOT NULL OR V.PatientId = '')
--     AND (V.Description IS NULL OR V.Description = '')
-- ORDER BY V.Date;
