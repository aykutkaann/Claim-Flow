import React, { useState } from 'react';
import api from '../api';

function Reports() {
  const [activeTab, setActiveTab] = useState('loss-ratios');
  const [data, setData] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState(null);

  async function loadReport(tab) {
    setActiveTab(tab);
    setData(null);
    setMessage(null);

    if (tab === 'search') return;

    setLoading(true);
    const endpoints = {
      'loss-ratios': '/api/reports/loss-ratios',
      'rolling-stats': '/api/reports/rolling-stats',
      'agent-performance': '/api/reports/agent-performance',
    };

    try {
      const res = await api.get(endpoints[tab]);
      setData(res.data);
    } catch (err) {
      setMessage({ type: 'error', text: 'Failed to load report.' });
    }
    setLoading(false);
  }

  async function handleSearch(e) {
    e.preventDefault();
    if (!searchTerm.trim()) return;
    setLoading(true);
    setMessage(null);
    setSearchResults(null);
    try {
      const res = await api.get(
        `/api/reports/search?q=${encodeURIComponent(searchTerm)}`
      );
      setSearchResults(res.data);
    } catch (err) {
      setMessage({ type: 'error', text: 'Search failed.' });
    }
    setLoading(false);
  }

  function renderTable() {
    if (!data) return null;

    // If data is an array, render it as a table
    if (Array.isArray(data)) {
      if (data.length === 0) {
        return <p style={{ color: '#636e72' }}>No data available.</p>;
      }
      const keys = Object.keys(data[0]);
      return (
        <div className="table-container">
          <table>
            <thead>
              <tr>
                {keys.map((k) => (
                  <th key={k}>{formatHeader(k)}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data.map((row, i) => (
                <tr key={i}>
                  {keys.map((k) => (
                    <td key={k}>{formatValue(row[k])}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      );
    }

    // If data is an object, render key-value pairs
    if (typeof data === 'object') {
      const entries = Object.entries(data);
      // Check if any value is an array
      const arrayEntries = entries.filter(([, v]) => Array.isArray(v));
      const scalarEntries = entries.filter(([, v]) => !Array.isArray(v) && typeof v !== 'object');

      return (
        <div>
          {scalarEntries.length > 0 && (
            <div className="stats-grid" style={{ marginBottom: 20 }}>
              {scalarEntries.map(([key, val]) => (
                <div key={key} className="stat-card">
                  <span className="stat-label">{formatHeader(key)}</span>
                  <span className="stat-value">{formatValue(val)}</span>
                </div>
              ))}
            </div>
          )}
          {arrayEntries.map(([key, arr]) => (
            <div key={key} style={{ marginBottom: 20 }}>
              <h4 style={{ marginBottom: 8 }}>{formatHeader(key)}</h4>
              {arr.length > 0 ? (
                <div className="table-container">
                  <table>
                    <thead>
                      <tr>
                        {Object.keys(arr[0]).map((k) => (
                          <th key={k}>{formatHeader(k)}</th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {arr.map((row, i) => (
                        <tr key={i}>
                          {Object.keys(arr[0]).map((k) => (
                            <td key={k}>{formatValue(row[k])}</td>
                          ))}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p style={{ color: '#636e72' }}>No data.</p>
              )}
            </div>
          ))}
        </div>
      );
    }

    return <pre>{JSON.stringify(data, null, 2)}</pre>;
  }

  function renderSearchResults() {
    if (!searchResults) return null;

    if (Array.isArray(searchResults)) {
      if (searchResults.length === 0) {
        return <p style={{ color: '#636e72' }}>No results found.</p>;
      }
      const keys = Object.keys(searchResults[0]);
      return (
        <div className="table-container">
          <table>
            <thead>
              <tr>
                {keys.map((k) => (
                  <th key={k}>{formatHeader(k)}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {searchResults.map((row, i) => (
                <tr key={i}>
                  {keys.map((k) => (
                    <td key={k}>{formatValue(row[k])}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      );
    }

    if (typeof searchResults === 'object') {
      return <pre>{JSON.stringify(searchResults, null, 2)}</pre>;
    }

    return <p>{String(searchResults)}</p>;
  }

  function formatHeader(key) {
    return key
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (s) => s.toUpperCase())
      .trim();
  }

  function formatValue(val) {
    if (val === null || val === undefined) return '-';
    if (typeof val === 'number') {
      if (Number.isInteger(val)) return val.toLocaleString();
      return val.toFixed(2);
    }
    if (typeof val === 'boolean') return val ? 'Yes' : 'No';
    if (typeof val === 'object') return JSON.stringify(val);
    return String(val);
  }

  return (
    <div>
      <div className="page-header">
        <h2>Reports</h2>
        <p>View analytics and search claims data</p>
      </div>

      {message && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}

      <div className="tabs">
        <button
          className={`tab ${activeTab === 'loss-ratios' ? 'active' : ''}`}
          onClick={() => loadReport('loss-ratios')}
        >
          Loss Ratios
        </button>
        <button
          className={`tab ${activeTab === 'rolling-stats' ? 'active' : ''}`}
          onClick={() => loadReport('rolling-stats')}
        >
          Rolling Stats
        </button>
        <button
          className={`tab ${activeTab === 'agent-performance' ? 'active' : ''}`}
          onClick={() => loadReport('agent-performance')}
        >
          Agent Performance
        </button>
        <button
          className={`tab ${activeTab === 'search' ? 'active' : ''}`}
          onClick={() => {
            setActiveTab('search');
            setData(null);
          }}
        >
          Search
        </button>
      </div>

      <div className="card">
        {activeTab === 'search' ? (
          <div>
            <h3>Full-Text Search</h3>
            <form onSubmit={handleSearch}>
              <div className="search-bar">
                <input
                  type="text"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder="Search claims, policies, customers..."
                />
                <button type="submit" className="btn btn-primary">
                  Search
                </button>
              </div>
            </form>
            {loading && <div className="loading">Searching...</div>}
            {renderSearchResults()}
          </div>
        ) : (
          <div>
            <h3>
              {activeTab === 'loss-ratios' && 'Loss Ratios'}
              {activeTab === 'rolling-stats' && 'Rolling Stats'}
              {activeTab === 'agent-performance' && 'Agent Performance'}
            </h3>
            {!data && !loading && (
              <p style={{ color: '#636e72' }}>
                Click a tab above to load the report.
              </p>
            )}
            {loading && <div className="loading">Loading report...</div>}
            {renderTable()}
          </div>
        )}
      </div>
    </div>
  );
}

export default Reports;
