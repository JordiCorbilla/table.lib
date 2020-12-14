# table.lib

Simple table library that renders any `List<T>` into a nicely formatted `markdown` table

## Markdown format

```c#
Table<TestClass>.Add(list).WriteToConsole();
```

| Field1 | Field2            | Field3        | Field4 | Field5      | Field6 |
| ------ | ----------------- | ------------- | ------ | ----------- | ------ |
| 321121 | Hi 312321         | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 32321  | Hi long text      | 21,111,111.32 | True   | 01-Jan-1970 | 34.43  |
| 321    | Hi longer text    | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very long text | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
