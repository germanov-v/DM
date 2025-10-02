
-- crm

DROP TABLE IF EXISTS reference.request_brands;
DROP TABLE IF EXISTS reference.request_sections;
DROP TABLE IF EXISTS profiles.request_files;
DROP TABLE IF EXISTS crm.request_products;
DROP TABLE IF EXISTS crm.request_chats;
DROP TABLE IF EXISTS reference.requests;
DROP SCHEMA IF EXISTS crm;

-- profiles

DROP TABLE IF EXISTS profiles.profile_brands;
DROP TABLE IF EXISTS profiles.profile_sections;
DROP TABLE IF EXISTS profiles.profile_files;

DROP TABLE IF EXISTS profiles.location_brands;
DROP TABLE IF EXISTS profiles.location_sections;
DROP TABLE IF EXISTS profiles.location_products;
DROP TABLE IF EXISTS profiles.file_locations;

DROP TABLE IF EXISTS profiles.portfolio_brands;
DROP TABLE IF EXISTS profiles.portfolio_sections;
DROP TABLE IF EXISTS profiles.portfolio_files;
DROP TABLE IF EXISTS profiles.portfolio_products;
DROP TABLE IF EXISTS profiles.locations;
DROP TABLE IF EXISTS profiles.portfolio;
DROP TABLE IF EXISTS profiles.profiles;
DROP SCHEMA IF EXISTS profiles;

-- products

DROP TABLE IF EXISTS products.section_brands;
DROP TABLE IF EXISTS products.products;
DROP TABLE IF EXISTS products.brands;
DROP TABLE IF EXISTS products.brands;
DROP TABLE IF EXISTS products.sections;
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