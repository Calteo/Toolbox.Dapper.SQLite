PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "People" (
	"Id"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	"Age"	INTEGER NOT NULL,
	"Description"	TEXT NOT NULL,
	PRIMARY KEY("Id" AUTOINCREMENT)
);
PRAGMA writable_schema=ON;
CREATE TABLE IF NOT EXISTS sqlite_sequence(name,seq);
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('People',0);
PRAGMA writable_schema=OFF;
COMMIT;
