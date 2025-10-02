Хороший вопрос. Коротко:

* У тебя **PK (portfolio_id, product_id)** — это btree-индекс слева направо.
  ➜ Любые запросы `WHERE portfolio_id = ?` (и даже с `AND product_id IN (...)`) уже эффективно используют **этот** индекс. Отдельный индекс по `portfolio_id` **не нужен**.

* Но по правилу «левого префикса» **нельзя** использовать этот PK для фильтрации только по `product_id`.
  ➜ Поэтому и создают **дополнительный индекс по `product_id`**, чтобы ускорять запросы/джоины вида:

  ```sql
  -- найти все портфолио, в которых встречается товар X
  SELECT p.*
  FROM profiles.portfolio_products pp
  JOIN profiles.portfolio p ON p.id = pp.portfolio_id
  WHERE pp.product_id = $1;
  ```

Это не про то, что «`product_id` встречается чаще», а про **направление ключа индекса**.

---

### Когда достаточно `ON (product_id)`

* Большинство запросов начинается «от продукта к портфолио»: `WHERE product_id = ?`.
* Не нужно покрывать сортировки/диапазоны по второй колонке.

### Когда лучше сделать композитный «в обратном порядке»

Вместо одиночного индекса можно создать:

```sql
CREATE INDEX IF NOT EXISTS idx_portfolio_products_product_portfolio
  ON profiles.portfolio_products (product_id, portfolio_id);
```

Плюсы:

* Плану не придётся обращаться к таблице за `portfolio_id` — он уже в индексе (index-only scan чаще возможен).
* Полезно при частых джоинах `products → portfolio_products → portfolio`.

Минусы:

* На пару байт больше, чем одиночный по `product_id`.

---

### Итого (практически)

Текущий набор оптимален:

* **PK (portfolio_id, product_id)** — закрывает все запросы по `portfolio_id`.
* **Доп. индекс по `product_id`** — закрывает обратное направление.

Хочешь чуть лучше — замени одиночный на композитный «(product_id, portfolio_id)».
