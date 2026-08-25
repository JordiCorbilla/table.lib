using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using System;
using System.Collections.Generic;
using System.IO;

namespace table.lib.benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class RenderingBenchmarks
    {
        private List<BenchmarkRow> _rows;
        private Dictionary<int, BenchmarkRow> _dictionary;
        private List<IDictionary<string, object>> _dbRows;
        private Table<BenchmarkRow> _table;
        private TableDic<int, BenchmarkRow> _dictionaryTable;
        private DbTable _dbTable;

        [Params(10, 1_000, 10_000)]
        public int RowCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _rows = new List<BenchmarkRow>(RowCount);
            _dictionary = new Dictionary<int, BenchmarkRow>(RowCount);
            _dbRows = new List<IDictionary<string, object>>(RowCount);

            for (var index = 0; index < RowCount; index++)
            {
                var row = new BenchmarkRow
                {
                    Id = index,
                    Name = $"Customer {index}",
                    Description = $"Order {index}, generated for benchmark coverage",
                    Amount = 1234.56m + index,
                    Ratio = 12.34 + index,
                    Active = index % 2 == 0,
                    Created = new DateTime(2026, 1, 1).AddMinutes(index),
                    Category = index % 5
                };
                _rows.Add(row);
                _dictionary.Add(index, row);
                _dbRows.Add(new Dictionary<string, object>
                {
                    ["Id"] = row.Id,
                    ["Name"] = row.Name,
                    ["Description"] = row.Description,
                    ["Amount"] = row.Amount,
                    ["Ratio"] = row.Ratio,
                    ["Active"] = row.Active,
                    ["Created"] = row.Created,
                    ["Category"] = row.Category
                });
            }

            _table = Table<BenchmarkRow>.Add(_rows);
            _dictionaryTable = TableDic<int, BenchmarkRow>.Add(_dictionary);
            _dbTable = DbTable.Add(_dbRows);
        }

        [Benchmark] public string Markdown() => _table.ToMarkDown();
        [Benchmark] public string Csv() => _table.ToCsv();
        [Benchmark] public string Html() => _table.ToHtml();
        [Benchmark] public string Sql() => _table.ToSqlInsertString();
        [Benchmark] public string DictionaryCsv() => _dictionaryTable.ToCsv();
        [Benchmark] public string DbSql() => _dbTable.ToSqlInsertString();

#if !BASELINE
        [Benchmark]
        public void CsvStreaming()
        {
            _table.WriteCsv(TextWriter.Null);
        }
#endif
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(Job.ShortRun.WithToolchain(InProcessNoEmitToolchain.Instance));
        }
    }

    public class BenchmarkRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public double Ratio { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public int Category { get; set; }
    }
}
