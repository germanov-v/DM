CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS pg_trgm; -- для чего он нужен?


image: postgis/postgis:16-3.4 - обязательный образ для работы с координатами.