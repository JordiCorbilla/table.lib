# table.lib

Simple `c#` table library that renders any `List<T>` into a nicely formatted `markdown`, `csv`, `html` or `console` table, allowing for extra formats.

## Markdown format

```c#
Table<TestClass>.Add(list).WriteToConsole();
```

```bash
| Field1 | Field2               | Field3        | Field4 | Field5      | Field6 |
| ------ | -------------------- | ------------- | ------ | ----------- | ------ |
| 321121 | Hi 312321            | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 32321  | Hi long text         | 21,111,111.32 | True   | 01-Jan-1970 | 34.43  |
| 321    | Hi longer text       | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very long text    | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very, long text   | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi "very" long  text | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
```

| Field1 | Field2               | Field3        | Field4 | Field5      | Field6 |
| ------ | -------------------- | ------------- | ------ | ----------- | ------ |
| 321121 | Hi 312321            | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 32321  | Hi long text         | 21,111,111.32 | True   | 01-Jan-1970 | 34.43  |
| 321    | Hi longer text       | 2,121.32      | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very long text    | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi very, long text   | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |
| 13     | Hi "very" long  text | 21,111,121.32 | True   | 01-Jan-1970 | 34.43  |

Considerations:

- Any `CRLF` will be automatically transformed into a space ` ` for easy representation of the output

## Dynamic Fields

If the List contains another collection of <strings>, the library is able to scan those and build the resultant dataset giving them a column name called `DynamicX`:

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

## Column Name change

If the name of the column is not of your liking, you can change it via `OverrideColumns` and provide your preferred name. Note that this will also alter the column width to allow for more room if the new name is larger than the previous one.

```c#
Table<IEnumerable<string>>.Add(test).
    OverrideColumns(new Dictionary<string, string> {{"Dynamic0","ColumnA"}}).
    WriteToConsole();
```

```bash
| Capacity | Count | ColumnA  | Dynamic1 | Dynamic2 |
| -------- | ----- | -------- | -------- | -------- |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |

```

| Capacity | Count | ColumnA  | Dynamic1 | Dynamic2 |
| -------- | ----- | -------- | -------- | -------- |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |
| 4        | 3     | AAA      | BBB      | CCC      |


## Column filtering

You don't want to show all the columns? Easy, just use the `FilterColumns` property:

```c#
Table<IEnumerable<string>>.Add(test).
    OverrideColumns(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } }).
    FilterColumns(new Dictionary<string, bool> { { "Capacity", false }, { "Count", false } }).
    WriteToConsole();
```

```bash
| ColumnA  | Dynamic1 | Dynamic2 |
| -------- | -------- | -------- |
| AAA      | BBB      | CCC      |
| AAA      | BBB      | CCC      |
| AAA      | BBB      | CCC      |
| AAA      | BBB      | CCC      |
```

| ColumnA  | Dynamic1 | Dynamic2 |
| -------- | -------- | -------- |
| AAA      | BBB      | CCC      |
| AAA      | BBB      | CCC      |
| AAA      | BBB      | CCC      |
| AAA      | BBB      | CCC      |

## HTML Output

Transform your output into a nicely formatted HTML table

```c#
Table<IEnumerable<string>>.Add(test).
    OverrideColumns(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } }).
    FilterColumns(new Dictionary<string, bool> { { "Capacity", false }, { "Count", false } }).
    WriteToHtml(@"C:\temp\test.html");

Table<TestClass>.Add(list).
    WriteToHtml(@"C:\temp\test-list.html");
```

Sample generated code:

```html
<table style="border-collapse: collapse; width: 100%;">
<tr>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field1</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field2</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field3</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field4</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field5</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field6</th>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">321121</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi 312321</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">2,121.32</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">32321</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi long text</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,111.32</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">321</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi longer text</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">2,121.32</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">13</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi very long text</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,121.32</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">13</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi very, long text</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,121.32</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">13</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi "very" long<br> text</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,121.32</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
</table>
```

Sample output:

<table style="border-collapse: collapse; width: 100%;">
<tr>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field1</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field2</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field3</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field4</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field5</th>
<th style="text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Field6</th>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">321121</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi 312321</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">2,121.32</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">32321</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi long text</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,111.32</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">321</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi longer text</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">2,121.32</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">13</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi very long text</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,121.32</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">13</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi very, long text</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,121.32</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
<tr>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">13</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">Hi "very" long<br> text</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">21,111,121.32</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">True</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">01-Jan-1970</td>
<td style="text-align: right; color: black; background-color: #f2f2f2;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;">34.43</td>
</tr>
</table>

Real HTML output:

![htmlresult](htmlresult.png)

## CSV Output

Trasform your output into a nicely formatted `CSV` file

```c#
Table<TestClass>.Add(list).
    WriteToCsv(@"C:\temp\test-list.csv");
```

The format of the file can be seen here:

```bash
Field1,Field2,Field3,Field4,Field5,Field6
321121,Hi 312321,"2,121.32",True,01-Jan-1970,34.43
32321,Hi long text,"21,111,111.32",True,01-Jan-1970,34.43
321,Hi longer text,"2,121.32",True,01-Jan-1970,34.43
13,Hi very long text,"21,111,121.32",True,01-Jan-1970,34.43
13,"Hi very, long text","21,111,121.32",True,01-Jan-1970,34.43
13,"Hi ""very"" long
 text","21,111,121.32",True,01-Jan-1970,34.43
```

Note that we use the [CSV standard](https://tools.ietf.org/html/rfc4180) when processing `CRLF`, `"` or `,` characters surrouding the value with double quotes. 

![csvoutput](csvoutput.png)
