import React, { useState, useEffect } from 'react';
import api from '../api';

function Tenants() {
  const [tenants, setTenants] = useState([]);
  const [form, setForm] = useState({ name: '', code: '' });
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState(null);

  useEffect(() => {
    loadTenants();
  }, []);

  async function loadTenants() {
    setLoading(true);
    try {
      const res = await api.get('/api/tenants');
      setTenants(res.data);
    } catch (err) {
      setMessage({ type: 'error', text: 'Failed to load tenants.' });
    }
    setLoading(false);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setMessage(null);
    try {
      await api.post('/api/tenants', form);
      setMessage({ type: 'success', text: 'Tenant created successfully!' });
      setForm({ name: '', code: '' });
      loadTenants();
    } catch (err) {
      const errorMsg =
        err.response?.data?.title || err.response?.data || 'Failed to create tenant.';
      setMessage({ type: 'error', text: String(errorMsg) });
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Tenants</h2>
        <p>Manage insurance company tenants</p>
      </div>

      {message && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}

      <div className="two-col">
        <div className="card">
          <h3>Create Tenant</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-grid">
              <div className="form-group">
                <label>Name</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="Acme Insurance"
                  required
                />
              </div>
              <div className="form-group">
                <label>Code</label>
                <input
                  type="text"
                  value={form.code}
                  onChange={(e) => setForm({ ...form, code: e.target.value })}
                  placeholder="ACME"
                  required
                />
              </div>
            </div>
            <button type="submit" className="btn btn-primary">
              Create Tenant
            </button>
          </form>
        </div>

        <div className="card">
          <h3>All Tenants ({tenants.length})</h3>
          {loading ? (
            <div className="loading">Loading...</div>
          ) : (
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Code</th>
                  </tr>
                </thead>
                <tbody>
                  {tenants.map((t) => (
                    <tr key={t.id}>
                      <td style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                        {t.id?.substring(0, 8)}...
                      </td>
                      <td>{t.name}</td>
                      <td>
                        <code>{t.code}</code>
                      </td>
                    </tr>
                  ))}
                  {tenants.length === 0 && (
                    <tr>
                      <td colSpan="3" style={{ textAlign: 'center', color: '#636e72' }}>
                        No tenants found.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Tenants;
