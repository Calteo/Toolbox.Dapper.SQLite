-- SQLite database dump
-- Generated: 2026-09-10T20:49:34.6021494Z

PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;

--
-- Schema
--

-- table: Children
CREATE TABLE "Children" (
	"Id"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	"DemoId"	INTEGER NOT NULL,
	PRIMARY KEY("Id" AUTOINCREMENT),
	CONSTRAINT "Children_Demo" FOREIGN KEY("DemoId") REFERENCES "Demo"("Id") ON DELETE CASCADE
);

-- table: Demo
CREATE TABLE "Demo" (
	"Id"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	PRIMARY KEY("Id" AUTOINCREMENT)
);

--
-- Data
--

-- Data for table Children
INSERT INTO "Children" VALUES (1, 'Child1', 1);
INSERT INTO "Children" VALUES (2, 'Child2', 1);

-- Data for table Demo
INSERT INTO "Demo" VALUES (1, 'Hello');


COMMIT;
