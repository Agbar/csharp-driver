﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using CqlPoco.Mapping;
using CqlPoco.Statements;

namespace CqlPoco
{
    /// <summary>
    /// The default CQL client implementation which uses the DataStax driver ISession provided in the constructor
    /// for running queries against a Cassandra cluster.
    /// </summary>
    internal class CqlClient : ICqlClient
    {
        private readonly ISession _session;
        private readonly MapperFactory _mapperFactory;
        private readonly StatementFactory _statementFactory;

        public CqlClient(ISession session, MapperFactory mapperFactory, StatementFactory statementFactory)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (mapperFactory == null) throw new ArgumentNullException("mapperFactory");
            if (statementFactory == null) throw new ArgumentNullException("statementFactory");

            _session = session;
            _mapperFactory = mapperFactory;
            _statementFactory = statementFactory;
        }

        public Task<List<T>> Fetch<T>()
        {
            // Just pass an empty string for the CQL and let if be auto-generated
            return Fetch<T>(string.Empty);
        }

        public async Task<List<T>> Fetch<T>(string cql, params object[] args)
        {
            // Get the statement to execute and execute it
            IStatementWrapper statement = await _statementFactory.GetSelect<T>(cql).ConfigureAwait(false);
            RowSet rows = await _session.ExecuteAsync(statement.Bind(args)).ConfigureAwait(false);

            // Map to return type
            Func<Row, T> mapper = _mapperFactory.GetMapper<T>(statement, rows);
            return rows.Select(mapper).ToList();
        }
        
        public async Task<T> Single<T>(string cql, params object[] args)
        {
            // Get statement to execute, execute and get single row
            IStatementWrapper statement = await _statementFactory.GetSelect<T>(cql).ConfigureAwait(false);
            RowSet rows = await _session.ExecuteAsync(statement.Bind(args)).ConfigureAwait(false);
            Row row = rows.Single();

            // Map to return type
            Func<Row, T> mapper = _mapperFactory.GetMapper<T>(statement, rows);
            return mapper(row);
        }
        
        public async Task<T> SingleOrDefault<T>(string cql, params object[] args)
        {
            // Get statement to execute, execute and get single row or default
            IStatementWrapper statement = await _statementFactory.GetSelect<T>(cql).ConfigureAwait(false);
            RowSet rows = await _session.ExecuteAsync(statement.Bind(args)).ConfigureAwait(false);
            Row row = rows.SingleOrDefault();

            // Map to return type or return default
            if (row == null) 
                return default(T);

            Func<Row, T> mapper = _mapperFactory.GetMapper<T>(statement, rows);
            return mapper(row);
        }

        public async Task<T> First<T>(string cql, params object[] args)
        {
            // Get statement to execute, execute and get first row
            IStatementWrapper statement = await _statementFactory.GetSelect<T>(cql).ConfigureAwait(false);
            RowSet rows = await _session.ExecuteAsync(statement.Bind(args)).ConfigureAwait(false);
            Row row = rows.First();

            // Map to return type
            Func<Row, T> mapper = _mapperFactory.GetMapper<T>(statement, rows);
            return mapper(row);
        }

        public async Task<T> FirstOrDefault<T>(string cql, params object[] args)
        {
            // Get statement to execute, execute and get first row
            IStatementWrapper statement = await _statementFactory.GetSelect<T>(cql).ConfigureAwait(false);
            RowSet rows = await _session.ExecuteAsync(statement.Bind(args)).ConfigureAwait(false);
            Row row = rows.FirstOrDefault();

            // Map to return type or return default
            if (row == null)
                return default(T);

            Func<Row, T> mapper = _mapperFactory.GetMapper<T>(statement, rows);
            return mapper(row);
        }

        public async Task Insert<T>(T poco)
        {
            // Get statement and bind values from POCO
            IStatementWrapper statement = await _statementFactory.GetInsert<T>().ConfigureAwait(false);
            Func<T, object[]> getBindValues = _mapperFactory.GetValueCollector<T>(statement);
            object[] values = getBindValues(poco);

            // Execute the statement
            await _session.ExecuteAsync(statement.Bind(values)).ConfigureAwait(false);
        }

        public async Task Update<T>(T poco)
        {
            // Get statement and bind values from POCO
            IStatementWrapper statement = await _statementFactory.GetUpdate<T>().ConfigureAwait(false);
            Func<T, object[]> getBindValues = _mapperFactory.GetValueCollector<T>(statement);
            object[] values = getBindValues(poco);

            // Execute
            await _session.ExecuteAsync(statement.Bind(values)).ConfigureAwait(false);
        }

        public async Task Delete<T>(T poco)
        {
            // Get the statement and bind values from POCO
            IStatementWrapper statement = await _statementFactory.GetDelete<T>().ConfigureAwait(false);
            Func<T, object[]> getBindValues = _mapperFactory.GetValueCollector<T>(statement, primaryKeyValuesOnly: true);
            object[] values = getBindValues(poco);

            // Execute
            await _session.ExecuteAsync(statement.Bind(values)).ConfigureAwait(false);
        }
    }
}