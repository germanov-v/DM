В PostgreSQL есть два варианта, как хранить широту/долготу:

---

### 1. **Простейший способ — отдельные колонки `latitude` и `longitude`**

```sql
latitude  double precision NOT NULL, -- широта
longitude double precision NOT NULL  -- долгота
```

* Используется чаще всего, если нужна только точка и простые выборки.
* Можно повесить CHECK-ограничения для валидации:

```sql
latitude  double precision NOT NULL CHECK (latitude BETWEEN -90  AND 90),
longitude double precision NOT NULL CHECK (longitude BETWEEN -180 AND 180)
```

---

### 2. **Использовать расширение PostGIS**

Если планируешь гео-запросы (поиск в радиусе, пересечение с полигонами, гео-индексы), то лучше установить PostGIS:

```sql
CREATE EXTENSION IF NOT EXISTS postgis;
```

И вместо пары чисел хранить одну колонку:

```sql
location geography(Point, 4326) NOT NULL
```

* `Point` — тип точки.
* `4326` — стандартная система координат WGS84 (GPS).
* Заполняется так:

  ```sql
  INSERT INTO places(name, location)
  VALUES ('Moscow', ST_SetSRID(ST_MakePoint(37.6173, 55.7558), 4326));
  -- порядок: longitude, latitude!
  ```

Индекс для быстрых гео-запросов:

```sql
CREATE INDEX idx_places_location ON places USING GIST (location);
```

---

✅ Если нужна просто «храню широту/долготу и иногда показываю на карте» → подойдут **два `double precision` с CHECK**.
✅ Если нужна гео-логика («найди все объекты в радиусе 10 км от точки», «попадает ли в область» и т.п.) → ставь **PostGIS** и используй `geography(Point, 4326)`.

