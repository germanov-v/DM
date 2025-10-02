-- products
DROP TABLE IF EXISTS reference.products;
DROP TABLE IF EXISTS reference.section_brands;
DROP TABLE IF EXISTS reference.brands;
DROP TABLE IF EXISTS reference.brands;
DROP TABLE IF EXISTS reference.sections;
DROP SCHEMA IF EXISTS products;    

    
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