-- DROP TABLE chats.messages;
-- DROP TABLE chats.lessons_parts;
-- DROP TABLE chats.practices;
-- DROP TABLE chats.participants;
-- DROP TABLE chats.chats;
-- DROP SCHEMA chats;
-- -- student
-- 
-- DROP TABLE students.subscribes;
-- DROP TABLE students.services;
-- DROP TABLE students.students;
-- DROP SCHEMA students;
-- 
-- -- practices
-- 
-- DROP TABLE practices.lessons_data;
-- DROP TABLE practices.subjects;
-- DROP TABLE practices.options;
-- DROP TABLE practices.practices;
-- DROP TABLE practices.languages;
-- DROP SCHEMA practices;
-- 
-- 
-- -- courses
-- DROP TABLE courses.courses_subjects;
-- DROP TABLE courses.courses_tracks;
-- DROP TABLE courses.lessons_subjects;
-- DROP TABLE courses.modules_subjects;
-- DROP TABLE courses.data;
-- DROP TABLE courses.lessons_parts;
-- DROP TABLE courses.lessons;
-- DROP TABLE courses.modules;
-- DROP TABLE courses.courses;
-- DROP SCHEMA courses;
-- 
-- 
-- -- tracks
-- 
-- DROP TABLE tracks.subjects;
-- DROP TABLE tracks.tracks;
-- DROP SCHEMA tracks;
-- 
-- 
-- DROP TABLE tags.tags;
-- DROP SCHEMA tags;
-- 
-- DROP TABLE reference.levels;
-- DROP SCHEMA reference;
    

    
-- reference
-- DROP TABLE reference.profile_types;
-- DROP TABLE reference.styles;
DROP TABLE IF EXISTS reference.geo_base_data;
DROP TABLE IF EXISTS reference.geo_city;
DROP TABLE IF EXISTS reference.geo_region;
DROP TABLE IF EXISTS reference.geo_country;
DROP SCHEMA IF EXISTS reference;


-- chats
DROP TABLE IF EXISTS chats.message_files;
DROP TABLE IF EXISTS chats.messages;
DROP TABLE IF EXISTS chats.participants;
DROP TABLE IF EXISTS chats.chats;
DROP SCHEMA IF EXISTS chats;
    
-- identity

DROP TABLE IF EXISTS identity.users_roles;
DROP TABLE IF EXISTS identity.tg_users;
DROP TABLE IF EXISTS identity.users_email;
DROP TABLE IF EXISTS identity.users_phone;
DROP TABLE IF EXISTS identity.external_providers;
DROP TABLE IF EXISTS identity.web_anonymous;
DROP TABLE IF EXISTS identity.ios_install;
DROP TABLE IF EXISTS identity.sessions;
DROP TABLE  IF EXISTS identity.users;
DROP TABLE  IF EXISTS identity.roles;
DROP SCHEMA IF EXISTS identity;

-- files

DROP TABLE IF EXISTS  files.files;
DROP SCHEMA IF EXISTS  files;


-- telegram
DROP TABLE IF EXISTS  telegram.accounts_bots;
DROP TABLE IF EXISTS  telegram.accounts;
DROP SCHEMA IF EXISTS  telegram;