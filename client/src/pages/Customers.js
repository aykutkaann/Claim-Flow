import React, { useState, useEffect } from 'react';
import api from '../api';

function Customers() {
  const [customers, setCustomers] = useState([]);
  const [tenants, setTenants] = useState([]);
  const [form, setForm] = useState({ fullName: '', email: '', tenantId: '' });
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState(null);

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    try {
      const [custRes, tenRes] = await Promise.all([
        api.get('/api/customers'),
        api.get('/api/tenants'),
      ]);
      setCustomers(custRes.data);
      setTenants(tenRes.data);
    } catch (err) {
      setMessage({ type: 'error', text: 'Failed to load data.' });
    }
    setLoading(false);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setMessage(null);
    try {
      await api.post('/api/customers', form);
      setMessage({ type: 'success', text: 'Customer created successfully!' });
      setForm({ fullName: '', email: '', tenantId: '' });
      loadData();
    } catch (err) {
      const errorMsg =
        err.response?.data?.title || err.response?.data || 'Failed to create customer.';
      setMessage({ type: 'error', text: String(errorMsg) });
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Customers</h2>
        <p>Manage insurance customers</p>
      </div>

      {message && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}

      <div className="two-col">
        <div className="card">
          <h3>Create Customer</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-grid">
              <div className="form-group">
                <label>Full Name</label>
                <input
                  type="text"
                  value={form.fullName}
                  onChange={(e) =>
                    setForm({ ...form, fullName: e.target.value })
                  }
                  placeholder="John Doe"
                  required
                />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input
                  type="email"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  placeholder="john@example.com"
                  required
                />
              </div>
              <div className="form-group">
                <label>Tenant</label>
                <select
                  value={form.tenantId}
                  onChange={(e) =>
                    setForm({ ...form, tenantId: e.target.value })
                  }
                  required
                >
                  <option value="">Select a tenant...</option>
                  {tenants.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name} ({t.code})
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <button type="submit" className="btn btn-primary">
              Create Customer
            </button>
          </form>
        </div>

        <div className="card">
          <h3>All Customers ({customers.length})</h3>
          {loading ? (
            <div className="loading">Loading...</div>
          ) : (
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Full Name</th>
                    <th>Email</th>
                    <th>Tenant ID</th>
                  </tr>
                </thead>
                <tbody>
                  {customers.map((c) => (
                    <tr key={c.id}>
                      <td style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                        {c.id?.substring(0, 8)}...
                      </td>
                      <td>{c.fullName}</td>
                      <td>{c.email}</td>
                      <td style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                        {c.tenantId?.substring(0, 8)}...
                      </td>
                    </tr>
                  ))}
                  {customers.length === 0 && (
                    <tr>
                      <td colSpan="4" style={{ textAlign: 'center', color: '#636e72' }}>
                        No customers found.
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

export default Customers;
