# table.lib

Simple table library that renders any `List<T>` into a nicely formatted `markdown`, `html` or `console` table

## Markdown format

```c#
Table<TestClass>.Add(list).WriteToConsole();
```

```bash
| Field1 | Field2            | Field3        | Field4 | Field5      | Field6 |
| ------ | ----------------- | ------------- | ------ | ----------- | ------ |
| 321121 | Hi 312321         | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 32321  | Hi long text      | 21,111,111.32 | True   | 01-Jan-1970 | 34.43  |
| 321    | Hi longer text    | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very long text | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
```

| Field1 | Field2            | Field3        | Field4 | Field5      | Field6 |
| ------ | ----------------- | ------------- | ------ | ----------- | ------ |
| 321121 | Hi 312321         | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 32321  | Hi long text      | 21,111,111.32 | True   | 01-Jan-1970 | 34.43  |
| 321    | Hi longer text    | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very long text | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |

## Dynamic Fields

If the List contains another List of strings, the library is able to scan those and build the resultant dataset:

```c#
var test = new List<IEnumerable<string>>
{
    new List<string>() {"AAA", "BBB", "CCC"},
    new List<string>() {"AAA", "BBB", "CCC"},
    new List<string>() {"AAA", "BBB", "CCC"},
    new List<string>() {"AAA", "BBB", "CCC"}
};

Table<IEnumerable<string>>.Add(test).WriteToConsole();
```

```bash
| Capacity | Count | Dynamic0 | Dynamic1 | Dynamic2 |
| -------- | ----- | -------- | -------- | -------- |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
```

| Capacity | Count | Dynamic0 | Dynamic1 | Dynamic2 |
| -------- | ----- | -------- | -------- | -------- |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
